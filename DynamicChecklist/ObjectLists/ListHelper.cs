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
        private static Dictionary<int, string> objectNames = null;

        private static Dictionary<int, int> objectCategories = null;

        internal static Dictionary<int, string> ObjectNames
        {
            get
            {
                if (objectNames == null)
                {
                    PopulateObjectsNames();
                }

                return objectNames;
            }
        }

        internal static Dictionary<int, int> ObjectCategories
        {
            get
            {
                if (objectCategories == null)
                {
                    PopulateObjectsNames();
                }

                return objectCategories;
            }
        }

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

        internal static void PopulateObjectsNames()
        {
            if (objectCategories != null)
            {
                objectCategories.Clear();
            }
            else
            {
                objectCategories = new Dictionary<int, int>();
            }

            if (objectNames != null)
            {
                objectNames.Clear();
            }
            else
            {
                objectNames = new Dictionary<int, string>();
            }

            foreach (var pair in Game1.objectInformation)
            {
                ObjectNames[pair.Key] = pair.Value.Split("/".ToCharArray())[0];
                var o = new StardewValley.Object(pair.Key, 0);
                ObjectCategories[pair.Key] = o.Category;
            }
        }
    }
}