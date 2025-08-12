using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Contracts
{
    public record EmailSendRequested(
        string To,
        string Subject,
        string Template,
        IDictionary<string, object>? Variables
);
}