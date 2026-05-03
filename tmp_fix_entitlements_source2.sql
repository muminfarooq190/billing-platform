update feature_entitlements
set source = 'AdminGrant',
    plan_type = null,
    updated_at = now()
where tenant_id = 'b321fff2-345d-432d-b180-893fca01b298'::uuid;
