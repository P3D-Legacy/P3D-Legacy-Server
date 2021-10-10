using System.Collections.Generic;

namespace P3D.Legacy.Common.Data
{
    public class BattleEndRoundData : P3DData
    {
        public static implicit operator BattleEndRoundData(string battleData) => new(battleData);

        public IReadOnlyList<string> Queries => ParseEndRoundData(Data);

        public BattleEndRoundData(string battleData) : base(battleData) { }


        private static IReadOnlyList<string> ParseEndRoundData(string data)
        {
            var newQueries = new List<string>();
            var tempData = string.Empty;

            //Converts the single string received as data into a list of string
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