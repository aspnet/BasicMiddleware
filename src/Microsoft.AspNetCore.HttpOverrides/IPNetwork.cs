﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace Microsoft.AspNetCore.HttpOverrides
{
    public class IPNetwork
    {
        static readonly char[] PrefixLengthSeparators = new char[] { '/' };

        /// <summary>
        /// All IPv4 addresses 0.0.0.0/0
        /// </summary>
        public static readonly IPNetwork AllIPv4 = new IPNetwork(IPAddress.Parse("0.0.0.0"), 0);

        /// <summary>
        /// IPv4 localhost range 127.0.0.0/8 (255.0.0.0)
        /// </summary>
        public static readonly IPNetwork IPv4Loopback = Parse("127.0.0.1/8");

        /// <summary>
        /// IPv4 link-local range 169.254.0.0/16 (255.255.0.0)
        /// </summary>
        public static readonly IPNetwork IPv4LinkLocal = Parse("169.254.0.0/16");

        /// <summary>
        /// RFC1918 Private IPv4 range 10.0.0.0/8 (255.0.0.0)
        /// </summary>
        public static readonly IPNetwork IPv4Private10 = Parse("10.0.0.0/8");

        /// <summary>
        /// RFC1918 Private IPv4 range 172.16.0.0/12 (255.240.0.0)
        /// </summary>
        public static readonly IPNetwork IPv4Private172 = Parse("172.16.0.0/12");

        /// <summary>
        /// RFC1918 Private IPv4 range 192.168.0.0/16 (255.255.0.0)
        /// </summary>
        public static readonly IPNetwork IPv4Private192 = Parse("192.168.0.0/16");

        /// <summary>
        /// RFC1918 Private IPv4 range 192.168.0.0/16 (255.255.0.0)
        /// </summary>
        public static readonly IPNetwork IPv4Multicast = Parse("224.0.0.0/4");

        /// <summary>
        /// RFC1918 Private IPv4 Addresses 10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16, link-local, and localhost
        /// </summary>
        public static readonly IPNetwork[] IPv4PrivateNetworks = new IPNetwork[]
        {
            IPv4Loopback,
            IPv4LinkLocal,
            IPv4Private10,
            IPv4Private172,
            IPv4Private192,
        };

        /// <summary>
        /// IPv6 default unicast route ::/0 (everything)
        /// </summary>
        public static readonly IPNetwork AllIPv6 = Parse("::/0");

        /// <summary>
        /// IPv6 localhost range ::1/128
        /// </summary>
        public static readonly IPNetwork IPv6Loopback = Parse("::1/128");

        /// <summary>
        /// Private IPv6 range fc00::/7 (RFC1918 equivalent)
        /// </summary>
        public static readonly IPNetwork IPv6Private = Parse("fc00::/7");

        /// <summary>
        /// IPv6 Teredo range 2001::/32
        /// </summary>
        public static readonly IPNetwork Teredo = Parse("2001::/32");

        /// <summary>
        /// 6to4 IPv6 tunneling 2002::/16
        /// </summary>
        public static readonly IPNetwork IPv6to4 = Parse("2002::/16");

        /// <summary>
        /// Link-local IPv6 range fe80::/10
        /// </summary>
        public static readonly IPNetwork IPv6LinkLocal = Parse("fe80::/10");

        /// <summary>
        /// IPv6 Multicast range ff00::/8
        /// </summary>
        public static readonly IPNetwork IPv6Multicast = Parse("ff00::/8");

        /// <summary>
        /// IPv6 Orchid range 2001:0010::/28
        /// </summary>
        public static readonly IPNetwork IPv6Orchid = Parse("2001:0010::/28");

        /// <summary>
        /// All IPv4 and IPv6 addresses 0.0.0.0/0, ::/0
        /// </summary>
        public static readonly IPNetwork[] All = new IPNetwork[]
        {
            AllIPv4,
            AllIPv6
        };

        /// <summary>
        /// Private IPv4 and IPv6 addresses 10.0.0.0/8, 172.16.0.0/12, 192.168.0.0/16, fc00::/7, link-local, and localhost
        /// </summary>
        public static readonly IPNetwork[] PrivateNetworks = new IPNetwork[]
        {
            IPv4Loopback,
            IPv4LinkLocal,
            IPv4Private10,
            IPv4Private172,
            IPv4Private192,

            IPv6Loopback,
            IPv6LinkLocal,
            IPv6Private,
        };

        // Helper method for Parse/TryParse
        private static bool TryParsePrefixLength(IPAddress prefix, string[] parts, out byte length)
        {
            if (parts.Length > 1)
            {
                return byte.TryParse(parts[1], NumberStyles.Number, null, out length)
                    && !(length > 32 && prefix.AddressFamily == AddressFamily.InterNetwork)
                    && !(length > 128 && prefix.AddressFamily == AddressFamily.InterNetworkV6);
            }
            else if (prefix.AddressFamily == AddressFamily.InterNetworkV6)
            {
                length = 128;
                return true;
            }
            else if (prefix.AddressFamily == AddressFamily.InterNetwork)
            {
                length = 32;
                return true;
            }
            else
            {
                length = 0;
                return false;
            }
        }

        /// <summary>
        /// Determines whether a string is a valid IP network.
        /// </summary>
        /// <remarks>
        /// Prefix length is optional. Defaults to /32 for IPv4 and /128 for IPv6.
        /// </remarks>
        /// <example>
        /// IPNetwork ipn;
        /// if (IPNetwork.TryParse("198.51.100.0/27", out ipn))
        ///     Console.WriteLine("prefix/length = {0}/{1}", ipn.Prefix, ipn.PrefixLength);
        /// else
        ///     Console.WriteLine("Invalid network string");
        /// </example>
        /// <param name="networkString">A network string</param>
        /// <param name="network">An Microsoft.AspNetCore.HttpOverrides.IPNetwork instance if networkString is valid; otherwise null.</param>
        /// <returns>true if networkString is a valid IP address; otherwise, false.</returns>
        public static bool TryParse(string networkString, out IPNetwork network)
        {
            if (networkString == null)
            {
                network = null;
                return false;
            }

            IPAddress prefix;
            byte length;

            var parts = networkString.Split(PrefixLengthSeparators, 2);

            if (IPAddress.TryParse(parts[0], out prefix) && TryParsePrefixLength(prefix, parts, out length))
            {
                network = new IPNetwork(prefix, length);
                return true;
            }
            else
            {
                network = null;
                return false;
            }

        }

        /// <summary>
        /// Converts an IP network string to an Microsoft.AspNetCore.HttpOverrides.IPNetwork instance.
        /// </summary>
        /// <remarks>
        /// Prefix length is optional. Defaults to /32 for IPv4 and /128 for IPv6.
        /// </remarks>
        /// <example>
        /// IPNetwork.Parse("198.51.100.0/27");
        /// IPNetwork.Parse("2001:db8::/64");
        /// </example>
        /// <param name="networkString">A network string</param>
        /// <returns>An Microsoft.AspNetCore.HttpOverrides.IPNetwork instance</returns>
        public static IPNetwork Parse(string networkString)
        {
            if (networkString == null)
            {
                throw new ArgumentNullException(nameof(networkString));
            }

            var parts = networkString.Split(PrefixLengthSeparators, 2);

            var prefix = IPAddress.Parse(parts[0]);

            byte length;
            if (!TryParsePrefixLength(prefix, parts, out length))
            {
                throw new FormatException("Invalid prefix length");
            }

            return new IPNetwork(prefix, length);
        }

        public IPNetwork(IPAddress prefix)
            : this(prefix, prefix.AddressFamily == AddressFamily.InterNetwork ? 32 : 128)
        {
        }

        public IPNetwork(IPAddress prefix, int prefixLength)
        {
            Prefix = prefix;
            PrefixLength = prefixLength;
            PrefixBytes = Prefix.GetAddressBytes();
            Mask = CreateMask();
        }

        public IPAddress Prefix { get; }

        private byte[] PrefixBytes { get; }

        /// <summary>
        /// The CIDR notation of the subnet mask
        /// </summary>
        public int PrefixLength { get; }

        private byte[] Mask { get; }

        public bool Contains(IPAddress address)
        {
            if (Prefix.AddressFamily != address.AddressFamily)
            {
                return false;
            }

            var addressBytes = address.GetAddressBytes();
            for (int i = 0; i < PrefixBytes.Length && Mask[i] != 0; i++)
            {
                if (PrefixBytes[i] != (addressBytes[i] & Mask[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private byte[] CreateMask()
        {
            var mask = new byte[PrefixBytes.Length];
            int remainingBits = PrefixLength;
            int i = 0;
            while (remainingBits >= 8)
            {
                mask[i] = 0xFF;
                i++;
                remainingBits -= 8;
            }
            if (remainingBits > 0)
            {
                mask[i] = (byte)(0xFF << (8 - remainingBits));
            }

            return mask;
        }

        public override string ToString()
        {
            return Prefix.ToString() + "/" + PrefixLength;
        }
    }
}
