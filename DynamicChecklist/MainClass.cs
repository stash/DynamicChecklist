﻿namespace DynamicChecklist
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Graph.Graphs;
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
        private CompleteGraph graph;
        private OpenChecklistButton checklistButton;
        private bool doneLoading;

        public static bool MenuAllowed =>
            Context.IsPlayerFree && !Game1.isFestival() && !Game1.showingEndOfNightStuff;

        public override void Entry(IModHelper helper)
        {
            ObjectList.Monitor = this.Monitor;
            ObjectList.Helper = this.Helper;

            this.config = this.Helper.ReadConfig<ModConfig>();
            this.config.Check();
            helper.WriteConfig(this.config);

            GameTexture.TextureDirectory = Path.Combine(this.Helper.DirectoryPath, "Resources");

            var events = helper.Events;
            events.Input.ButtonPressed += this.Input_ButtonPressed;
            events.GameLoop.SaveLoaded += this.GameLoop_SaveLoaded;
            events.GameLoop.Saved += this.GameLoop_Saved;
            events.GameLoop.DayStarted += this.GameLoop_DayStarted;
            events.GameLoop.DayEnding += this.GameLoop_DayEnding;
            events.Display.RenderingHud += this.Display_RenderingHud;
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

            if (this.graph.LocationInGraph(Game1.currentLocation))
            {
                this.graph.SetPlayerPosition(Game1.currentLocation, Game1.player.Position);
                this.graph.Calculate(Game1.currentLocation);
                foreach (ObjectList ol in this.objectLists)
                {
                    if (ol.OverlayActive)
                    {
                        ol.UpdatePath();
                    }
                }
            }
            else
            {
                foreach (ObjectList ol in this.objectLists)
                {
                    ol.ClearPath();
                }
            }
        }

        private void GameLoop_DayStarted(object sender, EventArgs e)
        {
            this.Monitor.Log("Day Started, waiting for first tick");
            this.doneLoading = true;
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
            foreach (var ol in this.objectLists)
            {
                ol.OnNewDay();
            }
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e)
        {
            this.Monitor.VerboseLog("Update Tick");
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
            this.graph.Calculate(Game1.currentLocation);
            var activatedObjectList = (ObjectList)sender;
            activatedObjectList.UpdatePath();
            if (!this.config.AllowMultipleOverlays)
            {
                foreach (var ol in this.objectLists)
                {
                    if (ol != sender)
                    {
                        ol.OverlayActive = false;
                    }
                }
            }
        }

        private void InitializeObjectLists()
        {
            this.objectLists.Clear();
            var listNames = (TaskName[])Enum.GetValues(typeof(TaskName));
            foreach (var listName in listNames)
            {
                switch (listName)
                {
                    case TaskName.Milk:
                        this.objectLists.Add(new AnimalList(this.config, AnimalList.Action.Milk));
                        break;
                    case TaskName.Pet:
                        this.objectLists.Add(new AnimalList(this.config, AnimalList.Action.Pet));
                        break;
                    case TaskName.Shear:
                        this.objectLists.Add(new AnimalList(this.config, AnimalList.Action.Shear));
                        break;
                    case TaskName.CrabPot:
                        this.objectLists.Add(new MachineList(this.config, MachineList.Action.CrabPot));
                        break;
                    case TaskName.Hay:
                        this.objectLists.Add(new HayList(this.config));
                        break;
                    case TaskName.Egg:
                        this.objectLists.Add(new EggList(this.config));
                        break;
                    case TaskName.Water:
                        this.objectLists.Add(new CropList(this.config, CropList.Action.Water));
                        break;
                    case TaskName.Harvest:
                        this.objectLists.Add(new CropList(this.config, CropList.Action.Harvest));
                        break;
                    case TaskName.PickTree:
                        this.objectLists.Add(new CropList(this.config, CropList.Action.PickTree));
                        break;
                    case TaskName.EmptyRefiner:
                        this.objectLists.Add(new MachineList(this.config, MachineList.Action.EmptyRefiner));
                        break;
                    case TaskName.EmptyCask:
                        this.objectLists.Add(new MachineList(this.config, MachineList.Action.EmptyCask));
                        break;
                    case TaskName.Birthday:
                        this.objectLists.Add(new NPCList(this.config, NPCList.Action.Birthday));
                        break;
                    case TaskName.Spouse:
                        this.objectLists.Add(new NPCList(this.config, NPCList.Action.Spouse));
                        break;
                    case TaskName.Child:
                        this.objectLists.Add(new NPCList(this.config, NPCList.Action.Child));
                        break;
                    case TaskName.TravellingMerchant:
                        this.objectLists.Add(new TravellingMerchantList(this.config));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            ObjectList.Graph = this.graph;

            foreach (ObjectList o in this.objectLists)
            {
                o.TaskFinished += new EventHandler(this.ShowTaskDoneMessage);
                o.OverlayActivated += new EventHandler(this.OnOverlayActivated);
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
            this.graph = new CompleteGraph(Game1.locations);
            this.graph.Populate();
            this.InitializeObjectLists();
            ChecklistMenu.ObjectLists = this.objectLists;
            Func<int> crt = this.CountRemainingTasks;
            this.checklistButton = new OpenChecklistButton(() => ChecklistMenu.Open(this.config), crt, this.config, this.Helper.Events);
            Game1.onScreenMenus.Insert(0, this.checklistButton); // So that click is registered with priority
            // NOTE: game hasn't had an update tick at this point
        }

        private int CountRemainingTasks()
        {
            return this.objectLists.Count(x => x.TaskLeft);
        }
    }
}
