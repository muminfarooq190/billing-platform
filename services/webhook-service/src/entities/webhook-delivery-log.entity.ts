import { Column, CreateDateColumn, Entity, PrimaryGeneratedColumn } from 'typeorm';

@Entity({ name: 'webhook_delivery_logs' })
export class WebhookDeliveryLogEntity {
  @PrimaryGeneratedColumn('uuid')
  public id!: string;

  @Column({ name: 'webhook_subscription_id', type: 'uuid' })
  public webhookSubscriptionId!: string;

  @Column({ name: 'event_type', type: 'varchar', length: 200 })
  public eventType!: string;

  @Column({ name: 'payload', type: 'jsonb' })
  public payload!: Record<string, unknown>;

  @Column({ name: 'status', type: 'varchar', length: 50 })
  public status!: 'Pending' | 'Delivered' | 'Failed' | 'Dead';

  @Column({ name: 'attempt_count', type: 'int', default: 0 })
  public attemptCount!: number;

  @Column({ name: 'last_attempt_at', type: 'timestamptz', nullable: true })
  public lastAttemptAt!: Date | null;

  @Column({ name: 'response_status_code', type: 'int', nullable: true })
  public responseStatusCode!: number | null;

  @Column({ name: 'response_body', type: 'text', nullable: true })
  public responseBody!: string | null;

  @CreateDateColumn({ name: 'created_at' })
  public createdAt!: Date;
}
