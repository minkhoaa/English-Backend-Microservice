using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Models
{
    public sealed class MessageBrokerSettings
    {
        public string Host { get; set; } = default!;  
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
    }
}