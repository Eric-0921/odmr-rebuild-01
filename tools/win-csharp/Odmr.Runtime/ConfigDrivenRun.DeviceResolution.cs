using System.IO.Ports;
using Odmr.Devices;

namespace Odmr.Runtime;

internal sealed record ConfigMagAxis(
    string AxisId,
    StationDeviceSpec Device,
    M8812AxisSession Session);

internal sealed record RuntimeResolvedConnections(
    LockinConnectionFacts Lockin,
    string SmbTransport,
    string? SmbHost,
    int? SmbPort,
    string? SmbResource,
    IReadOnlyDictionary<string, string> MagPorts,
    string? LaserPort);

public static partial class ConfigDrivenRun
{
    private static RuntimeResolvedConnections ResolveRuntimeConnections(RunConfigBundle bundle, string eventsPath, long processStart)
    {
        var smbDevice = bundle.Station.Devices.First(device => device.DeviceId == "smb100a_main");
        var lockinConnection = ResolveLockinConnection(bundle, eventsPath, processStart);
        var resolvedSmbConnection = ResolveSmbConnection(smbDevice, bundle.Connections, eventsPath, processStart, bundle.Plan.RunId);
        var magPorts = bundle.ResolvedPlan.RequiresMagneticControl
            ? ResolveMagPorts(bundle, eventsPath, processStart)
            : new Dictionary<string, string>();

        return new RuntimeResolvedConnections(
            lockinConnection,
            resolvedSmbConnection.Transport,
            resolvedSmbConnection.Host,
            resolvedSmbConnection.Port,
            resolvedSmbConnection.Resource,
            magPorts,
            bundle.Connections.LaserPort);
    }

    private static LockinConnectionFacts ResolveLockinConnection(RunConfigBundle bundle, string eventsPath, long processStart)
    {
        var lockinDevice = bundle.Station.Devices.First(device => device.DeviceId == bundle.Connections.Lockin.DeviceId);
        return bundle.OeProfile.NormalizedModel switch
        {
            LockinModelNames.Oe1022d => bundle.Connections.Lockin with
            {
                Resource = ResolveOe1022dResource(lockinDevice, bundle.Connections.Lockin.BaudRate ?? Oe1022dDefaults.BaudRate, eventsPath, processStart, bundle.Plan.RunId)
            },
            LockinModelNames.Oe1300 => bundle.Connections.Lockin with
            {
                Host = ResolveOe1300Host(lockinDevice, bundle.Connections.Lockin.Port ?? Oe1300Defaults.TcpPort, eventsPath, processStart, bundle.Plan.RunId),
                Port = bundle.Connections.Lockin.Port ?? Oe1300Defaults.TcpPort
            },
            _ => throw new InvalidOperationException($"unsupported lockin model: {bundle.OeProfile.NormalizedModel}")
        };
    }

    private static List<ConfigMagAxis> OpenMagAxes(RunConfigBundle bundle, RuntimeResolvedConnections resolvedConnections)
    {
        var axes = new List<ConfigMagAxis>
        {
            OpenMagAxis(bundle, "mag_x", resolvedConnections.MagPorts["mag_x"]),
            OpenMagAxis(bundle, "mag_y", resolvedConnections.MagPorts["mag_y"]),
            OpenMagAxis(bundle, "mag_z", resolvedConnections.MagPorts["mag_z"])
        };

        foreach (var axis in axes)
        {
            axis.Session.Clear();
            var idn = axis.Session.QueryIdn();
            foreach (var token in axis.Device.Identity?.ContainsAll ?? Array.Empty<string>())
            {
                if (!idn.Contains(token, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"{axis.AxisId} identity mismatch: expected token `{token}`, idn={idn}");
                }
            }

            axis.Session.SetRemote();
        }

        return axes;
    }

    private static ConfigMagAxis OpenMagAxis(RunConfigBundle bundle, string axisId, string port)
    {
        var device = bundle.Station.Devices.First(device => device.DeviceId == axisId);
        return new ConfigMagAxis(axisId, device, M8812Serial.Open(port));
    }

    private static string ResolveOe1022dResource(StationDeviceSpec device, int baudRate, string eventsPath, long processStart, string runId)
    {
        var candidates = new List<string>();
        AppendUnique(candidates, device.TransportHint.Resource);
        foreach (var candidate in device.TransportHint.ResourceCandidates ?? Array.Empty<string>())
        {
            AppendUnique(candidates, candidate);
        }

        foreach (var candidate in Oe1022dVisa.ListResources())
        {
            AppendUnique(candidates, candidate);
        }

        var failures = new List<string>();
        foreach (var resource in candidates)
        {
            try
            {
                using var oe = Oe1022dVisa.Open(resource, baudRate);
                var idn = oe.QueryIdn();
                if (IdentityMatches(device.Identity, idn))
                {
                    AppendEvent(eventsPath, processStart, runId, "device_resolved", "resolve", null, device.DeviceId, new
                    {
                        transport = "visa_resource",
                        resource,
                        idn
                    });
                    return resource;
                }

                failures.Add($"{resource}: identity mismatch idn={idn}");
            }
            catch (Exception ex)
            {
                failures.Add($"{resource}: {ex.Message}");
            }
        }

        throw new InvalidOperationException($"failed to resolve OE1022D resource for {device.DeviceId}: {string.Join(" | ", failures)}");
    }

    private static string ResolveOe1300Host(StationDeviceSpec device, int port, string eventsPath, long processStart, string runId)
    {
        var candidates = new List<string>();
        AppendUnique(candidates, device.TransportHint.Host);
        foreach (var candidate in device.TransportHint.HostCandidates ?? Array.Empty<string>())
        {
            AppendUnique(candidates, candidate);
        }

        var failures = new List<string>();
        foreach (var host in candidates)
        {
            try
            {
                using var oe = Oe1300Tcp.Open(host, port);
                var idn = oe.QueryIdn();
                if (IdentityMatches(device.Identity, idn))
                {
                    AppendEvent(eventsPath, processStart, runId, "device_resolved", "resolve", null, device.DeviceId, new
                    {
                        transport = "tcp_socket",
                        host,
                        port,
                        idn
                    });
                    return host;
                }

                failures.Add($"{host}:{port}: identity mismatch idn={idn}");
            }
            catch (Exception ex)
            {
                failures.Add($"{host}:{port}: {ex.Message}");
            }
        }

        throw new InvalidOperationException($"failed to resolve OE1300 host for {device.DeviceId}: {string.Join(" | ", failures)}");
    }

    private static (string Transport, string? Host, int? Port, string? Resource) ResolveSmbConnection(
        StationDeviceSpec device,
        StationConnectionFacts connections,
        string eventsPath,
        long processStart,
        string runId)
    {
        return connections.SmbTransport switch
        {
            "visa_resource" => ResolveSmbResource(device, eventsPath, processStart, runId),
            "tcp_socket" => ResolveSmbHost(device, connections.SmbPort ?? Smb100aDefaults.Port, eventsPath, processStart, runId),
            _ => throw new InvalidOperationException($"unsupported SMB transport: {connections.SmbTransport}")
        };
    }

    private static (string Transport, string? Host, int? Port, string? Resource) ResolveSmbHost(StationDeviceSpec device, int port, string eventsPath, long processStart, string runId)
    {
        var candidates = new List<string>();
        AppendUnique(candidates, device.TransportHint.Host);
        foreach (var candidate in device.TransportHint.HostCandidates ?? Array.Empty<string>())
        {
            AppendUnique(candidates, candidate);
        }

        var failures = new List<string>();
        foreach (var host in candidates)
        {
            try
            {
                using var smb = Smb100aTcp.Open(host, port, Smb100aDefaults.TimeoutMs);
                var idn = smb.Query(Smb100aCommands.QueryIdn);
                if (IdentityMatches(device.Identity, idn))
                {
                    AppendEvent(eventsPath, processStart, runId, "device_resolved", "resolve", null, device.DeviceId, new
                    {
                        transport = "tcp_socket",
                        host,
                        port,
                        idn
                    });
                    return ("tcp_socket", host, port, null);
                }

                failures.Add($"{host}:{port}: identity mismatch idn={idn}");
            }
            catch (Exception ex)
            {
                failures.Add($"{host}:{port}: {ex.Message}");
            }
        }

        throw new InvalidOperationException($"failed to resolve SMB100A host for {device.DeviceId}: {string.Join(" | ", failures)}");
    }

    private static (string Transport, string? Host, int? Port, string? Resource) ResolveSmbResource(StationDeviceSpec device, string eventsPath, long processStart, string runId)
    {
        var candidates = new List<string>();
        AppendUnique(candidates, device.TransportHint.Resource);
        foreach (var candidate in device.TransportHint.ResourceCandidates ?? Array.Empty<string>())
        {
            AppendUnique(candidates, candidate);
        }

        foreach (var candidate in Smb100aVisa.ListResources())
        {
            AppendUnique(candidates, candidate);
        }

        var failures = new List<string>();
        foreach (var resource in candidates)
        {
            try
            {
                using var smb = Smb100aVisa.Open(resource);
                var idn = smb.Query(Smb100aCommands.QueryIdn);
                if (IdentityMatches(device.Identity, idn))
                {
                    AppendEvent(eventsPath, processStart, runId, "device_resolved", "resolve", null, device.DeviceId, new
                    {
                        transport = "visa_resource",
                        resource,
                        idn
                    });
                    return ("visa_resource", null, null, resource);
                }

                failures.Add($"{resource}: identity mismatch idn={idn}");
            }
            catch (Exception ex)
            {
                failures.Add($"{resource}: {ex.Message}");
            }
        }

        throw new InvalidOperationException($"failed to resolve SMB100A VISA resource for {device.DeviceId}: {string.Join(" | ", failures)}");
    }

    private static ISmb100aSession OpenSmbSession(RuntimeResolvedConnections resolvedConnections)
    {
        return resolvedConnections.SmbTransport switch
        {
            "visa_resource" => Smb100aVisa.Open(resolvedConnections.SmbResource ?? throw new InvalidOperationException("SMB VISA resource missing")),
            "tcp_socket" => Smb100aTcp.Open(
                resolvedConnections.SmbHost ?? throw new InvalidOperationException("SMB TCP host missing"),
                resolvedConnections.SmbPort ?? throw new InvalidOperationException("SMB TCP port missing")),
            _ => throw new InvalidOperationException($"unsupported SMB transport: {resolvedConnections.SmbTransport}")
        };
    }

    private static IReadOnlyDictionary<string, string> ResolveMagPorts(RunConfigBundle bundle, string eventsPath, long processStart)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var axisId in new[] { "mag_x", "mag_y", "mag_z" })
        {
            var device = bundle.Station.Devices.First(device => device.DeviceId == axisId);
            result[axisId] = ResolveMagPort(device, eventsPath, processStart, bundle.Plan.RunId);
        }

        return result;
    }

    private static string ResolveMagPort(StationDeviceSpec device, string eventsPath, long processStart, string runId)
    {
        var candidates = new List<string>();
        AppendUnique(candidates, device.TransportHint.PortPath);
        foreach (var candidate in device.TransportHint.PortCandidates ?? Array.Empty<string>())
        {
            AppendUnique(candidates, candidate);
        }

        foreach (var candidate in SerialPort.GetPortNames().OrderBy(name => name, StringComparer.OrdinalIgnoreCase))
        {
            AppendUnique(candidates, candidate);
        }

        var failures = new List<string>();
        foreach (var port in candidates)
        {
            try
            {
                using var session = M8812Serial.Open(port);
                session.Clear();
                var idn = session.QueryIdn();
                if (IdentityMatches(device.Identity, idn))
                {
                    AppendEvent(eventsPath, processStart, runId, "device_resolved", "resolve", null, device.DeviceId, new
                    {
                        transport = "serial_port",
                        port,
                        idn
                    });
                    return port;
                }

                failures.Add($"{port}: identity mismatch idn={idn}");
            }
            catch (Exception ex)
            {
                failures.Add($"{port}: {ex.Message}");
            }
        }

        throw new InvalidOperationException($"failed to resolve M8812 port for {device.DeviceId}: {string.Join(" | ", failures)}");
    }

    private static bool IdentityMatches(StationIdentity? identity, string idn)
    {
        if (identity is null)
        {
            return true;
        }

        var containsAll = identity.ContainsAll ?? Array.Empty<string>();
        var containsAny = identity.ContainsAny ?? Array.Empty<string>();
        if (containsAll.Any(token => !idn.Contains(token, StringComparison.Ordinal)))
        {
            return false;
        }

        return containsAny.Count == 0 || containsAny.Any(token => idn.Contains(token, StringComparison.Ordinal));
    }

    private static void AppendUnique(List<string> values, string? candidate)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return;
        }

        if (!values.Contains(candidate, StringComparer.OrdinalIgnoreCase))
        {
            values.Add(candidate);
        }
    }
}
