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
    using StardewValley.Buildings;

    public class HayList : ObjectList
    {
        public HayList(ModConfig config)
            : base(config)
        {
            this.ImageTexture = OverlayTextures.Hand;
            this.OptionMenuLabel = "Filled Troughs";
            this.TaskDoneMessage = "All troughs have been filled";
            this.Name = TaskName.Hay;
            this.ObjectInfoList = new List<StardewObjectInfo>();
        }

        public override string OptionMenuLabel { get; protected set; }

        public override string TaskDoneMessage { get; protected set; }

        protected override Texture2D ImageTexture { get; set; }

        public override void BeforeDraw()
        {
            if (Game1.currentLocation.IsFarm && Game1.currentLocation is AnimalHouse)
            {
                this.UpdateObjectInfoList((AnimalHouse)Game1.currentLocation);
                this.TaskDone = !this.ObjectInfoList.Any(soi => soi.NeedAction);
            }
        }

        public override void OnMenuOpen()
        {
        }

        protected override void UpdateObjectInfoList()
        {
            foreach (var animalHouse in ListHelper.GetFarmAnimalHouses())
            {
                this.UpdateObjectInfoList(animalHouse);
            }

            this.TaskDone = !this.ObjectInfoList.Any(soi => soi.NeedAction);
        }

        private void UpdateObjectInfoList(AnimalHouse animalHouse)
        {
            this.ObjectInfoList.RemoveAll(soi => soi.Location == animalHouse);

            var houseWidth = animalHouse.map.Layers[0].LayerWidth;
            var houseHeight = animalHouse.map.Layers[0].LayerHeight;
            for (int tileX = 0; tileX < houseWidth; tileX++)
            {
                for (int tileY = 0; tileY < houseHeight; tileY++)
                {
                    bool tileIsTrough = animalHouse.doesTileHaveProperty(tileX, tileY, "Trough", "Back") != null;
                    if (tileIsTrough)
                    {
                        var loc = new Vector2(tileX, tileY);
                        bool tileHasHay = animalHouse.Objects.ContainsKey(loc);
                        this.ObjectInfoList.Add(new StardewObjectInfo(loc, animalHouse, !tileHasHay));
                    }
                }
            }
        }
    }
}
