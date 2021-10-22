using P3D.Legacy.Common.Data;
using P3D.Legacy.Common.Monsters;

using System.Collections.Generic;
using System.Linq;

namespace P3D.Legacy.Common.Extensions
{
    public static class DataItemsExtensions
    {
        //public static Monster[] DataItemsToMonsters(this DataItemStorage data) => data.ToString().Split('|').Select(str => str).Select(items => new Monster(new DataItemStorage(items))).ToArray();

        public static Dictionary<string, string> ToDictionary(this DataItemStorage data)
        {
            var dict = new Dictionary<string, string>();
            var str = data.ToString();
            str = str.Replace("{", "");
            //str = str.Replace("}", ",");
            var array = str.Split('}');
            foreach (var s in array.Reverse().Skip(1))
            {
                var v = s.Split('"');
                dict.Add(v[1], v[2].Replace("[", "").Replace("]", ""));
            }

            return dict;
        }
    }
}