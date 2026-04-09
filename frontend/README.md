# Frontend Monorepo

This folder contains the proposed frontend monorepo for Voyara.

## Apps
- `apps/admin-portal` - internal operations/admin UI
- `apps/customer-portal` - customer-facing responsive web portal
- `apps/customer-mobile` - React Native / Expo mobile app scaffold

## Shared packages
- `packages/ui`
- `packages/design-tokens`
- `packages/types`
- `packages/utils`
- `packages/api-client`
- `packages/auth`

## Notes
- This is a scaffold aligned with the backend services in the repository.
- All API integration should go through the gateway-aware shared client package.
- Route strings should not be scattered across feature components.
