using System.Collections.Generic;

namespace P3D.Legacy.Common.Data
{
    public class BattleHostData : P3DData
    {
        public static implicit operator BattleHostData(string battleData) => new(battleData);

        public IReadOnlyList<string> Queries => ParseHostData(Data);

        public BattleHostData(string battleData) : base(battleData) { }


        private static IReadOnlyList<string> ParseHostData(string data)
        {
            var newQueries = new List<string>();
            var tempData = string.Empty;

            while (data.Length > 0)
            {
                if (data[0] == '|' && tempData[^1] == '}')
                {
                    newQueries.Add(tempData);
                    tempData = "";
                }
                else
                {
                    tempData += data[0].ToString();
                }
                data = data.Remove(0, 1);
            }

            if (tempData.StartsWith("{") && tempData.EndsWith("}"))
            {
                newQueries.Add(tempData);
                tempData = "";
            }

            return newQueries;
        }
    }
}