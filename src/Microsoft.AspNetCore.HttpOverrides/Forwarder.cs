using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder
{
    public abstract class Forwarder
    {
        public abstract void ApplyForwarders(HttpContext context);
    }
}
