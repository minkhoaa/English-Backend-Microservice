// mail.module.ts
import { Module } from "@nestjs/common";
import { ConfigModule } from "@nestjs/config";
import { MailService } from "./mail.service";
import { MailController } from "./mail.controller";
import { MailSubscriber } from "./mail.subscriber";

@Module({
  imports:[ConfigModule],     
  providers: [MailService, MailSubscriber],
  controllers: [MailController]
})
export class MailModule {}
