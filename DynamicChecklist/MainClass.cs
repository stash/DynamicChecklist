using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using DynamicChecklist.ObjectLists;
using DynamicChecklist.Graph.Graphs;

namespace DynamicChecklist
{
    public class MainClass : Mod
    {
        public Keys OpenMenuKey = Keys.NumPad1;
        private ModConfig config;
        private IModHelper helper;
        private List<ObjectList> objectLists = new List<ObjectList>();
        private CompleteGraph graph;
        private OpenChecklistButton checklistButton;        
        
        public override void Entry(IModHelper helper)
        {    
            // TODO: Controller support       
            this.helper = helper;
            config = helper.ReadConfig<ModConfig>();
            helper.WriteConfig(config);
            // Menu Events
            MenuEvents.MenuChanged += MenuChangedEvent;
            ControlEvents.KeyPressed += this.ReceiveKeyPress;
            SaveEvents.AfterLoad += this.GameLoadedEvent;
            GameEvents.GameLoaded += this.onGameLoaded;
            TimeEvents.DayOfMonthChanged += this.OnDayOfMonthChanged;
            GraphicsEvents.OnPreRenderHudEvent += this.drawTick;
            GameEvents.OneSecondTick += this.UpdatePaths;
            LocationEvents.CurrentLocationChanged += this.UpdatePaths;
            try
            {
                OpenMenuKey = (Keys)Enum.Parse(typeof(Keys), config.OpenMenuKey);
            }
            catch
            {
                // use default value
            }           
        }
        private void drawTick(object sender, EventArgs e)
        {
            if (Game1.currentLocation == null || Game1.gameMode == 11 || Game1.currentMinigame != null || Game1.showingEndOfNightStuff || Game1.gameMode == 6 || Game1.gameMode == 0 || Game1.menuUp || Game1.activeClickableMenu != null)
            {
                return;
            }

            foreach (ObjectList ol in objectLists)
            {
                ol.BeforeDraw();
                ol.Draw(Game1.spriteBatch);
            }
            checklistButton.draw(Game1.spriteBatch);
        }
        private void UpdatePaths(object sender, EventArgs e)
        {
            if (Game1.currentLocation == null || Game1.gameMode == 11 || Game1.currentMinigame != null || Game1.showingEndOfNightStuff || Game1.gameMode == 6 || Game1.gameMode == 0 || Game1.menuUp || Game1.activeClickableMenu != null)
            {
                return;
            }
            if (graph.LocationInGraph(Game1.currentLocation))
            {
                graph.SetPlayerPosition(Game1.currentLocation, Game1.player.Position);
                graph.Calculate(Game1.currentLocation);
                foreach (ObjectList ol in objectLists)
                {
                    if (ol.OverlayActive)
                    {
                        ol.UpdatePath();
                    }
                }
            }
            else
            {
                foreach (ObjectList ol in objectLists)
                {
                    ol.ClearPath();
                }
            }
        }
        private void OnDayOfMonthChanged(object sender, EventArgs e)
        {
            foreach (ObjectList ol in objectLists)
            {
                ol.OnNewDay();
            }
        }
        private void showTaskDoneMessage(object sender, EventArgs e)
        {
            var s = (ObjectList)sender;
            Game1.showGlobalMessage(s.TaskDoneMessage);
        }
        private void OnOverlayActivated(object sender, EventArgs e)
        {
            graph.Calculate(Game1.currentLocation);
            var activatedObjectList = (ObjectList)sender;
            activatedObjectList.UpdatePath();
            if (!config.AllowMultipleOverlays)
            {
                foreach (ObjectList ol in objectLists)
                {
                    if (ol != sender)
                    {
                        ol.OverlayActive = false;
                    }

                }
            }

        }
        private void onGameLoaded(object sender, EventArgs e)
        {
            OverlayTextures.loadTextures(helper.DirectoryPath);
        }
        private void initializeObjectLists()
        {
            var ListNames = (TaskName[])Enum.GetValues(typeof(TaskName));
            foreach (var ListName in ListNames)
            {
                switch (ListName)
                {
                    case TaskName.Milk:
                        objectLists.Add(new AnimalList(config, AnimalList.Action.Milk));
                        break;
                    case TaskName.Pet:
                        objectLists.Add(new AnimalList(config, AnimalList.Action.Pet));
                        break;
                    case TaskName.Shear:
                        objectLists.Add(new AnimalList(config, AnimalList.Action.Shear));
                        break;
                    case TaskName.CrabPot:
                        objectLists.Add(new CrabPotList(config));
                        break;
                    case TaskName.Hay:
                        objectLists.Add(new HayList(config));
                        break;
                    case TaskName.Egg:
                        objectLists.Add(new EggList(config));
                        break;
                    case TaskName.Water:
                        objectLists.Add(new ObjectLists.CropList(config, ObjectLists.CropList.Action.Water));
                        break;
                    case TaskName.Harvest:
                        objectLists.Add(new ObjectLists.CropList(config, ObjectLists.CropList.Action.Harvest));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
            foreach (ObjectList ol in objectLists)
            {
                ol.OnNewDay();
            }
            ObjectList.Graph = graph;

            foreach (ObjectList o in objectLists)
            {
                o.TaskFinished += new EventHandler(showTaskDoneMessage);
                o.OverlayActivated += new EventHandler(OnOverlayActivated);
            }
        }
        private Texture2D loadTexture(String texName)
        {
            var textureStream = new FileStream(Path.Combine(Helper.DirectoryPath, "Resources", texName), FileMode.Open);
            var t = Texture2D.FromStream(Game1.graphics.GraphicsDevice, textureStream);
            return t;
        }
        private void ReceiveKeyPress(object sender, EventArgsKeyPressed e)
        {
            if (e.KeyPressed == OpenMenuKey)
            {
                if (Game1.activeClickableMenu is ChecklistMenu)
                {
                    Game1.activeClickableMenu = null;
                    Game1.playSound("bigDeSelect");
                }
                else
                {
                    if (MenuAllowed())
                    {                        
                        ChecklistMenu.Open(config);
                    }
                }
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
        private void GameLoadedEvent(object sender, EventArgs e)
        {
            graph = new CompleteGraph(Game1.locations);
            graph.Populate();
            initializeObjectLists();
            ChecklistMenu.objectLists = objectLists;
            Func<int> crt = CountRemainingTasks;
            checklistButton = new OpenChecklistButton(()=>ChecklistMenu.Open(config), crt, config);
            Game1.onScreenMenus.Insert(0, checklistButton); // So that click are registered with priority
        }
        private int CountRemainingTasks()
        {

            return objectLists.FindAll(x => x.TaskLeft).Count;
        }
        public bool MenuAllowed()
        {
            if (((Game1.dayOfMonth <= 0 ? 0 : (Game1.player.CanMove ? 1 : 0))) != 0 && !Game1.dialogueUp && (!Game1.eventUp || Game1.isFestival() && Game1.CurrentEvent.festivalTimer <= 0) && Game1.currentMinigame == null && Game1.activeClickableMenu == null)
            {
                return true;              
            }
            else
            {
                return false;
            }
        }
    }

    public class ModConfig
    {
        public string OpenMenuKey = "NumPad1";
        public bool ShowAllTasks = false;
        public bool AllowMultipleOverlays = true;
        public Dictionary<TaskName, bool> IncludeTask;
        public enum ButtonLocation {BelowJournal, LeftOfJournal}
        public ButtonLocation OpenChecklistButtonLocation = ButtonLocation.BelowJournal;

        public ModConfig()
        {
            IncludeTask = new Dictionary<TaskName, bool>();
            var ListNames = (TaskName[])Enum.GetValues(typeof(TaskName));
            foreach (var ListName in ListNames)
            {
                IncludeTask.Add(ListName, true);
            }
        }

    }    

}
