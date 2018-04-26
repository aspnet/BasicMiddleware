// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace BufferingBenchmarks
{
    public class JsonMiddleware
    {
        private static readonly JsonSerializer _json = new JsonSerializer();
        private static readonly UTF8Encoding _encoding = new UTF8Encoding(false);

        private readonly RequestDelegate _next;

        public JsonMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public Task Invoke(HttpContext httpContext)
        {
            httpContext.Response.StatusCode = 200;
            httpContext.Response.ContentType = "application/json";

            using (var sw = new StreamWriter(httpContext.Response.Body, _encoding))
            {
                _json.Serialize(sw, new
                {
                    login = "octocat",
                    id = 1,
                    avatar_url = "https://github.com/images/error/octocat_happy.gif",
                    gravatar_id = "",
                    url = "https://api.github.com/users/octocat",
                    html_url = "https://github.com/octocat",
                    followers_url = "https://api.github.com/users/octocat/followers",
                    following_url = "https://api.github.com/users/octocat/following{/other_user}",
                    gists_url = "https://api.github.com/users/octocat/gists{/gist_id}",
                    starred_url = "https://api.github.com/users/octocat/starred{/owner}{/repo}",
                    subscriptions_url = "https://api.github.com/users/octocat/subscriptions",
                    organizations_url = "https://api.github.com/users/octocat/orgs",
                    repos_url = "https://api.github.com/users/octocat/repos",
                    events_url = "https://api.github.com/users/octocat/events{/privacy}",
                    received_events_url = "https://api.github.com/users/octocat/received_events",
                    type = "User",
                    site_admin = false,
                    name = "monalisa octocat",
                    company = "GitHub",
                    blog = "https://github.com/blog",
                    location = "San Francisco",
                    email = "octocat@github.com",
                    hireable = false,
                    bio = "There once was...",
                    public_repos = 2,
                    public_gists = 1,
                    followers = 20,
                    following = 0,
                    created_at = "2008-01-14T04:33:35Z",
                    updated_at = "2008-01-14T04:33:35Z",
                    total_private_repos = 100,
                    owned_private_repos = 100,
                    private_gists = 81,
                    disk_usage = 10000,
                    collaborators = 8,
                    two_factor_authentication = true,
                    plan = new
                    {
                        name = "Medium",
                        space = 400,
                        private_repos = 20,
                        collaborators = 0
                    },
                    node_id = "MDQ6VXNlcjU4MzIzMQ=="
                });
            }

            return Task.CompletedTask;
        }
    }

    public static class JsonMiddlewareExtensions
    {
        public static IApplicationBuilder UseJson(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<JsonMiddleware>();
        }
    }
}
