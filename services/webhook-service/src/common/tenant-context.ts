import { Injectable, UnauthorizedException } from '@nestjs/common';
import { Request } from 'express';

@Injectable()
export class TenantContext {
  public getTenantId(request: Request): string {
    const tenantId = request.header('x-tenant-id')?.trim();
    if (!tenantId) {
      throw new UnauthorizedException('x-tenant-id header is required.');
    }

    return tenantId;
  }
}
