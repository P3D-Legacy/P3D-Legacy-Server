namespace P3D.Legacy.Common.Data
{
    public abstract class P3DData
    {
        public static implicit operator string(P3DData tradeData) => tradeData.Data;

        protected string Data { get; }

        protected P3DData(string data) => Data = data;
    }
}