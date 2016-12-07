// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

namespace Microsoft.AspNetCore.Rewrite.Extensions
{
	public static class HttpRequestExtensions
	{
		public static string ToAbsoluteUri(this HttpRequest request)
		{
			return UriHelper.BuildAbsolute(
				request.Scheme,
				request.Host,
				request.PathBase,
				request.Path,
				request.QueryString);
		}
	}
}