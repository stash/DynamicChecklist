namespace DynamicChecklist.ObjectLists
{
    using System.Collections.Generic;
    using System.Linq;
    using StardewValley;
    using StardewValley.Objects;
    using SObject = StardewValley.Object;

    /// <summary>
    /// Tasks for collecting animal produce (not just Eggs)
    /// </summary>
    public class EggList : ObjectList
    {
        public const int AutoGrabberId = 165;
        private const int TickOffset = 30;

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
            this.ImageTexture = GameTexture.Handbasket;
            this.OptionMenuLabel = "Collect Animal Products";
            this.TaskDoneMessage = "All animal products have been collected";
            this.Name = TaskName.Egg;
        }

        protected override void InitializeObjectInfoList()
        {
            this.UpdateObjectInfoList(TickOffset);
        }

        protected override void UpdateObjectInfoList(uint ticks)
        {
            // Ideally, only scan the farm every second when all farmers are absent.
            var bigUpdate = ticks % 60 == TickOffset;
            if (bigUpdate || ListHelper.IsActive(Game1.getFarm()))
            {
                this.ScanFarm();
            }

            var animalHouses = bigUpdate ? ListHelper.GetFarmAnimalHouses() : ListHelper.GetActiveFarmAnimalHouses();
            this.ScanAnimalHouses(animalHouses);
        }

        private void ScanAnimalHouses(IEnumerable<AnimalHouse> animalHouses)
        {
            // Scan barns/coops/etc. for collectables and autograbbers
            foreach (var animalHouse in animalHouses)
            {
                this.ObjectInfoList.RemoveAll(soi => soi.Location == animalHouse);
                var range = from pair in animalHouse.Objects.Pairs
                            where this.IsCollectable(pair.Value) || this.IsAutoGrabberReady(pair.Value)
                            let soi = new StardewObjectInfo(pair.Key, animalHouse, true)
                            select soi;
                this.ObjectInfoList.AddRange(range);
            }
        }

        private void ScanFarm()
        {
            // Scan farm to get things like Truffles
            var farm = Game1.getFarm();
            this.ObjectInfoList.RemoveAll(soi => soi.Location == farm);
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
