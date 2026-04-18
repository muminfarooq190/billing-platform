# Service-by-Service Blocker Checklist

_Last updated: 2026-04-18_

This checklist tracks the major blockers, near-blockers, and caution items for the current branch.

Legend:
- **BLOCKER** = should be fixed/verified before relying on the service for that purpose
- **RISK** = launchable with care, but can hurt MVP quality or operations
- **LATER** = important hardening, but not immediate MVP blocker

---

# 1. Billing-service

## Current verdict
**MVP-capable, not fully finance-hardened**

## BLOCKERS / near-blockers
- [ ] Stripe payment flow verified in deployed environment end-to-end
- [ ] Stripe webhook reconciliation verified in deployed environment
- [ ] package metadata pricing present and valid for all launch packages
- [ ] duplicate billing period invoice protection validated under real scheduler conditions

## RISKS
- [ ] no dedicated payment transaction entity/table yet
- [ ] no refunds / partial refunds yet
- [ ] finance audit trail still invoice-centric rather than payment-transaction-centric
- [ ] invoice/receipt artifact maturity is behind travel document maturity

## LATER
- [ ] add payment transaction model
- [ ] add refund support
- [ ] add richer collections/recovery handling

---

# 2. Communication-service

## Current verdict
**Email path is strong; WhatsApp path is usable but less proven**

## BLOCKERS / near-blockers
- [ ] recipient resolution verified for real customer contacts in deployed environment
- [ ] SendGrid configuration present and verified
- [ ] if WhatsApp is part of MVP, Twilio configuration present and verified
- [ ] if WhatsApp is promised, at least one successful real media send completed

## RISKS
- [ ] attachment fetch failure can still silently degrade delivery quality
- [ ] provider delivery callbacks are not fully implemented
- [ ] quiet-hours-aware channel fallback is not enforced
- [ ] preferred channel routing only works as intended when callers do not hard-force a channel
- [ ] WhatsApp media depends on externally reachable document URLs

## LATER
- [ ] add provider callback ingestion/status reconciliation
- [ ] tighten attachment failure policy
- [ ] add quiet-hours-aware preferred routing
- [ ] refine channel policy/fallback ordering by notification type

---

# 3. Travel-service

## Current verdict
**One of the strongest product surfaces in the branch**

## BLOCKERS / near-blockers
- [ ] quote send -> communication arrival verified end-to-end in deployed environment
- [ ] itinerary create -> communication arrival verified end-to-end in deployed environment
- [ ] QuestPDF rendering verified in target environment
- [ ] document endpoint access model reviewed for tenant/security expectations

## RISKS
- [ ] booking-confirmed communication path is not as clearly wired as quote/itinerary
- [ ] public/share/document URL exposure needs deployment-aware review

## LATER
- [ ] add booking-confirmed end-to-end communication trigger if in scope
- [ ] improve document design/branding/polish if customer-facing appearance matters more

---

# 4. Identity-service

## Current verdict
**Feature-integrated, but not deeply audited in this pass**

## BLOCKERS / near-blockers
- [ ] verify branding/settings/admin features are correctly mapped to launch packages

## RISKS
- [ ] not deeply behavior-audited in the same depth as billing/travel/communication

## LATER
- [ ] deeper tenant/admin/security audit if scaling or exposing more admin surfaces

---

# 5. Webhook-service

## Current verdict
**Real, useful, and much safer than before; still not fully mature**

## BLOCKERS / near-blockers
- [ ] webhook-service deployed behind an acceptable trust boundary for `x-tenant-id`-based scoping model
- [ ] subscription create/list/replay behavior verified in deployed environment
- [ ] outbound webhook target verification/signature contract documented for consumers

## RISKS
- [ ] auth model still basic even after tenant scoping hardening
- [ ] retry policy is improved but still not extremely nuanced
- [ ] dedupe is fingerprint-based and should be verified against real upstream event shapes
- [ ] no advanced secret rotation model for outbound webhook signing

## LATER
- [ ] stronger auth/authz model than just tenant header trust
- [ ] dual-secret rotation support
- [ ] more advanced retry/dead-letter operations tooling

---

# 6. Feature/entitlement platform

## Current verdict
**One of the strongest areas; genuinely integrated across major services**

## BLOCKERS / near-blockers
- [ ] target tenant package assignments confirmed in launch environment
- [ ] seeded feature catalog/packages confirmed in launch environment
- [ ] launch packages match actual intended product plan/offer

## RISKS
- [ ] package evolution/change management can get messy if not documented
- [ ] legacy compatibility packages still exist and should be understood operationally

## LATER
- [ ] better tooling/docs for package evolution and entitlement audits

---

# 7. Cross-service / platform-wide blockers

## BLOCKERS / near-blockers
- [ ] all required env vars are present in deployed environments
- [ ] RabbitMQ connectivity stable across billing/communication/webhook
- [ ] Postgres migrations applied across all touched services
- [ ] Redis availability verified where required
- [ ] public base URLs correct for Stripe/travel documents/communication references

## RISKS
- [ ] runtime configuration drift across services can break otherwise-good code
- [ ] live provider validation (Stripe/SendGrid/Twilio) matters more now than code shape alone

## LATER
- [ ] automated preflight checks for service-to-service and provider config

---

# 8. Launch recommendation summary

## Safe to launch first
- [ ] feature-gated SaaS access model
- [ ] email-first quote/itinerary communication
- [ ] Stripe-backed MVP billing flow
- [ ] webhook integration for controlled consumers

## Launch with extra caution
- [ ] WhatsApp as a publicly promised channel
- [ ] webhook-service in semi-untrusted network exposure
- [ ] billing claims beyond MVP-grade finance operations

## Strongest next hardening sequence
1. [ ] communication callback/status hardening
2. [ ] live WhatsApp validation
3. [ ] billing payment transaction model
4. [ ] stronger webhook auth model
