﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Microsoft.AspNetCore.Rewrite.Internal.UrlActions
{
    public class RewriteAction : UrlAction
    {
        public RuleResult Result { get; }
        public bool QueryStringAppend { get; }
        public bool QueryStringDelete { get; }
        public bool EscapeBackReferences { get; }

        public RewriteAction(
            RuleResult result,
            Pattern pattern,
            bool queryStringAppend,
            bool queryStringDelete,
            bool escapeBackReferences)
        {
            // For the replacement, we must have at least
            // one segment (cannot have an empty replacement)
            Result = result;
            Url = pattern;
            QueryStringAppend = queryStringAppend;
            QueryStringDelete = queryStringDelete;
            EscapeBackReferences = escapeBackReferences;
        }

        public RewriteAction(
            RuleResult result,
            Pattern pattern,
            bool queryStringAppend) :
            this(result,
                pattern,
                queryStringAppend,
                queryStringDelete: false,
                escapeBackReferences: false)
        {

        }

        public override void ApplyAction(RewriteContext context, BackReferenceCollection ruleBackReferences, BackReferenceCollection conditionBackReferences)
        {
            var pattern = Url.Evaluate(context, ruleBackReferences, conditionBackReferences);
            var request = context.HttpContext.Request;

            if (string.IsNullOrEmpty(pattern))
            {
                pattern = "/";
            }

            if (EscapeBackReferences)
            {
                // because escapebackreferences will be encapsulated by the pattern, just escape the pattern
                pattern = Uri.EscapeDataString(pattern);
            }


            // TODO PERF, substrings, object creation, etc.
            if (pattern.IndexOf("://", StringComparison.Ordinal) >= 0)
            {
                string scheme;
                HostString host;
                PathString path;
                QueryString query;
                FragmentString fragment;
                UriHelper.FromAbsolute(pattern, out scheme, out host, out path, out query, out fragment);

                if (query.HasValue)
                {
                    if (QueryStringAppend)
                    {
                        request.QueryString = request.QueryString.Add(query);
                    }
                    else
                    {
                        request.QueryString = query;
                    }
                }
                else if (QueryStringDelete)
                {
                    request.QueryString = QueryString.Empty;
                }

                request.Scheme = scheme;
                request.Host = host;
                request.Path = path;
            }
            else
            {
                var split = pattern.IndexOf('?');
                if (split >= 0)
                {
                    var path = pattern.Substring(0, split);
                    if (path[0] == '/')
                    {
                        request.Path = PathString.FromUriComponent(path);
                    }
                    else
                    {
                        request.Path = PathString.FromUriComponent('/' + path);
                    }

                    if (QueryStringAppend)
                    {
                        request.QueryString = request.QueryString.Add(
                            QueryString.FromUriComponent(
                                pattern.Substring(split)));
                    }
                    else
                    {
                        request.QueryString = QueryString.FromUriComponent(
                            pattern.Substring(split));
                    }
                }
                else
                {
                    if (pattern[0] == '/')
                    {
                        request.Path = PathString.FromUriComponent(pattern);
                    }
                    else
                    {
                        request.Path = PathString.FromUriComponent('/' + pattern);
                    }

                    if (QueryStringDelete)
                    {
                        request.QueryString = QueryString.Empty;
                    }
                }
            }
            context.Result = Result;
        }
    }
}
