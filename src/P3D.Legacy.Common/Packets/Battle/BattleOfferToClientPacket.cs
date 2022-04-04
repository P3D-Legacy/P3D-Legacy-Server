using P3D.Legacy.Common.Data.P3DData;

namespace P3D.Legacy.Common.Packets.Battle
{
    public sealed record BattleOfferToClientPacket() : P3DPacket(P3DPacketType.BattleOffer)
    {
        public BattleOfferData BattleData { get => new(DataItemStorage.Get(0)); init => DataItemStorage.Set(0, value.ToP3DString()); }

        public void Deconstruct(out BattleOfferData battleData)
        {
            battleData = BattleData;
        }
    }
}