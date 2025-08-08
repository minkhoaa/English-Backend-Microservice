using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthService.Models
{
    public class ApiResponse
    {
        public bool IsSucceed { get; set; }
        public string Message { get; set; }
        public object Data { get; set; } 
     }
}