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

        private bool visited = false;
        private bool isOpen = false;

        public TravellingMerchantList(ModConfig config)
            : base(config)
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
                var coord = MerchantTile + new Vector2(.25f, -.5f); // display above merchant, slightly right
                this.ObjectInfoList.Add(new StardewObjectInfo(coord, forest));
            }
        }

        protected override void UpdateObjectInfoList(uint ticks)
        {
            if (this.TaskDone)
            {
                return;
            }

            this.isOpen = Game1.timeOfDay < 2000; // 8PM
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
            if (this.TaskDone)
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
