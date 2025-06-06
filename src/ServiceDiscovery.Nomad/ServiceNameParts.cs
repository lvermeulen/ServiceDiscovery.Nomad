﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace ServiceDiscovery.Nomad;

public readonly struct ServiceNameParts(string host, string? endPointName, int port) : IEquatable<ServiceNameParts>
{
    public string? EndPointName { get; init; } = endPointName;

    public string Host { get; init; } = host;

    public int Port { get; init; } = port;

    public override string ToString() => EndPointName is not null ? $"EndPointName: {EndPointName}, Host: {Host}, Port: {Port}" : $"Host: {Host}, Port: {Port}";

    public static bool TryParse(string serviceName, out ServiceNameParts parts)
    {
        if (serviceName.IndexOf("://", StringComparison.OrdinalIgnoreCase) < 0 && Uri.TryCreate($"fakescheme://{serviceName}", default, out var uri))
        {
            parts = Create(uri, hasScheme: false);
            return true;
        }

        if (Uri.TryCreate(serviceName, default, out uri))
        {
            parts = Create(uri, hasScheme: true);
            return true;
        }

        parts = default;
        return false;

        static ServiceNameParts Create(Uri uri, bool hasScheme)
        {
            var uriHost = uri.Host;
            var segmentSeparatorIndex = uriHost.IndexOf('.');
            string theHost;
            string? theEndPointName = null;
            var thePort = uri.Port > 0 ? uri.Port : 0;
            if (uriHost.StartsWith('_') && segmentSeparatorIndex > 1 && uriHost[^1] != '.')
            {
                theEndPointName = uriHost[1..segmentSeparatorIndex];

                // Skip the endpoint name, including its prefix ('_') and suffix ('.').
                theHost = uriHost[(segmentSeparatorIndex + 1)..];
            }
            else
            {
                theHost = uriHost;
                if (hasScheme)
                {
                    theEndPointName = uri.Scheme;
                }
            }

            return new(theHost, theEndPointName, thePort);
        }
    }

    public static bool TryCreateEndPoint(ServiceNameParts parts, [NotNullWhen(true)] out EndPoint? endPoint)
    {
        if (IPAddress.TryParse(parts.Host, out var ip))
        {
            endPoint = new IPEndPoint(ip, parts.Port);
        }
        else if (!string.IsNullOrEmpty(parts.Host))
        {
            endPoint = new DnsEndPoint(parts.Host, parts.Port);
        }
        else
        {
            endPoint = null;
            return false;
        }

        return true;
    }

    public static bool TryCreateEndPoint(string serviceName, [NotNullWhen(true)] out EndPoint? serviceEndPoint)
    {
        if (TryParse(serviceName, out var parts))
        {
            return TryCreateEndPoint(parts, out serviceEndPoint);
        }

        serviceEndPoint = null;
        return false;
    }

    public override bool Equals(object? obj) => obj is ServiceNameParts other && Equals(other);

    public bool Equals(ServiceNameParts other) => EndPointName == other.EndPointName
        && Host == other.Host
        && Port == other.Port;

    public override int GetHashCode() => HashCode.Combine(EndPointName, Host, Port);

    public static bool operator ==(ServiceNameParts left, ServiceNameParts right) => left.Equals(right);

    public static bool operator !=(ServiceNameParts left, ServiceNameParts right) => !(left == right);
}
