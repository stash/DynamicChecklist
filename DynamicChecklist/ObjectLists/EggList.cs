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

    public class EggList : ObjectList
    {
        public EggList(ModConfig config)
            : base(config)
        {
            this.ImageTexture = OverlayTextures.Hand;
            this.OptionMenuLabel = "Collect Animal Products";
            this.TaskDoneMessage = "All animal products have been collected";
            this.Name = TaskName.Egg;
        }

        public override void BeforeDraw()
        {
            if (!this.TaskDone && Game1.currentLocation.IsFarm)
            {
                this.UpdateObjectInfoList();
                this.TaskDone = !this.ObjectInfoList.Any(soi => soi.NeedAction);
            }
        }

        protected override void UpdateObjectInfoList()
        {
            this.ObjectInfoList.Clear();

            foreach (var animalHouse in ListHelper.GetFarmAnimalHouses())
            {
                var range = from pair in animalHouse.Objects.Pairs
                            where pair.Value.IsSpawnedObject
                            let soi = new StardewObjectInfo(pair.Key, animalHouse, true)
                            select soi;
                this.ObjectInfoList.AddRange(range);
            }
        }
    }
}
