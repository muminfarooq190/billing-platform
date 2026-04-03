import { Column, CreateDateColumn, Entity, PrimaryGeneratedColumn } from 'typeorm';

@Entity({ name: 'webhook_subscriptions' })
export class WebhookSubscriptionEntity {
  @PrimaryGeneratedColumn('uuid')
  public id!: string;

  @Column({ name: 'tenant_id', type: 'uuid' })
  public tenantId!: string;

  @Column({ name: 'target_url', type: 'varchar', length: 500 })
  public targetUrl!: string;

  @Column({ name: 'events', type: 'text', array: true })
  public events!: string[];

  @Column({ name: 'signing_secret', type: 'varchar', length: 255 })
  public signingSecret!: string;

  @Column({ name: 'is_active', type: 'boolean', default: true })
  public isActive!: boolean;

  @CreateDateColumn({ name: 'created_at' })
  public createdAt!: Date;
}
