namespace DynamicChecklist.ObjectLists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewValley;
    using StardewValley.Objects;

    public class MachineList : ObjectList
    {
        private readonly Action action;
        private readonly System.Action newDayAction;
        private readonly Func<GameLocation, bool> locationFilter = (loc) => true;
        private readonly Func<StardewValley.Object, bool> objectFilter = (obj) => true;

        public MachineList(ModConfig config, Action action)
            : base(config)
        {
            this.action = action;
            switch (action)
            {
                case Action.CrabPot:
                    this.newDayAction = () => this.IsPlayerLuremaster = Game1.player.professions.Contains(11);
                    this.locationFilter = (loc) => ListHelper.LocationHasWater(loc);
                    this.objectFilter = this.CrabPotTaskFilter;
                    this.OptionMenuLabel = "Collect From And Bait Crab Pots";
                    this.TaskDoneMessage = "All crab pots have been collected from and baited";
                    this.Name = TaskName.CrabPot;
                    break;
                case Action.EmptyRefiner:
                    this.objectFilter = this.GeneralTaskFilter;
                    this.OptionMenuLabel = "Empty Refining Machines";
                    this.TaskDoneMessage = "All refining machines have been emptied";
                    this.Name = TaskName.EmptyRefiner;
                    break;
                case Action.EmptyCask:
                    this.objectFilter = this.CaskTaskFilter;
                    this.OptionMenuLabel = "Bottle Casked Wine";
                    var quality = "Iridium"; // TODO: "Gold or higher", "Silver or higher"
                    this.TaskDoneMessage = $"All {quality} quality casks have been emptied";
                    this.Name = TaskName.EmptyCask;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public enum Action
        {
            CrabPot, EmptyRefiner, EmptyCask
        }

        protected override bool NeedsPerItemOverlay => false; // all harvestable machines have their own overlay already

        private bool IsPlayerLuremaster { get; set; } = false;

        public override void OnNewDay()
        {
            this.newDayAction?.Invoke();
            base.OnNewDay();
        }

        public override void BeforeDraw()
        {
            this.UpdateObjectInfoList(Game1.currentLocation);
            this.TaskDone = !this.ObjectInfoList.Any(soi => soi.NeedAction);
        }

        protected override void UpdateObjectInfoList()
        {
            foreach (var loc in ListHelper.GetActiveLocations().Where(this.locationFilter))
            {
                this.UpdateObjectInfoList(loc);
            }

            this.TaskDone = !this.ObjectInfoList.Any(soi => soi.NeedAction);
        }

        private void UpdateObjectInfoList(GameLocation loc)
        {
            this.ObjectInfoList.RemoveAll(soi => soi.Location == loc);
            this.ObjectInfoList.AddRange(
                from pair in loc.Objects.Pairs
                where this.objectFilter(pair.Value)
                select new StardewObjectInfo(pair.Key, loc, true));
        }

        private bool CrabPotTaskFilter(StardewValley.Object obj)
        {
            bool needAction = false;
            if (obj is CrabPot currentCrabPot)
            {
                if (currentCrabPot.readyForHarvest.Value)
                {
                    needAction = true;
                }

                // if player is luremaster, crab pots dont need bait
                if (currentCrabPot.bait.Value == null && !this.IsPlayerLuremaster)
                {
                    needAction = true;
                }
            }

            return needAction;
        }

        private bool GeneralTaskFilter(StardewValley.Object obj)
        {
            // Casks handled by CaskTaskFilter
            return obj.bigCraftable.Value && obj.readyForHarvest.Value && !(obj is Cask);
        }

        private bool CaskTaskFilter(StardewValley.Object obj)
        {
            bool needAction = false;
            if (obj is Cask cask)
            {
                // TODO: configurable value, also change TaskDoneMessage
                needAction = cask.heldObject.Value?.Quality >= StardewValley.Object.bestQuality;
            }

            return needAction;
        }
    }
}