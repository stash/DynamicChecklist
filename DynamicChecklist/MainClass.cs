using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
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
        private Texture2D cropsTexture;

        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();          
            // Menu Events
            MenuEvents.MenuChanged += MenuChangedEvent;
            ControlEvents.KeyPressed += this.ReceiveKeyPress;
            SaveEvents.AfterLoad += this.GameLoadedEvent;

            var c = new Crop();
            var b = helper.Reflection.GetPrivateMethod(c, "getSourceRect");
            try
            {
                OpenMenuKey = (Keys)Enum.Parse(typeof(Keys), Config.OpenMenuKey);
            }
            catch
            {
                // use default value
            }
            GameEvents.GameLoaded += this.loadTextures;

        }
        private void loadTextures(object sender, EventArgs e)
        {
            cropsTexture = loadTexture("Crops.png");
        }
        private Texture2D loadTexture(String texName)
        {
            var textureStream = new FileStream(Path.Combine(Helper.DirectoryPath, "Resources", texName), FileMode.Open);
            var t = Texture2D.FromStream(Game1.graphics.GraphicsDevice, textureStream);
            return t;
        }
        private void ReceiveKeyPress(object sender, EventArgsKeyPressed e)
        {
            //TODO ignore close menu when entering checkbox name
            if (e.KeyPressed != OpenMenuKey) return;
            if (Game1.activeClickableMenu is ChecklistMenu)
            {
                Game1.activeClickableMenu = null;
            }
            else
            {
                updateObjectCollection();
                ChecklistMenu.objectCollection = objectCollection;
                ChecklistMenu.Open();
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
            objectCollection = new ObjectCollection(cropsTexture);
            updateObjectCollection();
        }
        public void updateObjectCollection()
        {
            var locs = Game1.locations;
            foreach (GameLocation loc in locs)
            {
                var o = loc.Objects;
                if (loc is Farm)
                {
                    objectCollection.cropList.update((Farm)loc);
                }
            }
            objectCollection.crabTrapList.updateAll();
        }
    }

    internal class ModConfig
    {
        public bool val { get; set; } = false;
        public String OpenMenuKey = "NumPad1";
    }
}
