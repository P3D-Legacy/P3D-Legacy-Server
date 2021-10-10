using System.Collections.Generic;

namespace P3D.Legacy.Common.Data
{
    // If LeadMonsterIndex is not null, this is a confirmation, else client just gives the pokemons for the battle
    public class BattleOfferData : P3DData
    {
        public static implicit operator BattleOfferData(string battleData) => new(battleData);

        public int? LeadMonsterIndex => int.TryParse(Data, out var index) ? (int?) index : null;
        //public IReadOnlyList<Monster> Monsters => ParseOfferData(Data);

        public BattleOfferData(string battleData) : base(battleData) { }

        /*
        private static IReadOnlyList<Monster> ParseOfferData(string data)
        {
            var monsters = new List<Monster>();
            var tempData = string.Empty;

            while (data.Length > 0)
            {
                if (data[0] == '|' && tempData[^1] == '}')
                {
                    monsters.Add(new Monster(tempData));
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
                monsters.Add(new Monster(tempData));
                tempData = "";
            }

            return monsters;
        }
        */
    }
}