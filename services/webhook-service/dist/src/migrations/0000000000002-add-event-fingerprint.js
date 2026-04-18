"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.AddEventFingerprint0000000000002 = void 0;
class AddEventFingerprint0000000000002 {
    constructor() {
        this.name = 'AddEventFingerprint0000000000002';
    }
    async up(queryRunner) {
        await queryRunner.query(`
      ALTER TABLE "webhook_delivery_logs"
      ADD COLUMN IF NOT EXISTS "event_fingerprint" character varying(128) NULL
    `);
        await queryRunner.query(`
      CREATE UNIQUE INDEX IF NOT EXISTS "UX_webhook_delivery_logs_subscription_fingerprint"
      ON "webhook_delivery_logs" ("webhook_subscription_id", "event_fingerprint")
      WHERE "event_fingerprint" IS NOT NULL
    `);
    }
    async down(queryRunner) {
        await queryRunner.query('DROP INDEX IF EXISTS "UX_webhook_delivery_logs_subscription_fingerprint"');
        await queryRunner.query('ALTER TABLE "webhook_delivery_logs" DROP COLUMN IF EXISTS "event_fingerprint"');
    }
}
exports.AddEventFingerprint0000000000002 = AddEventFingerprint0000000000002;
//# sourceMappingURL=0000000000002-add-event-fingerprint.js.map