import { MigrationInterface, QueryRunner } from 'typeorm';

export class InitialSchema0000000000001 implements MigrationInterface {
  name = 'InitialSchema0000000000001';

  public async up(queryRunner: QueryRunner): Promise<void> {
    await queryRunner.query(`
      CREATE TABLE IF NOT EXISTS "webhook_subscriptions" (
        "id" uuid NOT NULL DEFAULT uuid_generate_v4(),
        "tenant_id" uuid NOT NULL,
        "target_url" character varying(500) NOT NULL,
        "events" text array NOT NULL,
        "signing_secret" character varying(255) NOT NULL,
        "is_active" boolean NOT NULL DEFAULT true,
        "created_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
        "updated_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
        CONSTRAINT "PK_webhook_subscriptions_id" PRIMARY KEY ("id")
      )
    `);

    await queryRunner.query(`
      CREATE TABLE IF NOT EXISTS "webhook_delivery_logs" (
        "id" uuid NOT NULL DEFAULT uuid_generate_v4(),
        "webhook_subscription_id" uuid NOT NULL,
        "event_type" character varying(200) NOT NULL,
        "payload" jsonb NOT NULL,
        "status" character varying(50) NOT NULL,
        "attempt_count" integer NOT NULL DEFAULT 0,
        "last_attempt_at" TIMESTAMPTZ NULL,
        "response_status_code" integer NULL,
        "response_body" text NULL,
        "created_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
        "updated_at" TIMESTAMPTZ NOT NULL DEFAULT now(),
        CONSTRAINT "PK_webhook_delivery_logs_id" PRIMARY KEY ("id"),
        CONSTRAINT "FK_webhook_delivery_logs_subscription" FOREIGN KEY ("webhook_subscription_id") REFERENCES "webhook_subscriptions"("id") ON DELETE CASCADE
      )
    `);

    await queryRunner.query(`
      CREATE INDEX IF NOT EXISTS "IX_webhook_delivery_logs_subscription" ON "webhook_delivery_logs" ("webhook_subscription_id")
    `);

    await queryRunner.query(`
      CREATE INDEX IF NOT EXISTS "IX_webhook_delivery_logs_status" ON "webhook_delivery_logs" ("status")
    `);
  }

  public async down(queryRunner: QueryRunner): Promise<void> {
    await queryRunner.query('DROP INDEX IF EXISTS "IX_webhook_delivery_logs_status"');
    await queryRunner.query('DROP INDEX IF EXISTS "IX_webhook_delivery_logs_subscription"');
    await queryRunner.query('DROP TABLE IF EXISTS "webhook_delivery_logs"');
    await queryRunner.query('DROP TABLE IF EXISTS "webhook_subscriptions"');
  }
}
