using System.Diagnostics;
using System.Globalization;
using Odmr.Devices;

namespace Odmr.WinProbe;

internal static class CliSupport
{
    internal static Dictionary<string, string> ParseOptions(string[] args)
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal);

        for (var i = 0; i < args.Length; i++)
        {
            var key = args[i];
            if (!key.StartsWith("--", StringComparison.Ordinal))
            {
                throw new ArgumentException($"unexpected argument: {key}");
            }

            if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                options[key[2..]] = "true";
                continue;
            }

            options[key[2..]] = args[++i];
        }

        return options;
    }

    internal static string GetRequiredOption(IReadOnlyDictionary<string, string> options, string key)
    {
        if (!options.TryGetValue(key, out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"missing required option --{key}");
        }

        return value;
    }

    internal static string GetOption(IReadOnlyDictionary<string, string> options, string key, string defaultValue) =>
        options.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : defaultValue;

    internal static string? GetOptionalOption(IReadOnlyDictionary<string, string> options, string key) =>
        options.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : null;

    internal static int GetIntOption(IReadOnlyDictionary<string, string> options, string key, int defaultValue)
    {
        if (!options.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            throw new ArgumentException($"--{key} must be an integer");
        }

        return parsed;
    }

    internal static bool GetBoolOption(IReadOnlyDictionary<string, string> options, string key, bool defaultValue)
    {
        if (!options.TryGetValue(key, out var value))
        {
            return defaultValue;
        }

        if (!bool.TryParse(value, out var parsed))
        {
            throw new ArgumentException($"--{key} must be true or false");
        }

        return parsed;
    }

    internal static int Fail(string message)
    {
        Console.Error.WriteLine(message);
        return 1;
    }

    internal static string UtcNowString() =>
        DateTimeOffset.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff'Z'", CultureInfo.InvariantCulture);

    internal static ulong MonotonicNsSince(long startTimestamp)
    {
        var ticks = Stopwatch.GetTimestamp() - startTimestamp;
        return (ulong)(ticks * 1_000_000_000.0 / Stopwatch.Frequency);
    }

    internal static string PathRelative(string root, string path) =>
        Path.GetRelativePath(root, path).Replace('\\', '/');

    internal static string ResolveSmbVisaResource()
    {
        var candidates = new List<string>();
        AppendUnique(candidates, Smb100aDefaults.Resource);
        AppendUnique(candidates, Smb100aDefaults.FallbackResource);
        foreach (var candidate in Smb100aVisa.ListResources())
        {
            AppendUnique(candidates, candidate);
        }

        var failures = new List<string>();
        foreach (var candidate in candidates)
        {
            try
            {
                using var smb = Smb100aVisa.Open(candidate);
                var idn = smb.Query(Smb100aCommands.QueryIdn);
                if (idn.Contains(Smb100aDefaults.RequiredVendor, StringComparison.Ordinal) &&
                    idn.Contains(Smb100aDefaults.RequiredModel, StringComparison.Ordinal))
                {
                    return candidate;
                }

                failures.Add($"{candidate}: identity mismatch idn={idn}");
            }
            catch (Exception ex)
            {
                failures.Add($"{candidate}: {ex.Message}");
            }
        }

        throw new InvalidOperationException($"failed to resolve SMB100A VISA resource: {string.Join(" | ", failures)}");
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
