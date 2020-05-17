namespace DynamicChecklist.ObjectLists
{
    using System;
    using System.Collections.Generic;
    using DynamicChecklist.Graph2;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Network;

    public class RecipeList : ObjectList
    {
        private Dictionary<WorldPoint, TVReference> tvs = new Dictionary<WorldPoint, TVReference>();
        private bool recipeAvailable;
        private bool obtainedRecipe;
        private string recipeName;

        public RecipeList(ModConfig config, TaskName name)
            : base(config, name)
        {
            this.ImageTexture = GameTexture.QueenOfSauceHead;
            this.OptionMenuLabel = "Watch Queen of Sauce";
            this.TaskDoneMessage = "One step closer to culinary enlightenment";
        }

        protected override void InitializeObjectInfoList()
        {
            this.obtainedRecipe = false;

            var tvHack = new TVHack();
            this.recipeAvailable = tvHack.IsNewRecipeAvailable(out var recipeName);
            if (!this.recipeAvailable)
            {
                return;
            }

            this.recipeName = recipeName;
            foreach (var gameLocation in Game1.locations)
            {
                var location = LocationReference.For(gameLocation);
                foreach (var pair in gameLocation.objects.Pairs)
                {
                    if (pair.Value is StardewValley.Objects.TV)
                    {
                        this.AddTvRef(location, pair);
                    }
                }
            }

            // Watch for added or removed TVs
            Helper.Events.World.ObjectListChanged += this.World_ObjectListChanged;
        }

        protected override void UpdateObjectInfoList(uint ticks)
        {
            if (!this.recipeAvailable || this.obtainedRecipe)
            {
                return;
            }

            if (Game1.player.cookingRecipes.ContainsKey(this.recipeName))
            {
                this.TaskDone = true;
                this.obtainedRecipe = true;
                this.Cleanup();
                return;
            }
        }

        protected override void Cleanup()
        {
            Helper.Events.World.ObjectListChanged -= this.World_ObjectListChanged;
            this.tvs.Clear();
            base.Cleanup();
        }

        private void AddTvRef(LocationReference location, KeyValuePair<Vector2, StardewValley.Object> pair)
        {
            var pos = pair.Key;
            var worldPoint = new WorldPoint(location, (int)pos.X, (int)pos.Y);
            var soi = new StardewObjectInfo(worldPoint);
            var tvRef = new TVReference(worldPoint, soi);
            this.tvs.Add(worldPoint, tvRef);
            this.ObjectInfoList.Add(soi);
        }

        private void World_ObjectListChanged(object sender, StardewModdingAPI.Events.ObjectListChangedEventArgs e)
        {
            var location = LocationReference.For(e.Location);

            foreach (var removedPair in e.Removed)
            {
                var worldPoint = new WorldPoint(location, (int)removedPair.Key.X, (int)removedPair.Key.Y);
                if (this.tvs.TryGetValue(worldPoint, out var tvRef))
                {
                    this.ObjectInfoList.Remove(tvRef.ObjectInfo);
                }
            }

            foreach (var addedPair in e.Added)
            {
                if (addedPair.Value is StardewValley.Objects.TV)
                {
                    this.AddTvRef(location, addedPair);
                }
            }

            if (this.ObjectInfoList.Count == 0)
            {
                this.Cancel();
            }
            else if (!this.obtainedRecipe)
            {
                this.TaskDone = false;
            }
        }

        internal class TVReference
        {
            public TVReference(WorldPoint worldPoint, StardewObjectInfo soi)
            {
                this.WorldPoint = worldPoint;
                this.ObjectInfo = soi;
            }

            public WorldPoint WorldPoint { get; set; }

            public StardewObjectInfo ObjectInfo { get; set; }
        }

        /// <summary>
        /// Inherit from <see cref="StardewValley.Objects.TV"/> to get access to protected methods.
        /// </summary>
        internal class TVHack : StardewValley.Objects.TV
        {
            public TVHack()
                : base()
            {
            }

            /// <summary>
            /// Does the current player know the recipe on TV today?
            /// </summary>
            /// <param name="recipeName">Name of today's recipe, if that could be determined</param>
            /// <returns>True if it's a recipe day AND the player doesn't know it</returns>
            public bool IsNewRecipeAvailable(out string recipeName)
            {
                recipeName = null;

                // Most of this logic has been pulled from decompiled `TV.getWeeklyRecipe` in SDV 1.4.
                // Can't actually call that method since that actually gives the player that recipe knowledge!
                int whichWeek;
                var day = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
                if (day.Equals("Wed"))
                {
                    whichWeek = this.getRerunWeek();
                }
                else if (day.Equals("Sun"))
                {
                    uint daysPlayedMod224 = Game1.stats.DaysPlayed % 224u;
                    whichWeek = (daysPlayedMod224 == 0) ? 32 : (int)(daysPlayedMod224 / 7u);
                }
                else
                {
                    return false;
                }

                var cookingRecipeChannel = Game1.temporaryContent.Load<Dictionary<string, string>>("Data\\TV\\CookingChannel");
                recipeName = cookingRecipeChannel[whichWeek.ToString()].Split('/')[0];
                return Game1.player.cookingRecipes.ContainsKey(recipeName); // does the player know it
            }
        }
    }
}
