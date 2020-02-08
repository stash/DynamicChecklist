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
                    break;
                case Action.Harvest:
                    this.ImageTexture = OverlayTextures.Plus;
                    this.OptionMenuLabel = "Harvest Crops";
                    this.TaskDoneMessage = "All crops have been harvested";
                    this.Name = TaskName.Harvest;
                    break;
                default:
                    throw new NotImplementedException();
            }

            this.ObjectInfoList = new List<StardewObjectInfo>();
        }

        public enum Action
        {
            Water, Harvest
        }

        public override string OptionMenuLabel { get; protected set; }

        public override string TaskDoneMessage { get; protected set; }

        protected override Texture2D ImageTexture { get; set; }

        public override void OnMenuOpen()
        {
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

            this.TaskDone = this.CountNeedAction == 0;
        }

        private void AddFromLocation(GameLocation loc)
        {
            this.ObjectInfoList.AddRange(from pair in loc.terrainFeatures.Pairs
                                         let terrainFeature = pair.Value
                                         where terrainFeature is HoeDirt
                                         let coordinate = pair.Key
                                         let hoeDirt = (HoeDirt)terrainFeature
                                         let soi = this.CreateSOI(hoeDirt, coordinate, loc)
                                         select soi);
        }

        private StardewObjectInfo CreateSOI(HoeDirt hoeDirt, Vector2 coordinate, GameLocation loc)
        {
            bool needAction;
            switch (this.action)
            {
                case Action.Water:
                    var isWatered = hoeDirt.state.Value == HoeDirt.watered;
                    needAction = hoeDirt.needsWatering() && !isWatered && !hoeDirt.crop.dead.Value;
                    break;
                case Action.Harvest:
                    needAction = hoeDirt.readyForHarvest();
                    break;
                default:
                    throw new NotImplementedException();
            }

            return new StardewObjectInfo(coordinate, loc, needAction);
        }
    }
}
