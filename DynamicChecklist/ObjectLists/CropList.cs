using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;

namespace DynamicChecklist.ObjectLists
{
    class CropList : ObjectList
    {
        public enum Action { Water, Harvest };
        private Action action;

        public override string OptionMenuLabel { get; protected set; }
        public override string TaskDoneMessage { get; protected set; }

        protected override Texture2D ImageTexture { get; set; }

        public CropList(ModConfig config, Action action) : base(config)
        {
            this.action = action;
            switch (action)
            {
                case Action.Water:
                    ImageTexture = OverlayTextures.WateringCan;
                    OptionMenuLabel = "Water Crops";
                    TaskDoneMessage = "All crops have been watered";
                    Name = TaskName.Water;
                    break;
                case Action.Harvest:
                    ImageTexture = OverlayTextures.Heart;
                    OptionMenuLabel = "Harvest Crops";
                    TaskDoneMessage = "All crops have been harvested";
                    Name = TaskName.Harvest;
                    break;
                default:
                    throw (new NotImplementedException());
            }
            
            ObjectInfoList = new List<StardewObjectInfo>();

        }

        public override void OnMenuOpen()
        {

        }

        public override void BeforeDraw()
        {
            if (Game1.currentLocation == Game1.getFarm())
            {
                UpdateObjectInfoList();
            }
        }

        protected override void UpdateObjectInfoList()
        {
            ObjectInfoList.Clear();
            Farm farm = Game1.getFarm();
            int nNeedWater = 0;
            foreach (KeyValuePair<Vector2, TerrainFeature> entry in farm.terrainFeatures)
            {
                var terrainFeature = entry.Value;               
                if (terrainFeature is HoeDirt)
                {                   
                    var coordinate = entry.Key;
                    var hoeDirt = (HoeDirt)terrainFeature;
                    StardewObjectInfo soi = CreateSOI(hoeDirt, coordinate, farm, action);
                    ObjectInfoList.Add(soi);
                    if (soi.NeedAction)
                    {
                        nNeedWater++;
                    }
                }
            }
            var a = this.TaskExistedAtStartOfDay;
            var b = this.TaskExistsNow;
            TaskDone = CountNeedAction == 0;
        }
        private StardewObjectInfo CreateSOI(HoeDirt hoeDirt, Vector2 coordinate, GameLocation loc, Action action)
        {
            var soi = new StardewObjectInfo();
            soi.Coordinate = coordinate*Game1.tileSize + new Vector2(Game1.tileSize / 2, Game1.tileSize / 2); ;
            soi.Location = loc;
            switch (action)
            {
                case Action.Water:
                    var isWatered = hoeDirt.state == StardewValley.TerrainFeatures.HoeDirt.watered;
                    soi.NeedAction = hoeDirt.needsWatering() && !isWatered && !hoeDirt.crop.dead;
                    break;
                case Action.Harvest:
                    soi.NeedAction = hoeDirt.readyForHarvest();
                    break;
                default:
                    throw (new NotImplementedException());
            }

            return soi;
        }
    }
}
