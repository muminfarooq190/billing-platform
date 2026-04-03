#!/usr/bin/env bash
set -euo pipefail

API_GATEWAY_URL=${API_GATEWAY_URL:-http://localhost:5000}
TENANT_NAME=${TENANT_NAME:-"Acme Corp"}
TENANT_EMAIL=${TENANT_EMAIL:-"admin@acme.com"}
TENANT_PASSWORD=${TENANT_PASSWORD:-"Demo1234!"}

REGISTER_RESPONSE=$(curl -sS -X POST "$API_GATEWAY_URL/api/auth/register" \
  -H "Content-Type: application/json" \
  -d "{\"tenantName\":\"$TENANT_NAME\",\"email\":\"$TENANT_EMAIL\",\"password\":\"$TENANT_PASSWORD\"}")

ACCESS_TOKEN=$(echo "$REGISTER_RESPONSE" | sed -n 's/.*"accessToken":"\([^"]*\)".*/\1/p')
TENANT_ID=$(echo "$REGISTER_RESPONSE" | sed -n 's/.*"tenantId":"\([^"]*\)".*/\1/p')

if [[ -z "$ACCESS_TOKEN" || -z "$TENANT_ID" ]]; then
  echo "Failed to register demo tenant. Response: $REGISTER_RESPONSE"
  exit 1
fi

SUBSCRIPTION_RESPONSE=$(curl -sS -X POST "$API_GATEWAY_URL/api/billing/subscriptions" \
  -H "Authorization: Bearer $ACCESS_TOKEN" \
  -H "Content-Type: application/json" \
  -d "{\"tenantId\":\"$TENANT_ID\",\"planType\":\"Pro\",\"billingCycle\":\"Monthly\"}")

SUBSCRIPTION_ID=$(echo "$SUBSCRIPTION_RESPONSE" | sed -n 's/.*"id":"\([^"]*\)".*/\1/p')

for _ in 1 2 3; do
  curl -sS -X POST "$API_GATEWAY_URL/api/billing/invoices/generate" \
    -H "Authorization: Bearer $ACCESS_TOKEN" \
    -H "Content-Type: application/json" \
    -d "{\"subscriptionId\":\"$SUBSCRIPTION_ID\"}" >/dev/null

done

echo "Seed completed for tenant $TENANT_ID"
