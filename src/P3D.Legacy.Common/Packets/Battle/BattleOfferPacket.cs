using P3D.Legacy.Common.Data.P3DData;

namespace P3D.Legacy.Common.Packets.Battle
{
    public sealed record BattleOfferPacket() : P3DPacket(P3DPacketType.BattleOffer)
    {
        public Origin DestinationPlayerId { get => DataItemStorage.GetOrigin(0); init => DataItemStorage.Set(0, value); }
        public BattleOfferData BattleData { get => new(DataItemStorage.Get(1)); init => DataItemStorage.Set(1, value.ToP3DString()); }

        public void Deconstruct(out Origin destinationPlayerId, out BattleOfferData battleData)
        {
            destinationPlayerId = DestinationPlayerId;
            battleData = BattleData;
        }
    }
}