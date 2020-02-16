namespace DynamicChecklist.ObjectLists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using StardewValley;
    using StardewValley.Objects;

    public class MachineList : ObjectList
    {
        private const int TickOffset = 50;
        private readonly Action action;
        private readonly System.Action newDayAction;
        private readonly Func<GameLocation, bool> locationFilter = (loc) => true;
        private readonly Func<StardewValley.Object, bool> objectFilter = (obj) => true;
        private readonly bool machinesCanUpdate = true;

        public MachineList(ModConfig config, Action action)
            : base(config)
        {
            this.action = action;
            this.ImageTexture = OverlayTextures.Empty; // always hidden though
            switch (action)
            {
                case Action.CrabPot:
                    this.machinesCanUpdate = false; // only updates at start of day
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
                    this.OptionMenuLabel = "Empty Casks";
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

        protected override void InitializeObjectInfoList()
        {
            this.newDayAction?.Invoke();
            this.UpdateLocations(Game1.locations);
        }

        protected override void UpdateObjectInfoList(uint ticks)
        {
            // update all locations once per second (refiners that update)
            IEnumerable<GameLocation> locations;
            if (this.machinesCanUpdate)
            {
                // Both farmers and machines can change task list, so update Active ones fast, everything else slow
                // Offset by action ID to distribute a bit more evenly.
                locations = (ticks % 60 == TickOffset + (int)this.action) ? Game1.locations : ListHelper.GetActiveLocations();
            }
            else
            {
                // Only farmers can update
                locations = ListHelper.GetActiveLocations();
            }

            this.UpdateLocations(locations);
        }

        private void UpdateLocations(IEnumerable<GameLocation> locations)
        {
            foreach (var loc in locations.Where(this.locationFilter))
            {
                this.ObjectInfoList.RemoveAll(soi => soi.Location == loc);
                this.ObjectInfoList.AddRange(
                    from pair in loc.Objects.Pairs
                    where this.objectFilter(pair.Value)
                    select new StardewObjectInfo(pair.Key, loc, true));
            }
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
            return obj.bigCraftable.Value &&
                obj.readyForHarvest.Value &&
                !(obj is Cask) && // Handled by CaskTaskFilter
                obj.ParentSheetIndex != EggList.AutoGrabberId; // Handled by EggList
        }

        private bool CaskTaskFilter(StardewValley.Object obj)
        {
            // Implicit in this function: exclude AutoGrabbers
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