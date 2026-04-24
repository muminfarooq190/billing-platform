select "Email", "Status", must_change_password from users where "Email" = 'admin@example.com';
update users
set "Status" = 'Active', updated_at = now()
where "Email" = 'admin@example.com' and ("Status" is null or "Status" = '');
select "Email", "Status", must_change_password from users where "Email" = 'admin@example.com';
