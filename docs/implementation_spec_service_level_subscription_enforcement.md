# Implementation Spec: Service-Level Subscription Enforcement

## Objective

Every product service must enforce subscription entitlements at the **service boundary and command/query handling layer**, not only at the API gateway.

This spec defines the standard pattern all services in the platform should follow so that subscription restrictions are consistent, auditable, and not bypassable by alternate routes, internal calls, background jobs, or future API additions.

---

## Why This Exists

Gateway-only enforcement is not enough.

If entitlement checks live only in the API gateway, then a restricted operation may still be executed through:

- internal service-to-service calls
- asynchronous job processors
- event consumers
- new routes that are added later but not registered in gateway config
- direct access paths in test/dev tooling
- public controllers that were missed during route registration

That means **every service must enforce its own subscription gates** for premium or plan-limited actions.

The gateway may provide an additional coarse filter, but the backend service remains the final authority for whether an operation is allowed.

---

## Core Rule

For any feature limited by plan, add enforcement in **both** places:

1. **Gateway level** for route-level blocking where practical
2. **Service level** for authoritative execution control

If only one exists, the implementation is incomplete.

---

## Standard Architecture

### Billing Service Responsibilities

Billing is the source of truth for:

- tenant subscription status
- active plan
- feature entitlements
- plan overrides / manual grants
- effective entitlement resolution

Billing should expose a stable entitlement resolution contract that other services can consume.

Examples:

- `travel.quotations.create`
- `travel.bookings.manage`
- `communication.campaigns.send`
- `identity.branding.advanced`
- `crm.contacts.export`

Billing should return the **effective entitlement state**, not force downstream services to reimplement plan logic.

---

### Gateway Responsibilities

Gateway should:

- map protected routes to entitlement keys
- reject obviously unauthorized requests early
- propagate tenant context consistently
- optionally cache entitlement lookups for efficiency

Gateway enforcement is a convenience and performance layer.

It is **not** the final enforcement point.

---

### Product Service Responsibilities

Each product service must:

- resolve tenant context before protected operations
- check entitlement before executing premium commands or queries
- fail fast before aggregate creation or persistence
- return a consistent forbidden / feature-disabled result
- avoid embedding plan names directly in business logic
- depend on entitlement keys, not hardcoded plan comparisons

Services must assume requests may arrive from paths other than the gateway.

---

## Required Service-Level Pattern

Every service with monetized or restricted functionality must implement the following building blocks.

### 1. Entitlement Abstraction

Each service should depend on an application abstraction such as:

```csharp
public interface IFeatureGate
{
    Task EnsureEnabledAsync(Guid tenantId, string featureKey, CancellationToken cancellationToken = default);
    Task<bool> IsEnabledAsync(Guid tenantId, string featureKey, CancellationToken cancellationToken = default);
}
```

Or a service-specific equivalent:

```csharp
public interface ITravelFeatureGate
{
    Task EnsureQuotationCreationEnabledAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
```

Prefer the shared generic pattern unless there is a strong reason not to.

---

### 2. Consistent Entitlement Keys

Every restricted action should have an explicit key.

Examples:

- `travel.quotations.create`
- `travel.quotations.approve`
- `travel.bookings.fulfillment`
- `travel.reports.export`
- `communication.templates.branding`
- `identity.portal.custom-branding`

Rules:

- use dot-delimited keys
- keep names capability-based
- do not tie names to specific plan names
- use the same keys in billing, gateway, service handlers, docs, and postman notes

---

### 3. Enforcement at Handler Entry

Protected commands and queries must validate entitlements at the start of the handler.

Example:

```csharp
public sealed class CreateQuotationCommandHandler : IRequestHandler<CreateQuotationCommand, Guid>
{
    private readonly IFeatureGate _featureGate;
    private readonly IQuotationRepository _quotationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateQuotationCommandHandler(
        IFeatureGate featureGate,
        IQuotationRepository quotationRepository,
        IUnitOfWork unitOfWork)
    {
        _featureGate = featureGate;
        _quotationRepository = quotationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(CreateQuotationCommand request, CancellationToken cancellationToken)
    {
        await _featureGate.EnsureEnabledAsync(request.TenantId, "travel.quotations.create", cancellationToken);

        var quotation = Quotation.Create(...);
        await _quotationRepository.AddAsync(quotation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return quotation.Id;
    }
}
```

This check must happen **before** persistence and before side effects.

---

### 4. Enforcement in Background and Async Flows

Do not assume background work is safe just because the originating API was checked.

Repeat enforcement where restricted work is executed in:

- message consumers
- workflow processors
- scheduled jobs
- document generation jobs
- bulk export pipelines
- automation runners

If the business action is restricted, the executor must check again.

---

### 5. Consistent Failure Contract

When entitlement is missing, services should return a consistent failure.

Recommended behavior:

- HTTP `403 Forbidden` for denied features
- machine-readable error code, for example:
  - `feature_not_enabled`
  - `subscription_required`
  - `entitlement_denied`
- include the required feature key where safe

Example response:

```json
{
  "code": "entitlement_denied",
  "message": "This feature is not enabled for the current tenant.",
  "feature": "travel.quotations.create"
}
```

Do not return vague validation errors for entitlement failures.

---

## Where Checks Must Exist

Every service should review operations in these categories.

### Write Operations

Must be checked for entitlement when monetized:

- create
- update
- delete
- approve
- publish
- export
- assign
- fulfill
- send
- generate
- share publicly

### Sensitive Read Operations

May also require checks when plan-limited:

- advanced reports
- analytics dashboards
- exports
- premium search modes
- audit history access
- public portal customization views

### Public / Tokenized Endpoints

Public endpoints must still validate whether the tenant is entitled to the feature behind them.

Examples:

- public quote acceptance
- branded portals
- share-link actions
- customer upload links

A public token is not a subscription bypass.

---

## Enforcement Layers by Service Type

### Identity Service

Must enforce for features such as:

- tenant branding
- theme overrides
- custom templates
- advanced portal customization
- multiple branding scopes if plan-limited

### Travel Service

Must enforce for features such as:

- quotation creation
- approval workflows
- booking fulfillment
- exports and reporting
- public quotation actions if plan-limited
- change request workflows

### Communication Service

Must enforce for features such as:

- campaign sending
- advanced templating
- branded email rendering
- automation or drip features
- attachments or bulk sends if restricted

### CRM / Other Product Services

Must enforce for features such as:

- advanced pipeline stages
- export tools
- automation
- integrations
- custom fields / advanced segmentation

---

## Anti-Patterns

Do **not** do these:

### 1. Hardcoding Plan Names in Handlers

Bad:

```csharp
if (tenant.Plan != "Pro")
```

Why bad:

- plan logic spreads everywhere
- overrides become messy
- renaming plans becomes painful
- inconsistent behavior across services

Use feature keys instead.

---

### 2. Gateway-Only Protection

Bad:

- route is blocked in gateway
- handler has no entitlement check

Why bad:

- internal execution paths can bypass it
- future routes may forget gateway config

---

### 3. UI-Only Restriction

Bad:

- button hidden in frontend
- backend still allows operation

Why bad:

- not security
- trivially bypassed

---

### 4. Silent Degradation Without Explicit Denial

Bad:

- request succeeds partially
- system quietly skips premium behavior
- user gets confusing output

If a feature is restricted, deny it clearly.

---

## Recommended Implementation Template

Each service should add:

### Application Layer

- feature gate abstraction
- entitlement-aware command/query handlers
- a standard domain/application exception for denied features

### Infrastructure Layer

- client for billing entitlement resolution
- optional cache for entitlement lookups
- resilience / retry policy for billing dependency

### API Layer

- exception mapping for entitlement-denied errors to HTTP 403
- optional endpoint metadata for route-level documentation

---

## Suggested Exception Model

```csharp
public sealed class FeatureNotEnabledException : Exception
{
    public FeatureNotEnabledException(string featureKey)
        : base($"Feature '{featureKey}' is not enabled for the tenant.")
    {
        FeatureKey = featureKey;
    }

    public string FeatureKey { get; }
}
```

Map this consistently in API middleware / exception handling.

---

## Suggested Review Checklist for Every Service

Before marking subscription enforcement complete, verify:

- [ ] billing defines the feature key
- [ ] gateway route mapping exists where applicable
- [ ] service handler checks entitlement before side effects
- [ ] async/background execution path also checks entitlement
- [ ] public/token endpoints are checked if they expose restricted capabilities
- [ ] frontend hides or disables unavailable actions
- [ ] API returns consistent 403/error payload
- [ ] postman/docs reflect actual restricted endpoints
- [ ] tests cover allowed and denied scenarios

---

## Required Test Coverage

Every restricted feature should have tests for:

1. **Allowed tenant**
   - operation succeeds

2. **Denied tenant**
   - operation fails with expected exception / 403

3. **Gateway denied path**
   - route blocked when gateway config is enabled

4. **Direct service execution denied**
   - handler still blocks even if gateway is bypassed

5. **Override/grant path**
   - feature works when a manual override enables it

6. **Public endpoint denied**
   - token/public route still fails if subscription does not allow feature

---

## Rollout Standard

For each service, implementation should happen in this order:

1. define feature keys in billing/shared contract
2. add gateway route awareness
3. add service-level feature gate abstraction
4. enforce in handlers
5. add exception mapping
6. add tests
7. update postman/docs only for real implemented behavior

---

## Definition of Done

A restricted capability is only considered correctly implemented when:

- the entitlement exists in billing
- the route is blocked in gateway where applicable
- the product service denies execution internally
- public/background/internal paths cannot bypass enforcement
- tests prove both allowed and denied behavior

If the service does not enforce the feature itself, the implementation is **not complete**.

---

## Final Rule

**Every service must treat subscription enforcement as part of business correctness, not just API filtering.**

The gateway is a guardrail.
The service is the lock.
Billing is the source of truth.
