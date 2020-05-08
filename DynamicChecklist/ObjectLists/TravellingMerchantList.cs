namespace DynamicChecklist.ObjectLists
{
    using System.Linq;
    using Microsoft.Xna.Framework;
    using StardewModdingAPI.Events;
    using StardewValley;
    using StardewValley.Locations;
    using StardewValley.Menus;

    public class TravellingMerchantList : ObjectList
    {
        private static readonly Vector2 MerchantTile = new Vector2(27, 11);
        private static readonly Vector2 DrawOffset = new Vector2(.75f, 0f); // displays above merchant, slightly right

        private bool visited = false;
        private bool isOpen = false;

        public TravellingMerchantList(ModConfig config, TaskName name)
            : base(config, name)
        {
            this.ImageTexture = GameTexture.TravellingMerchant;
            this.OptionMenuLabel = "Visit Travelling Merchant";
            this.TaskDoneMessage = "Travelling Merchant Visited!";
            Helper.Events.Display.MenuChanged += this.Display_MenuChanged;
        }

        protected override void InitializeObjectInfoList()
        {
            var forest = Game1.locations.OfType<Forest>().First();
            if (forest.travelingMerchantDay)
            {
                this.isOpen = true;
                this.visited = false;
                var soi = new StardewObjectInfo(MerchantTile, forest) { DrawOffset = DrawOffset };
                this.ObjectInfoList.Add(soi);
            }
        }

        protected override void UpdateObjectInfoList(uint ticks)
        {
            if (this.TaskDone || this.ObjectInfoList.Count == 0)
            {
                return;
            }

            this.isOpen = Game1.timeOfDay < 2000; // 8PM
            this.ObjectInfoList[0].DrawOffset = new Vector2(.75f * Game1.tileSize, -.75f * Game1.tileSize);
            if (!this.isOpen && !this.visited)
            {
                this.Cancel(); // cannot be completed
            }
            else if (this.visited && !this.TaskDone)
            {
                this.ObjectInfoList[0].NeedAction = false; // marks task as complete
            }
        }

        private void Display_MenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (this.TaskDone || this.ObjectInfoList.Count == 0)
            {
                return;
            }

            // Credit to GuiNoya of the DailyTasksReport mod for this beautiful hack!
            if (e.NewMenu is ShopMenu &&
                Game1.currentLocation is Forest &&
                Game1.player.GetGrabTile() == MerchantTile)
            {
                this.visited = true;
            }
        }
    }
}
