using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Buildings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicChecklist.ObjectLists
{
    class EggList : ObjectList
    {
        public override string OptionMenuLabel { get; protected set; }

        public override string TaskDoneMessage { get; protected set; }

        protected override Texture2D ImageTexture { get; set; }

        public EggList(ModConfig config) : base(config)
        {
            ImageTexture = OverlayTextures.Heart;
            OptionMenuLabel = "Collect Animal Products";
            TaskDoneMessage = "All animal products have been collected";
            Name = TaskName.Egg;
            ObjectInfoList = new List<StardewObjectInfo>();
        }

        public override void BeforeDraw()
        {
            if (!TaskDone && Game1.currentLocation.IsFarm)
            {
                UpdateObjectInfoList();
            }
        }

        public override void OnMenuOpen()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateObjectInfoList()
        {
            this.ObjectInfoList.Clear();
            var farmBuildings = Game1.getFarm().buildings;
            foreach (Building building in farmBuildings)
            {
                if (building.indoors != null && building.indoors.GetType() == typeof(AnimalHouse))
                {
                    var animalHouse = (AnimalHouse)building.indoors;
                    foreach (KeyValuePair<Vector2, StardewValley.Object> obj in animalHouse.Objects)
                    {
                        if (obj.Value.IsSpawnedObject)
                        {
                            StardewObjectInfo soi = CreateSOI(obj, animalHouse);
                            ObjectInfoList.Add(soi);
                        }
                    }

                }
            }
            var taskDone = true;
            foreach (StardewObjectInfo soi in ObjectInfoList)
            {
                if (soi.NeedAction)
                {
                    taskDone = false;
                    break;
                }
            }
            TaskDone = taskDone;
        }
        private StardewObjectInfo CreateSOI(KeyValuePair<Vector2, StardewValley.Object> obj, GameLocation loc)
        {
            var soi = new StardewObjectInfo();
            soi.Coordinate = obj.Key*Game1.tileSize + new Vector2(Game1.tileSize/2, Game1.tileSize / 2);
            soi.Location = loc;
            soi.NeedAction = true;
            return soi;
        }
    }
}
