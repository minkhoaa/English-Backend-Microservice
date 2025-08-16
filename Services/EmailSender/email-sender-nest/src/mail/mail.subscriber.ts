import { Injectable, Logger } from '@nestjs/common';
import { RabbitSubscribe } from '@golevelup/nestjs-rabbitmq';
import { MailService } from './mail.service';
import { Console } from 'console';

type EmailSendMessage = {
  to: string;
  subject: string;
  template: string;
  variables?: Record<string, unknown>;
};

@Injectable()
export class MailSubscriber {
  private readonly log = new Logger(MailSubscriber.name);

  constructor(private readonly mail: MailService) {
    this.log.log('✅ MailSubscriber constructed');
  }

  @RabbitSubscribe({
    exchange: 'mail.commands',
    routingKey: 'email.send',
    queue: 'email.send.queue',
  })
  async handleEmailSend(envelope: any) {
    // Unwrap MassTransit envelope (payload thật nằm trong envelope.message)
    const data: any = envelope?.message ?? envelope;

    // Chuẩn hoá key (camelCase/PascalCase)
    const to        = data?.to        ?? data?.To;
    const subject   = data?.subject   ?? data?.Subject;
    const template  = data?.template  ?? data?.Template;
    const rawVars   = data?.variables ?? data?.Variables;

    const variables =
      rawVars && typeof rawVars === 'object' && !Array.isArray(rawVars) ? rawVars : {};

    if (!to || !subject || !template) {
      this.log.error(`❌ Invalid payload: ${JSON.stringify(data)}`);
      return; // hoặc throw new Error(...) nếu bạn muốn requeue
    }

    this.log.log(`📨 email.send → to=${to}, template=${template}`);
    try {
      await this.mail.sendEmailConfirm({ to, subject, template, variables: variables as Record<string, any> });
      this.log.log(`✅ Email sent to ${to}`);
    } catch (err: any) {
      this.log.error(`❌ Send failed: ${err?.message || err}`);
      throw err; // để broker requeue nếu bạn cấu hình
    }
  }
}
