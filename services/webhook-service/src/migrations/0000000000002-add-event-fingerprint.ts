import { MigrationInterface, QueryRunner } from 'typeorm';

export class AddEventFingerprint0000000000002 implements MigrationInterface {
  name = 'AddEventFingerprint0000000000002';

  public async up(queryRunner: QueryRunner): Promise<void> {
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

  public async down(queryRunner: QueryRunner): Promise<void> {
    await queryRunner.query('DROP INDEX IF EXISTS "UX_webhook_delivery_logs_subscription_fingerprint"');
    await queryRunner.query('ALTER TABLE "webhook_delivery_logs" DROP COLUMN IF EXISTS "event_fingerprint"');
  }
}
