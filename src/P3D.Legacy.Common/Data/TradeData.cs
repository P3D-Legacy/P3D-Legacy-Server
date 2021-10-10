namespace P3D.Legacy.Common.Data
{
    public class TradeData : P3DData
    {
        public static implicit operator TradeData(string tradeData) => new(tradeData);

        //public Monster Monster => new Monster(Data);

        public TradeData(string tradeData) : base(tradeData) { }
    }
}