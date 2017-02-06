using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace DynamicChecklist
{

    public class MainClass : Mod
    {
        public ObjectCollection objectCollection;
        public Keys OpenMenuKey = Keys.NumPad1;
        private ModConfig Config; 

        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();

            // Menu Events
            MenuEvents.MenuChanged += MenuChangedEvent;
            ControlEvents.KeyPressed += this.ReceiveKeyPress;
            SaveEvents.AfterLoad += this.GameLoadedEvent;

            try
            {
                OpenMenuKey = (Keys)Enum.Parse(typeof(Keys), Config.OpenMenuKey);
            }
            catch
            {
                // use default value
            }

        }
        private void ReceiveKeyPress(object sender, EventArgsKeyPressed e)
        {
            if (e.KeyPressed != OpenMenuKey) return;
            if (Game1.activeClickableMenu is ChecklistMenu)
            {
                Game1.activeClickableMenu = null;
            }
            else
            {
                Game1.activeClickableMenu = new ChecklistMenu(objectCollection);
            }

        }
        public void MenuChangedEvent(object sender, EventArgsClickableMenuChanged e)
        {
            if (!(e.NewMenu is GameMenu))
            {
                return;
            }
            var gameMenu = e.NewMenu;

        }
        public void GameLoadedEvent(object sender, EventArgs e)
        {
            objectCollection = new ObjectCollection();
            var locs = Game1.locations;
            foreach (GameLocation loc in locs)
            {
                var o = loc.Objects;
                if (loc is Farm)
                {
                    objectCollection.cropList.update((Farm)loc);
                }
            }
        }
    }

    internal class ModConfig
    {
        public bool val { get; set; } = false;
        public String OpenMenuKey = "NumPad1";
    }
}
