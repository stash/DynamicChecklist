namespace DynamicChecklist.ObjectLists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Graph;
    using Graph.Graphs;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewValley;
    using StardewValley.Buildings;
    using StardewValley.Network;

    public class ListHelper
    {
        public static IEnumerable<AnimalHouse> GetFarmAnimalHouses()
        {
            foreach (Building b in Game1.getFarm().buildings)
            {
                var indoors = b.indoors.Value;
                if (indoors != null && indoors is AnimalHouse)
                {
                    yield return (AnimalHouse)indoors;
                }
            }
        }

        public static IEnumerable<KeyValuePair<Vector2, StardewValley.Object>> EnumerateObjects(OverlaidDictionary objects)
        {
            foreach (var pair in objects.Pairs)
            {
                yield return pair;
            }
        }
    }
}