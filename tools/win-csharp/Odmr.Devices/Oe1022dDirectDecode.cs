using System.Buffers.Binary;

namespace Odmr.Devices;

public sealed record Oe1022dMeasurementField(string Key, string DisplayName, int Offset);

public sealed record Oe1022dStatusSnapshot(
    byte BRefSourceCode,
    byte BRefSlopeCode,
    double BRefCurrentFreqHz,
    byte BInputOverload,
    byte BGainOverload,
    byte BPllLocked);

public static class Oe1022dDirectDecode
{
    public const string DecodeMode = "measurement_fields_20x50_big_endian_v1";
    public const int SamplesPerFrame = 50;
    public const int ValueBytes = 8;
    public const int BRefSourceCodeOffset = 8504;
    public const int BRefCurrentFreqHzOffset = 8505;
    public const int BRefSlopeCodeOffset = 8521;
    public const int BInputOverloadOffset = 8779;
    public const int BGainOverloadOffset = 8780;
    public const int BPllLockedOffset = 8781;

    public static readonly Oe1022dMeasurementField[] MeasurementFields =
    [
        new("a_x", "A-X", 0),
        new("a_y", "A-Y", 400),
        new("a_freq", "A-Freq", 800),
        new("a_noise", "A-Noise", 1200),
        new("a_xh1", "A-Xh1", 1600),
        new("a_yh1", "A-Yh1", 2000),
        new("a_xh2", "A-Xh2", 2400),
        new("a_yh2", "A-Yh2", 2800),
        new("b_x", "B-X", 3200),
        new("b_y", "B-Y", 3600),
        new("b_freq", "B-Freq", 4000),
        new("b_noise", "B-Noise", 4400),
        new("b_xh1", "B-Xh1", 4800),
        new("b_yh1", "B-Yh1", 5200),
        new("b_xh2", "B-Xh2", 5600),
        new("b_yh2", "B-Yh2", 6000),
        new("auxadc1", "AUXADC1", 6400),
        new("auxadc2", "AUXADC2", 6800),
        new("auxadc3", "AUXADC3", 7200),
        new("auxadc4", "AUXADC4", 7600)
    ];

    public static Oe1022dStatusSnapshot ReadStatus(ReadOnlySpan<byte> payload) =>
        new(
            payload[BRefSourceCodeOffset],
            payload[BRefSlopeCodeOffset],
            ReadDoubleBigEndian(payload.Slice(BRefCurrentFreqHzOffset, ValueBytes)),
            payload[BInputOverloadOffset],
            payload[BGainOverloadOffset],
            payload[BPllLockedOffset]);

    public static double ReadMeasurementValue(ReadOnlySpan<byte> payload, int fieldIndex, int sampleIndex)
    {
        if (fieldIndex < 0 || fieldIndex >= MeasurementFields.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(fieldIndex));
        }

        if (sampleIndex < 0 || sampleIndex >= SamplesPerFrame)
        {
            throw new ArgumentOutOfRangeException(nameof(sampleIndex));
        }

        var field = MeasurementFields[fieldIndex];
        var valueOffset = field.Offset + sampleIndex * ValueBytes;
        return ReadDoubleBigEndian(payload.Slice(valueOffset, ValueBytes));
    }

    private static double ReadDoubleBigEndian(ReadOnlySpan<byte> span)
    {
        var rawBits = BinaryPrimitives.ReadInt64BigEndian(span);
        return BitConverter.Int64BitsToDouble(rawBits);
    }
}
