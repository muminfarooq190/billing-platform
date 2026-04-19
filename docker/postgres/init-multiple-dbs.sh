#!/bin/sh
set -eu

if [ -z "${POSTGRES_USER:-}" ] || [ -z "${POSTGRES_PASSWORD:-}" ]; then
  echo "POSTGRES_USER and POSTGRES_PASSWORD must be set"
  exit 1
fi

create_db() {
  db_name="$1"
  echo "Ensuring database '$db_name' exists"
  psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname postgres <<-EOSQL
    SELECT format('CREATE DATABASE %I', '$db_name')
    WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = '$db_name')\gexec
EOSQL
}

create_db "billing_identity"
create_db "billing_billing"
create_db "billing_webhook"
create_db "billing_travel"
create_db "billing_communication"
create_db "billing_geo_leads"

psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "billing_geo_leads" <<-EOSQL
  CREATE EXTENSION IF NOT EXISTS postgis;
EOSQL
