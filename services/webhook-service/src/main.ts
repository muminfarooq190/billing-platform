import { Controller, Get, Module } from '@nestjs/common';
import { NestFactory } from '@nestjs/core';

@Controller()
class AppController {
  @Get('/health')
  public health(): { service: string; status: string } {
    return { service: 'webhook-service', status: 'ok' };
  }
}

@Module({
  controllers: [AppController],
})
class AppModule {}

async function bootstrap(): Promise<void> {
  const app = await NestFactory.create(AppModule);
  const port = Number.parseInt(process.env.PORT ?? '3000', 10);
  await app.listen(port);
}

void bootstrap();
