using System.Globalization;

using Vogen;

namespace P3D.Legacy.Common;

[ValueObject<long>(conversions: Conversions.SystemTextJson | Conversions.TypeConverter, deserializationStrictness: DeserializationStrictness.AllowKnownInstances, comparison: ComparisonGeneration.Default)]
public readonly partial record struct Origin
{
    public static Origin None = new(0);
    public static Origin Server = new(-1);

    public bool IsNone => this == None;
    public bool IsServer => this == Server;
    public bool IsPlayer => Value > 0;

    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

    public static bool operator <(Origin left, Origin right) => left.CompareTo(right) < 0;
    public static bool operator >(Origin left, Origin right) => left.CompareTo(right) > 0;
    public static bool operator <=(Origin left, Origin right) => left.CompareTo(right) <= 0;
    public static bool operator >=(Origin left, Origin right) => left.CompareTo(right) >= 0;
}