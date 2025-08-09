import { Body, Controller, Get, Post } from "@nestjs/common";
import { IsEmail, IsNotEmpty, IsObject, IsOptional, IsString } from "class-validator";
import { MailService } from "./mail.service";
import { ok } from "assert";
import { ApiTags, ApiResponse, ApiProperty, ApiPropertyOptional  } from "@nestjs/swagger";
import { time } from "console";
import { title } from "process";
import { url } from "inspector";
class SendEmailDto {
  @ApiProperty({example:'user@example.com'})
  @IsEmail() to!: string;
  @ApiProperty({example: 'string'})
  @IsString() @IsNotEmpty() subject!: string;
  @ApiProperty({example:'new-content'})
  @IsString() @IsNotEmpty() template!: string; 

  @ApiPropertyOptional({
    type:'object',
    additionalProperties: true,
    example: {title: 'Unit 12', url: 'https://app.local/lesson/12'}
  })
  @IsObject() @IsOptional() variables?: Record<string, any>;
}

@ApiTags('Email')
@Controller('emailcontroller')
export class MailController {
    constructor(private readonly mail: MailService) {}

    @Post('send')
    @ApiResponse({status: 200, description:'Email sent'})
    public async send(@Body() dto: SendEmailDto) {
        await this.mail.send(dto);
        return {ok:true};
    }
    
}