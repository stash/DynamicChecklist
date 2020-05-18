﻿namespace DynamicChecklist
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
        private ModConfig config;
        private List<ObjectList> objectLists = new List<ObjectList>();
        private OpenChecklistButton checklistButton;

        public static bool MenuAllowed =>
            Context.IsPlayerFree && !Game1.isFestival() && !Game1.showingEndOfNightStuff;

        public static WorldGraph WorldGraph { get; private set; }

        public override void Entry(IModHelper helper)
        {
            LocationReference.Setup(helper);

            ObjectList.Monitor = this.Monitor;
            ObjectList.Helper = this.Helper;
            WorldGraph.Monitor = this.Monitor;

            this.config = this.Helper.ReadConfig<ModConfig>();
            this.config.Check();
            helper.WriteConfig(this.config);

            GameTexture.TextureDirectory = Path.Combine(this.Helper.DirectoryPath, "Resources");

            var events = helper.Events;
            events.Display.RenderingHud += this.Display_RenderingHud;
            events.Input.ButtonPressed += this.Input_ButtonPressed;
            events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            events.GameLoop.Saved += this.GameLoop_Saved;
            events.GameLoop.DayStarted += this.GameLoop_DayStarted;
            events.GameLoop.DayEnding += this.GameLoop_DayEnding;
            events.World.LocationListChanged += this.World_LocationListChanged;

            events.GameLoop.OneSecondUpdateTicked += this.UpdatePaths;
            events.Player.Warped += this.UpdatePaths;

            try
            {
                this.openMenuKey = (Keys)Enum.Parse(typeof(Keys), this.config.OpenMenuKey);
            }
            catch
            {
                // use default value
            }
        }

        private void World_LocationListChanged(object sender, LocationListChangedEventArgs e)
        {
            WorldGraph.LocationListChanged(e.Added, e.Removed);
        }

        private void GameLoop_Saved(object sender, EventArgs e)
        {
            this.Helper.WriteConfig(this.config);
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
            this.Monitor.Log("Day Started, waiting for first tick");
            this.Helper.Events.GameLoop.UpdateTicked += this.FirstTickOfTheDay;
        }

        private void GameLoop_DayEnding(object sender, DayEndingEventArgs e)
        {
            this.Monitor.Log("Day Ending, disabling updates");
            this.Helper.Events.GameLoop.UpdateTicked -= this.GameLoop_UpdateTicked;
        }

        private void FirstTickOfTheDay(object sender, UpdateTickedEventArgs e)
        {
            this.Monitor.Log("First Tick of Day, enabling updates");
            this.Helper.Events.GameLoop.UpdateTicked -= this.FirstTickOfTheDay;
            this.Helper.Events.GameLoop.UpdateTicked += this.GameLoop_UpdateTicked;

            this.InitializeGraph();
            foreach (var ol in this.objectLists)
            {
                ol.OnNewDay();
            }
        }

        private void InitializeGraph()
        {
            this.Monitor.Log("Starting world graph generation...");
#if DEBUG
            var stopwatch = new Stopwatch();
            stopwatch.Start();
#endif
            WorldGraph = new WorldGraph(WorldGraph.AllLocations());
            WorldGraph.BuildAllInteriors();
            WorldGraph.BuildAllExteriors();
#if DEBUG
            stopwatch.Stop();
            this.Monitor.Log($"World graph generation done ({stopwatch.ElapsedMilliseconds} ms)!");

            var filename = Path.Combine(this.Helper.DirectoryPath, "world.dot");
            var gv = new WorldGraph.GraphViz(WorldGraph);
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

        private void ShowTaskDoneMessage(object sender, EventArgs e)
        {
            var s = (ObjectList)sender;
            Game1.showGlobalMessage(s.TaskDoneMessage);
        }

        private void OnOverlayActivated(object sender, EventArgs e)
        {
            var activatedObjectList = (ObjectList)sender;
            activatedObjectList.UpdatePath();
            if (!this.config.AllowMultipleOverlays)
            {
                foreach (var ol in this.objectLists)
                {
                    if (ol != sender)
                    {
                        ol.Enabled = false;
                    }
                }
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
                        this.objectLists.Add(new AnimalList(this.config, name));
                        break;
                    case TaskName.Hay:
                        this.objectLists.Add(new HayList(this.config, name));
                        break;
                    case TaskName.Egg:
                        this.objectLists.Add(new EggList(this.config, name));
                        break;
                    case TaskName.Water:
                    case TaskName.Harvest:
                    case TaskName.PickTree:
                        this.objectLists.Add(new CropList(this.config, name));
                        break;
                    case TaskName.CrabPot:
                    case TaskName.EmptyRefiner:
                    case TaskName.EmptyCask:
                        this.objectLists.Add(new MachineList(this.config, name));
                        break;
                    case TaskName.Birthday:
                    case TaskName.Spouse:
                    case TaskName.Child:
                    case TaskName.CareForPet:
                        this.objectLists.Add(new NPCList(this.config, name));
                        break;
                    case TaskName.TravellingMerchant:
                        this.objectLists.Add(new TravellingMerchantList(this.config, name));
                        break;
                    case TaskName.FishPond:
                        this.objectLists.Add(new FishPondList(this.config, name));
                        break;
                    case TaskName.JunimoHut:
                    case TaskName.Mill:
                        this.objectLists.Add(new BuildingOutputList(this.config, name));
                        break;
                    case TaskName.FarmCave:
                        this.objectLists.Add(new FarmCaveList(this.config, name));
                        break;
                    case TaskName.QueenOfSauce:
                        this.objectLists.Add(new RecipeList(this.config, name));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            foreach (ObjectList o in this.objectLists)
            {
                o.TaskFinished += new EventHandler(this.ShowTaskDoneMessage);
                o.WasEnabled += new EventHandler(this.OnOverlayActivated);
            }
        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e)
        {
            if (e.Button.ToString() == this.config.OpenMenuKey)
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
                        ChecklistMenu.Open(this.config);
                    }
                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, EventArgs e)
        {
            this.InitializeObjectLists();
            ChecklistMenu.ObjectLists = this.objectLists;
            Func<int> crt = this.CountRemainingTasks;
            this.checklistButton = new OpenChecklistButton(() => ChecklistMenu.Open(this.config), crt, this.config, this.Helper.Events);
            Game1.onScreenMenus.Insert(0, this.checklistButton); // So that click is registered with priority
            // NOTE: game hasn't had an update tick at this point
        }

        private int CountRemainingTasks()
        {
            return this.objectLists.Count(x => x.Enabled && !x.TaskDone);
        }
    }
}
