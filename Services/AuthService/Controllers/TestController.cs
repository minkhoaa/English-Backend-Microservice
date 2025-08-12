using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Contracts;
using MassTransit;
using MassTransit.Testing;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IPublishEndpoint _bus;
        public TestController(IPublishEndpoint bus)
        {
            _bus = bus;
        }

    
        [HttpPost("/send-email")]
        public async Task<IActionResult> SendEmail()
        {
            var msg = new EmailSendRequested(
                To: "tukhoa040505@gmail.com",
                Subject: "Test email",
                Template: "new-content",
                Variables: new Dictionary<string, object>
                {
                    ["resetUrl"] = "https://app.local/reset?token=abc123"
                }
            );

            await _bus.Publish(msg, ctx =>
            {
                ctx.SetRoutingKey("email.send");
            });

            return Ok("ok");
        }
    }
}