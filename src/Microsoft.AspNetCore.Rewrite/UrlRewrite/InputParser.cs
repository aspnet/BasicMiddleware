// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Rewrite.Internal;

namespace Microsoft.AspNetCore.Rewrite.UrlRewrite
{
    /// <summary>
    /// Parses the TestString segment of the mod_rewrite condition.
    /// </summary>
    public class InputParser
    {
        private const char Percent = '%';
        private const char Dollar = '$';
        private const char Space = ' ';
        private const char Colon = ':';
        private const char OpenBrace = '{';
        private const char CloseBrace = '}';

        /// <summary>
        /// Creates a pattern, which is a template to create a new test string to 
        /// compare to the condition pattern. Can contain server variables, back references, etc.
        /// </summary>
        /// <param name="testString">The test string portion of the RewriteCond
        /// Examples:
        /// %{REMOTE_ADDR}
        /// /var/www/%{REQUEST_URI}
        /// %1
        /// $1</param>
        /// <returns>A new <see cref="Pattern"/>, containing a list of <see cref="PatternSegment"/></returns>
        public static Pattern ParseInputString(string testString)
        {
            if (testString == null)
            {
                testString = string.Empty;
            }
            var context = new ParserContext(testString);
            var results = new List<PatternSegment>();
            while (context.Next())
            {
                if (context.Current == OpenBrace)
                {
                    // This is a server parameter, parse for a condition variable
                    if (!context.Next())
                    {
                        throw new FormatException(context.Error());
                    }
                    if (!ParseParameter(context, results))
                    {
                        throw new FormatException(context.Error());
                    }
                }
                else
                {
                    // Parse for literals, which will return on either the end of the test string 
                    // or when it hits a special character
                    if (!ParseLiteral(context, results))
                    {
                        throw new FormatException(context.Error());
                    }
                }
            }
            return new Pattern(results);
        }

        private static bool ParseParameter(ParserContext context, List<PatternSegment> results)
        {
            context.Mark();
            string parameter;
            while (context.Next())
            {
                if (context.Current == CloseBrace)
                {
                    
                }
            }
        }

        /// <summary>
        /// Parse a string literal in the test string. Continues capturing until the start of a new variable type.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="results"></param>
        /// <returns></returns>
        private static bool ParseLiteral(ParserContext context, List<PatternSegment> results)
        {
            context.Mark();
            string literal;
            while (true)
            {
                if (context.Current == Percent || context.Current == Dollar)
                {
                    literal = context.Capture();
                    context.Back();
                    break;
                }
                if (!context.Next())
                {
                    literal = context.Capture();
                    break;
                }
            }

            if (IsValidLiteral(context, literal))
            {
                // add results
                results.Add(new PatternSegment(literal, SegmentType.Literal));
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
