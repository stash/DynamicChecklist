namespace DynamicChecklist.ObjectLists
{
    using System.Linq;
    using StardewValley;
    using StardewValley.Locations;

    public class FarmCaveList : ObjectList
    {
        private const string MenuLabelGeneric = "Collect From Farm Cave";
        private const string MenuLabelSpecific = "Collect {type_plural} From Farm Cave";
        private const string TaskDoneGeneric = "Farm cave emptied";
        private const string TaskDoneSpecific = "Farm {type} cave emptied";
        private const int MushroomBoxSheetIndex = 128;
        private const int FruitCave = 1;
        private const int MushroomCave = 2;

        public FarmCaveList(ModConfig config, TaskName name)
            : base(config, name)
        {
            this.ImageTexture = GameTexture.Plus; // Just for fruit; mushrooms have built-in overlays
            this.OptionMenuLabel = MenuLabelGeneric;
            this.TaskDoneMessage = TaskDoneGeneric;
        }

        public static int CaveType => Game1.player.caveChoice.Value;

        public static bool IsAvailable => CaveType != 0;

        public static string CaveItemType => CaveType == FruitCave ? "Fruit" : "Mushroom";

        public static string CaveItemTypePlural => CaveType == FruitCave ? "Fruit" : "Mushrooms";

        protected override bool NeedsPerItemOverlay => CaveType == FruitCave; // Mushroom boxes already have built-in overlays

        protected override void InitializeObjectInfoList()
        {
            // Need to wait until first tick to determine the loaded game's cave type
            if (IsAvailable)
            {
                this.OptionMenuLabel = MenuLabelSpecific.Replace("{type_plural}", CaveItemTypePlural);
                this.TaskDoneMessage = TaskDoneSpecific.Replace("{type}", CaveItemType);
                this.UpdateObjectInfoList(0);
            }
        }

        protected override void UpdateObjectInfoList(uint ticks)
        {
            if (!IsAvailable)
            {
                return;
            }

            this.ObjectInfoList.Clear();
            var farmCaves = Game1.locations.OfType<FarmCave>(); // Should only be one, but who knows what other mods do!
            foreach (var farmCave in farmCaves)
            {
                var location = LocationReference.For(farmCave);
                foreach (var pair in farmCave.objects.Pairs)
                {
                    // The player can stick other machines (e.g., preserves jars) in the cave, so have to be choosy
                    var obj = pair.Value;
                    if ((CaveType == FruitCave && obj.CanBeGrabbed) ||
                        (CaveType == MushroomCave && obj.ParentSheetIndex == MushroomBoxSheetIndex && obj.readyForHarvest.Value))
                    {
                        this.ObjectInfoList.Add(new StardewObjectInfo(tileCoord: pair.Key, location));
                    }
                }
            }
        }
    }
}
