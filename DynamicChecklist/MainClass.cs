namespace DynamicChecklist
{
    using System;
    using System.Collections.Generic;
#if DEBUG
    using System.Diagnostics;
#endif
    using System.IO;
    using System.Linq;
    using DynamicChecklist.Graph2;
    using Microsoft.Xna.Framework.Input;
    using ObjectLists;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;

    public class MainClass : Mod
    {
        private Keys openMenuKey = Keys.NumPad1;
        private List<ObjectList> objectLists = new List<ObjectList>();
        private OpenChecklistButton checklistButton;

        public static bool MenuAllowed =>
            Context.IsPlayerFree && !Game1.isFestival() && !Game1.showingEndOfNightStuff;

        public static MainClass Instance { get; private set; }

        public static IModEvents Events => Instance.Helper.Events;

        public WorldGraph WorldGraph { get; private set; }

        public ModConfig Config { get; private set; }

        public static void Log(string message, LogLevel level = LogLevel.Trace) => Instance.Monitor.Log(message, level);

        public override void Entry(IModHelper helper)
        {
            Instance = this;
            LocationReference.Setup();

            this.Config = this.Helper.ReadConfig<ModConfig>();
            this.Config.Check();
            helper.WriteConfig(this.Config);

            GameTexture.TextureDirectory = Path.Combine(this.Helper.DirectoryPath, "Resources");

            Events.Display.RenderingHud += this.Display_RenderingHud;
            Events.Input.ButtonPressed += this.Input_ButtonPressed;
            Events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            Events.GameLoop.Saved += this.GameLoop_Saved;
            Events.GameLoop.DayStarted += this.GameLoop_DayStarted;
            Events.GameLoop.DayEnding += this.GameLoop_DayEnding;
            Events.GameLoop.ReturnedToTitle += this.GameLoop_ReturnedToTitle;
            Events.World.LocationListChanged += this.World_LocationListChanged;

            Events.GameLoop.OneSecondUpdateTicked += this.UpdatePaths;
            Events.Player.Warped += this.UpdatePaths;

            try
            {
                this.openMenuKey = (Keys)Enum.Parse(typeof(Keys), this.Config.OpenMenuKey);
            }
            catch
            {
                // use default value
            }
        }

        internal void OnOverlayActivated(ObjectList activatedObjectList)
        {
            activatedObjectList.UpdatePath();
            if (!this.Config.AllowMultipleOverlays)
            {
                foreach (var ol in this.objectLists)
                {
                    if (ol != activatedObjectList)
                    {
                        ol.Enabled = false;
                    }
                }
            }
        }

        private void GameLoop_ReturnedToTitle(object sender, ReturnedToTitleEventArgs e)
        {
            Log("Returning to title, disabling updates and disposing");
            Events.GameLoop.UpdateTicked -= this.GameLoop_UpdateTicked;
            foreach (var ol in this.objectLists)
            {
                ol.Dispose();
            }

            this.WorldGraph.Dispose();
            this.WorldGraph = null;
        }

        private void World_LocationListChanged(object sender, LocationListChangedEventArgs e)
        {
            this.WorldGraph.LocationListChanged(e.Added, e.Removed);
        }

        private void GameLoop_Saved(object sender, EventArgs e)
        {
            this.Helper.WriteConfig(this.Config);
        }

        private void Display_RenderingHud(object sender, EventArgs e)
        {
            if (!MenuAllowed || Game1.currentMinigame != null)
            {
                return;
            }

            foreach (var ol in this.objectLists)
            {
                ol.Draw(Game1.spriteBatch);
            }

            this.checklistButton.draw(Game1.spriteBatch);
        }

        private void UpdatePaths(object sender, EventArgs e)
        {
            if (!MenuAllowed || Game1.currentMinigame != null)
            {
                return;
            }

            foreach (ObjectList ol in this.objectLists)
            {
                ol.ClearPath();
                if (ol.Enabled)
                {
                    ol.UpdatePath();
                }
            }
        }

        private void GameLoop_DayStarted(object sender, EventArgs e)
        {
            Log("Day Started, waiting for first tick");
            Events.GameLoop.UpdateTicked += this.FirstTickOfTheDay;
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            Log("Day Ending, disabling updates");
            Events.GameLoop.UpdateTicked -= this.GameLoop_UpdateTicked;
            foreach (var ol in this.objectLists)
            {
                ol.Cleanup();
            }
        }

        private void FirstTickOfTheDay(object sender, UpdateTickedEventArgs e)
        {
            Log("First Tick of Day, enabling updates");
            Events.GameLoop.UpdateTicked -= this.FirstTickOfTheDay;
            Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;

            this.InitializeGraph();
            foreach (var ol in this.objectLists)
            {
                ol.OnNewDay();
            }
        }

        private void InitializeGraph()
        {
            Log("Starting world graph generation...");
#if DEBUG
            var stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            this.WorldGraph = new WorldGraph(WorldGraph.AllLocations());
            this.WorldGraph.BuildAllInteriors();
            this.WorldGraph.BuildAllExteriors();
#if DEBUG
            stopwatch.Stop();
            this.Monitor.Log($"World graph generation done ({stopwatch.ElapsedMilliseconds} ms)!");

            var filename = Path.Combine(this.Helper.DirectoryPath, "world.dot");
            var gv = new WorldGraph.GraphViz(this.WorldGraph);
            gv.Write(filename);
            this.Monitor.Log($"Wrote GraphViz to {filename}");
#endif
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            foreach (var ol in this.objectLists)
            {
                ol.OnUpdateTicked(e.Ticks);
            }
        }

        private void InitializeObjectLists()
        {
            this.objectLists.Clear();
            var taskNames = (TaskName[])Enum.GetValues(typeof(TaskName));
            foreach (var name in taskNames)
            {
                switch (name)
                {
                    case TaskName.Milk:
                    case TaskName.Pet:
                    case TaskName.Shear:
                        this.objectLists.Add(new AnimalList(name));
                        break;
                    case TaskName.Hay:
                        this.objectLists.Add(new HayList(name));
                        break;
                    case TaskName.Egg:
                        this.objectLists.Add(new EggList(name));
                        break;
                    case TaskName.Water:
                    case TaskName.Harvest:
                    case TaskName.PickTree:
                        this.objectLists.Add(new CropList(name));
                        break;
                    case TaskName.CrabPot:
                    case TaskName.EmptyRefiner:
                    case TaskName.EmptyCask:
                        this.objectLists.Add(new MachineList(name));
                        break;
                    case TaskName.Birthday:
                    case TaskName.Spouse:
                    case TaskName.Child:
                    case TaskName.CareForPet:
                        this.objectLists.Add(new NPCList(name));
                        break;
                    case TaskName.TravellingMerchant:
                        this.objectLists.Add(new TravellingMerchantList(name));
                        break;
                    case TaskName.FishPond:
                        this.objectLists.Add(new FishPondList(name));
                        break;
                    case TaskName.JunimoHut:
                    case TaskName.Mill:
                        this.objectLists.Add(new BuildingOutputList(name));
                        break;
                    case TaskName.FarmCave:
                        this.objectLists.Add(new FarmCaveList(name));
                        break;
                    case TaskName.QueenOfSauce:
                        this.objectLists.Add(new RecipeList(name));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button.ToString() == this.Config.OpenMenuKey)
            {
                if (Game1.activeClickableMenu is ChecklistMenu)
                {
                    Game1.activeClickableMenu = null;
                    Game1.playSound("bigDeSelect");
                }
                else
                {
                    if (MenuAllowed)
                    {
                        ChecklistMenu.Open(this.Config);
                    }
                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, EventArgs e)
        {
            this.InitializeObjectLists();
            ChecklistMenu.ObjectLists = this.objectLists;
            Func<int> crt = this.CountRemainingTasks;
            this.checklistButton = new OpenChecklistButton(() => ChecklistMenu.Open(this.Config), crt, this.Config, this.Helper.Events);
            Game1.onScreenMenus.Insert(0, this.checklistButton); // So that click is registered with priority
            // NOTE: game hasn't had an update tick at this point
        }

        private int CountRemainingTasks()
        {
            return this.objectLists.Count(x => x.Enabled && !x.TaskDone);
        }
    }
}
