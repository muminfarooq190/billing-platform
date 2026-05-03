-- identity: make sure Owner role carries every seeded/system permission we care about now
insert into role_permission_assignments ("Id", "RoleDefinitionId", "PermissionKey", created_at)
select gen_random_uuid(), rd."Id", p.permission_key, now()
from role_definitions rd
cross join (
  values
    ('identity.users.manage'),
    ('identity.roles.manage'),
    ('identity.audit.read'),
    ('identity.settings.manage'),
    ('identity.tenant.manage'),
    ('branding.theme.manage'),
    ('travel.workflowhub.read'),
    ('travel.inquiries.read'),
    ('travel.inquiries.write'),
    ('travel.contacts.read'),
    ('travel.contacts.write'),
    ('travel.followups.read'),
    ('travel.followups.write'),
    ('travel.bookings.read'),
    ('travel.bookings.write'),
    ('travel.itineraries.read'),
    ('travel.itineraries.write'),
    ('travel.timeline.read'),
    ('travel.notes.read'),
    ('travel.notes.write'),
    ('travel.documents.read'),
    ('travel.audit.read'),
    ('travel.quotation.read'),
    ('travel.quotations.read'),
    ('travel.quotation.write'),
    ('billing.invoices.read'),
    ('communication.logs.read'),
    ('communication.notification.send'),
    ('communication.templates.manage')
) as p(permission_key)
where rd."NormalizedName" = 'OWNER'
and not exists (
  select 1
  from role_permission_assignments existing
  where existing."RoleDefinitionId" = rd."Id"
    and existing."PermissionKey" = p.permission_key
);

-- billing: grant every feature in catalog to tenant
insert into feature_entitlements (
  "Id", tenant_id, feature_key, granted, source, plan_type, limit_value,
  effective_from, effective_to, metadata_json, created_at, updated_at, deleted_at
)
select
  gen_random_uuid(),
  'b321fff2-345d-432d-b180-893fca01b298'::uuid,
  fc.feature_key,
  true,
  'manual-grant',
  'OwnerOverride',
  null,
  now(),
  null,
  '{}'::jsonb,
  now(),
  now(),
  null
from feature_catalog fc
where not exists (
  select 1
  from feature_entitlements fe
  where fe.tenant_id = 'b321fff2-345d-432d-b180-893fca01b298'::uuid
    and fe.feature_key = fc.feature_key
    and coalesce(fe.deleted_at, '-infinity'::timestamp with time zone) is null
);

-- billing: assign every feature to the current owner user
insert into tenant_user_feature_assignments (
  id, tenant_id, user_id, feature_key, status, assigned_by_user_id,
  assigned_at, revoked_by_user_id, revoked_at, effective_from, effective_to,
  notes, metadata_json, created_at, updated_at, deleted_at
)
select
  gen_random_uuid(),
  'b321fff2-345d-432d-b180-893fca01b298'::uuid,
  '90e88d0f-2276-421a-837a-7f023ba06495'::uuid,
  fc.feature_key,
  'Active',
  '90e88d0f-2276-421a-837a-7f023ba06495'::uuid,
  now(),
  null,
  null,
  now(),
  null,
  'Bulk grant all features for local admin owner.',
  '{}'::jsonb,
  now(),
  now(),
  null
from feature_catalog fc
where not exists (
  select 1
  from tenant_user_feature_assignments tua
  where tua.tenant_id = 'b321fff2-345d-432d-b180-893fca01b298'::uuid
    and tua.user_id = '90e88d0f-2276-421a-837a-7f023ba06495'::uuid
    and tua.feature_key = fc.feature_key
    and tua.status = 'Active'
    and coalesce(tua.deleted_at, '-infinity'::timestamp with time zone) is null
);
