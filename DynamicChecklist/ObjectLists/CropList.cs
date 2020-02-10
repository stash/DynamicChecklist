namespace DynamicChecklist.ObjectLists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewValley;
    using StardewValley.TerrainFeatures;

    public class CropList : ObjectList
    {
        private Action action;
        private Func<TerrainFeature, bool> filter;

        public CropList(ModConfig config, Action action)
            : base(config)
        {
            this.action = action;
            switch (action)
            {
                case Action.Water:
                    this.ImageTexture = OverlayTextures.WateringCan;
                    this.OptionMenuLabel = "Water Crops";
                    this.TaskDoneMessage = "All crops have been watered";
                    this.Name = TaskName.Water;
                    this.filter = this.WaterFilter;
                    break;
                case Action.Harvest:
                    this.ImageTexture = OverlayTextures.Plus;
                    this.OptionMenuLabel = "Harvest Crops";
                    this.TaskDoneMessage = "All crops have been harvested";
                    this.Name = TaskName.Harvest;
                    this.filter = this.HarvestFilter;
                    break;
                case Action.PickTree:
                    this.ImageTexture = OverlayTextures.Plus;
                    this.OptionMenuLabel = "Pick Trees";
                    var number = "three fruits"; // TODO: "two or more fruits", "fruit"
                    this.TaskDoneMessage = $"All trees with {number} have been picked";
                    this.Name = TaskName.PickTree;
                    this.filter = this.PickTreeFilter;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public enum Action
        {
            Water, Harvest, PickTree
        }

        public override void BeforeDraw()
        {
            if (Game1.currentLocation == Game1.getFarm() || Game1.currentLocation == Game1.getLocationFromName("Greenhouse"))
            {
                this.UpdateObjectInfoList();
            }
        }

        protected override void UpdateObjectInfoList()
        {
            this.ObjectInfoList.Clear();

            this.AddFromLocation(Game1.getFarm());
            this.AddFromLocation(Game1.getLocationFromName("Greenhouse"));

            this.TaskDone = !this.ObjectInfoList.Any(soi => soi.NeedAction);
        }

        private static bool IsDead(HoeDirt dirt)
        {
            return dirt != null && dirt.crop != null && dirt.crop.dead.Value;
        }

        private static bool IsUnwatered(HoeDirt dirt)
        {
            return dirt != null &&
                dirt.state.Value != HoeDirt.watered &&
                dirt.needsWatering() &&
                !IsDead(dirt);
        }

        private bool WaterFilter(TerrainFeature terrainFeature)
        {
            if (terrainFeature is HoeDirt)
            {
                return IsUnwatered((HoeDirt)terrainFeature);
            }

            return false;
        }

        private bool IsHarvestable(HoeDirt dirt)
        {
            var crop = dirt?.crop;
            if (dirt == null || !dirt.readyForHarvest() || crop == null)
            {
                return false;
            }

            // TODO configure this feature:
            return ListHelper.ObjectCategories[crop.indexOfHarvest.Value] != StardewValley.Object.flowersCategory;
        }

        private bool HarvestFilter(TerrainFeature terrainFeature)
        {
            if (terrainFeature is HoeDirt)
            {
                return this.IsHarvestable((HoeDirt)terrainFeature);
            }

            return false;
        }

        private bool PickTreeFilter(TerrainFeature terrainFeature)
        {
            if (terrainFeature is FruitTree)
            {
                var tree = (FruitTree)terrainFeature;
                return tree.fruitsOnTree.Value >= 3;
            }

            return false;
        }

        private void AddFromLocation(GameLocation loc)
        {
            var range = from pair in loc.terrainFeatures.Pairs
                        let coordinate = pair.Key
                        let terrainFeature = pair.Value
                        where this.filter(terrainFeature)
                        select new StardewObjectInfo(coordinate, loc, true);
            this.ObjectInfoList.AddRange(range);
        }
    }
}
