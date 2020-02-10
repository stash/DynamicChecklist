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
    using StardewValley.Objects;
    using SObject = StardewValley.Object;

    /// <summary>
    /// Tasks for collecting animal produce (not just Eggs)
    /// </summary>
    public class EggList : ObjectList
    {
        public const int AutoGrabberId = 165;

        private static readonly int[] CollectableCategories = new int[]
        {
            SObject.sellAtPierres, // truffles
            SObject.sellAtPierresAndMarnies, // feathers, wool, etc.
            SObject.EggCategory,
            SObject.MilkCategory,
            SObject.meatCategory, // as of 1.4, nothing in this one?
        };

        private static readonly int[] CollectableObjects = new int[]
        {
            107 // Dinosaur Egg
        };

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
                            where this.IsCollectable(pair.Value) || this.IsAutoGrabberReady(pair.Value)
                            let soi = new StardewObjectInfo(pair.Key, animalHouse, true)
                            select soi;
                this.ObjectInfoList.AddRange(range);
            }

            // Scan farm to get things like Truffles
            var farm = Game1.getFarm();
            var farmCollectables = from pair in farm.Objects.Pairs
                                   where this.IsCollectable(pair.Value)
                                   let soi = new StardewObjectInfo(pair.Key, farm, true)
                                   select soi;
            this.ObjectInfoList.AddRange(farmCollectables);
        }

        private bool IsAutoGrabberReady(SObject obj)
        {
            // AutoGrabbers are Chest-likes (see also: JunimoHuts).
            // Do a contents count since `obj.readyForHarvest` is always false
            if (obj.bigCraftable.Value && obj.ParentSheetIndex == AutoGrabberId)
            {
                var chest = (Chest)obj.heldObject.Value;
                return chest.items.Count > 0;
            }

            return false;
        }

        private bool IsCollectable(SObject obj)
        {
            var index = obj.ParentSheetIndex;
            return obj.IsSpawnedObject &&
                (CollectableCategories.Contains(obj.Category) || CollectableObjects.Contains(index));
        }
    }
}
