import { Body, Controller, Get, Post } from "@nestjs/common";
import { IsEmail, IsNotEmpty, IsObject, IsOptional, IsString } from "class-validator";
import { MailService } from "./mail.service";
import { ok } from "assert";

class SendEmailDto {
  @IsEmail() to!: string;
  @IsString() @IsNotEmpty() subject!: string;
  @IsString() @IsNotEmpty() template!: string; // vd: 'new-content'
  @IsObject() @IsOptional() variables?: Record<string, any>;
}


@Controller()
export class MailController {
    constructor(private readonly mail: MailService) {}

    @Post('emails')
    public async send(@Body() dto: SendEmailDto) {
        await this.mail.send(dto);
        return {ok:true};
    }
    
}