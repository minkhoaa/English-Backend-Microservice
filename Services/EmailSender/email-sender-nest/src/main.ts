import { NestFactory } from '@nestjs/core';
import { AppModule } from './app.module';
import { ValidationPipe } from '@nestjs/common';

import { SwaggerModule, DocumentBuilder} from '@nestjs/swagger'

async function bootstrap() {
  const app = await NestFactory.create(AppModule);

  app.useGlobalPipes(new ValidationPipe({whitelist: true, transform:true}))

  const config = new DocumentBuilder()
            .setTitle("Email/Nest Service")
            .setDescription("API docs")
            .setVersion("1.0")
            .addBearerAuth()
            .build();

  const document = SwaggerModule.createDocument(app, config);
  SwaggerModule.setup('swagger', app, document); 

  await app.listen(process.env.PORT ?? 3000,'0.0.0.0');
  console.log("Email Sender listening on port 3000");

  
}
bootstrap();
