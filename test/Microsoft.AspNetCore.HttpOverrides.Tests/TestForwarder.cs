using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.HttpOverrides.Tests
{
    public class TestForwarder : Forwarder
    {
	    public override void ApplyForwarders(HttpContext context)
	    {
		    if (!context.Request.Headers.ContainsKey("X-ARR-SSL")) return;
				context.Request.Scheme = "https";
				context.Request.Headers.Remove("X-ARR-SSL");
	    }
    }
}
