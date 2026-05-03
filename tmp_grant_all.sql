select "Id", "TenantId", "Email", "Role", "Status" from users;
select "Id", "TenantId", "Name", "NormalizedName", is_system_default from role_definitions order by "TenantId" nulls first, "Name";
select tenant_id, user_id, feature_key, status from tenant_user_feature_assignments order by tenant_id, user_id, feature_key;
select tenant_id, feature_key, granted, source from feature_entitlements order by tenant_id, feature_key;
