import { Injectable } from "@nestjs/common";
import { ConfigService } from "@nestjs/config";
import * as nodemailer from "nodemailer"
import * as fs from "fs/promises"
import path from "path";
import Handlebars, { Exception } from "handlebars";
import mjml2html from "mjml"
import { ExecutionContextHost } from "@nestjs/core/helpers/execution-context-host";


@Injectable()
export class MailService {
    private transporter : nodemailer.Transporter; 
    private from: string;
    constructor(private cfg : ConfigService) {
        const driver = this.cfg.get<string>("MAIL_DRIVER") || "smtp";
        if (driver == "gmail") {
            this.transporter = nodemailer.createTransport({
                service: 'gmail',
                auth: {
                    user: this.cfg.get<string>("GMAIL_USER"),
                    pass: this.cfg.get<string>("GMAIL_APP_PASS")
                },
            });
            this.from = this.cfg.get<string>('MAIL_FROM')!;
        } else  {
            this.transporter = nodemailer.createTransport({
                host: this.cfg.get<string>('MAIL_SMTP_HOST'),
                port: Number(this.cfg.get<string>('MAIL_SMTP_PORT') || 1025),
                secure: false,
        });
            this.from = this.cfg.get<string>('MAIL_FROM') || 'no-reply@example.com';
        }
    }
    private async render(templateName:string, variables: Record<string,any>) {
        const file = path.join(process.cwd(), 'src','mail', 'templates', `${templateName}.mjml`);
        const mjml = await fs.readFile(file, 'utf-8');
        const compiled = Handlebars.compile(mjml); 
        const mjmlFilled = compiled(variables); 
        const {html, errors } = mjml2html(mjmlFilled);
        if (errors?.length) throw new Error('Mjml render error' + JSON.stringify(errors));
        return html; 
    }
    public async send(opts: {to:string, subject:string, template:string, variables?: Record<string, any>}) {
        const html = await this.render(opts.template, opts.variables?? {});
        await this.transporter.sendMail({from:this.from, to:opts.to, subject:opts.subject, html:html});
    }
}