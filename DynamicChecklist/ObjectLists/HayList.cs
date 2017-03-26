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
    class HayList : ObjectList
    {

        public override string OptionMenuLabel { get; protected set; }

        public override string TaskDoneMessage { get; protected set; }

        protected override Texture2D ImageTexture { get; set; }

        public HayList(ModConfig config) : base(config)
        {
            ImageTexture = OverlayTextures.Crab;
            OptionMenuLabel = "Filled Troughs";
            TaskDoneMessage = "All troughs have been filled";
            ObjectInfoList = new List<StardewObjectInfo>();
        }

        public override void BeforeDraw()
        {
            if (Game1.currentLocation.IsFarm && Game1.currentLocation is AnimalHouse)
            {               
                UpdateObjectInfoList((AnimalHouse)Game1.currentLocation);
                TaskDone = !(ObjectInfoList.Any(soi => soi.NeedAction));
            }

        }

        public override void OnMenuOpen()
        {
        }

        protected override void UpdateObjectInfoList()
        {
            foreach (Building b in Game1.getFarm().buildings)
            {
                if(b.indoors is AnimalHouse)
                {
                    UpdateObjectInfoList((AnimalHouse)b.indoors);
                }
            }
            TaskDone = !(ObjectInfoList.Any(soi => soi.NeedAction));
        }
        private void UpdateObjectInfoList(AnimalHouse animalHouse)
        {
            ObjectInfoList.RemoveAll(soi => soi.Location == animalHouse);
            foreach (KeyValuePair<Vector2, StardewValley.Object> o in animalHouse.Objects)
            {
                if (o.Value.Name.Equals("Hay"))
                {
                    var soi = new StardewObjectInfo();
                    soi.Coordinate = o.Key * Game1.tileSize + new Vector2(Game1.tileSize / 2, Game1.tileSize / 2);
                    soi.Location = animalHouse;
                }
            }
            //var tile = animalHouse.map.GetLayer("Back").
            var houseWidth = animalHouse.map.Layers[0].LayerWidth;
            var houseHeight = animalHouse.map.Layers[0].LayerHeight;
            for(int tileX=0; tileX < houseWidth; tileX++)
            {
                for (int tileY = 0; tileY < houseWidth; tileY++)
                {
                    bool tileIsTrough = animalHouse.doesTileHaveProperty(tileX, tileY, "Trough", "Back") != null;                    
                    if (tileIsTrough)
                    {
                        bool tileHasHay = animalHouse.Objects.ContainsKey(new Vector2(tileX, tileY));
                        var soi = new StardewObjectInfo();
                        soi.Coordinate = new Vector2((tileX+0.5f) * Game1.tileSize, (tileY + 0.5f) * Game1.tileSize);
                        soi.Location = animalHouse;
                        soi.NeedAction = !tileHasHay;
                        ObjectInfoList.Add(soi);
                    }
                }
            }
            //if (who.ActiveObject != null && who.ActiveObject.Name.Equals("Hay") && (this.doesTileHaveProperty(tileLocation.X, tileLocation.Y, "Trough", "Back") != null && !this.objects.ContainsKey(new Vector2((float) tileLocation.X, (float) tileLocation.Y))))
        }
    }
}
