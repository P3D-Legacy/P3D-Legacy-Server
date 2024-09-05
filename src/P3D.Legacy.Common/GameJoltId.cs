using System.Globalization;

using Vogen;

namespace P3D.Legacy.Common
{
    [ValueObject<ulong>(conversions: Conversions.SystemTextJson | Conversions.TypeConverter, deserializationStrictness: DeserializationStrictness.AllowKnownInstances)]
    public readonly partial record struct GameJoltId
    {
        public static readonly GameJoltId None = new(0);

        public bool IsNone => this == None;

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);

        public bool Equals(ulong other) => Value == other;
    }
}