// app.module.ts
import { Module } from '@nestjs/common';
import { ConfigModule } from '@nestjs/config';
import { RabbitMQModule } from '@golevelup/nestjs-rabbitmq';
import { MailModule } from './mail/mail.module';

@Module({
  imports: [
    ConfigModule.forRoot({ isGlobal: true }),
    RabbitMQModule.forRoot({
      uri: process.env.RABBIT_URI ?? 'amqp://guest:guest@emailservice-mq:5672',
      exchanges: [{ name: 'mail.commands', type: 'topic' }],
      connectionInitOptions: { wait: true, timeout: 5000 },
    }),
    MailModule,   
  ],
})
export class AppModule {}
