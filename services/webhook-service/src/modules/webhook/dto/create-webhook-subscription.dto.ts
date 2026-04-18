import { IsArray, IsBoolean, IsOptional, IsString, IsUrl, IsUUID, ArrayNotEmpty, ArrayUnique, MaxLength } from 'class-validator';

export class CreateWebhookSubscriptionDto {
  @IsUUID()
  public tenantId!: string;

  @IsUrl({ require_tld: false }, { message: 'targetUrl must be a valid URL.' })
  @MaxLength(500)
  public targetUrl!: string;

  @IsArray()
  @ArrayNotEmpty()
  @ArrayUnique()
  @IsString({ each: true })
  public events!: string[];

  @IsString()
  @MaxLength(255)
  public signingSecret!: string;

  @IsOptional()
  @IsBoolean()
  public isActive?: boolean;
}
