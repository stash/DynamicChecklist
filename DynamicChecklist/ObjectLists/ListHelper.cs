﻿namespace DynamicChecklist.ObjectLists
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
        private static Dictionary<int, string> objectNames = new Dictionary<int, string>();
        private static Dictionary<string, int> objectIndexes = new Dictionary<string, int>();
        private static Dictionary<int, int> objectCategories = new Dictionary<int, int>();
        private static Dictionary<string, bool> locationHasWater = new Dictionary<string, bool>();

        internal static Dictionary<int, string> ObjectNames
        {
            get
            {
                if (objectNames.Count == 0)
                {
                    PopulateObjectsNames();
                }

                return objectNames;
            }
        }

        internal static Dictionary<string, int> ObjectIndexes {
            get {
                if (objectIndexes.Count == 0)
                {
                    PopulateObjectsNames();
                }

                return objectIndexes;
            }
        }

        internal static Dictionary<int, int> ObjectCategories
        {
            get
            {
                if (objectCategories.Count == 0)
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

        /// <summary>
        /// Returns the currently active locations (i.e., locations with a player)
        /// </summary>
        /// <returns>currently active locations</returns>
        public static IEnumerable<GameLocation> GetActiveLocations()
        {
            // TODO: only return actually active locations
            return Game1.locations.AsEnumerable();
        }

        public static bool LocationHasWater(GameLocation location)
        {
            var name = location.NameOrUniqueName;
            if (!locationHasWater.ContainsKey(name))
            {
                bool foundWater = false;
                if (location.waterTiles != null)
                {
                    foreach (var t in location.waterTiles)
                    {
                        if (t)
                        {
                            foundWater = true;
                            break;
                        }
                    }
                }

                locationHasWater[name] = foundWater;
            }

            return locationHasWater[name];
        }

        internal static void PopulateObjectsNames()
        {
            objectCategories.Clear();
            objectNames.Clear();
            objectIndexes.Clear();

            foreach (var pair in Game1.objectInformation)
            {
                string name = pair.Value.Split("/".ToCharArray())[0];
                int index = pair.Key;
                objectNames[index] = name;
                objectIndexes[name] = index;
                var o = new StardewValley.Object(index, 0);
                objectCategories[index] = o.Category;
            }
        }
    }
}