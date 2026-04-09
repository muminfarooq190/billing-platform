export const FeatureKeys = {
  travelQuotationCreate: 'travel.quotation.create',
  travelQuotationSend: 'travel.quotation.send',
  travelQuotationAttachmentsUpload: 'travel.quotation.attachments.upload',
  travelBookingCreate: 'travel.booking.create',
  travelBookingDocumentsUpload: 'travel.booking.documents.upload',
  travelTimelineRead: 'travel.timeline.read',
  travelAuditRead: 'travel.audit.read',
  travelNotesWrite: 'travel.notes.write',
  communicationNotificationSend: 'communication.notification.send',
  communicationTemplatesManage: 'communication.templates.manage',
  communicationBulkSend: 'communication.bulk.send',
  communicationLogsRead: 'communication.logs.read',
  brandingThemeManage: 'branding.theme.manage',
  brandingAssetsManage: 'branding.assets.manage',
  identityRbacAdvanced: 'identity.rbac.advanced',
  identitySsoManage: 'identity.sso.manage',
  identityAuditExport: 'identity.audit.export'
} as const;

export type FeatureKey = (typeof FeatureKeys)[keyof typeof FeatureKeys];
