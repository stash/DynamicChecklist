namespace DynamicChecklist.ObjectLists
{
    using System;
    using System.Linq;
    using StardewValley;
    using StardewValley.Objects;
    using StardewValley.TerrainFeatures;

    public class CropList : ObjectList
    {
        private const int TickOffset = 20;
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

        protected override void InitializeObjectInfoList()
        {
            // Initially use all locations
            foreach (var loc in Game1.locations)
            {
                this.AddFromLocation(loc);
            }
        }

        protected override void UpdateObjectInfoList(uint ticks)
        {
            var bigUpdate = (ticks % 60) == TickOffset;
            var locations = bigUpdate ? Game1.locations : ListHelper.GetActiveLocations();
            foreach (var loc in locations)
            {
                this.ObjectInfoList.RemoveAll(soi => soi.Location == loc);
                this.AddFromLocation(loc);
            }
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
            if (terrainFeature is HoeDirt dirt)
            {
                return IsUnwatered(dirt);
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
            if (terrainFeature is HoeDirt dirt)
            {
                return this.IsHarvestable(dirt);
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

            if (this.action == Action.Water || this.action == Action.Harvest)
            {
                var potRange = from pair in loc.Objects.Pairs
                               where pair.Value is IndoorPot pot && this.filter(pot.hoeDirt.Value)
                               let coordinate = pair.Key
                               select new StardewObjectInfo(coordinate, loc, true);
                this.ObjectInfoList.AddRange(potRange);
            }
        }
    }
}
