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
        private Action action;
        private System.Action newDayAction;
        private Func<GameLocation, bool> locationFilter = (loc) => true;
        private Func<StardewValley.Object, bool> objectFilter = (obj) => true;
        private string generalMachineFilterName = string.Empty;

        public MachineList(ModConfig config, Action action)
            : base(config)
        {
            this.action = action;
            switch (action)
            {
                case Action.CrabPot:
                    this.newDayAction = () => this.IsPlayerLuremaster = Game1.player.professions.Contains(11);
                    this.locationFilter = (loc) => ListHelper.LocationHasWater(loc);
                    this.objectFilter = this.CrabPotObjectFilter;
                    this.OptionMenuLabel = "Collect From And Bait Crab Pots";
                    this.TaskDoneMessage = "All crab pots have been collected from and baited";
                    this.Name = TaskName.CrabPot;
                    break;
                case Action.EmptyKeg:
                    this.objectFilter = this.GeneralMachineFilter;
                    this.generalMachineFilterName = "Keg";
                    this.OptionMenuLabel = "Empty Kegs";
                    this.TaskDoneMessage = "All kegs have been emptied";
                    this.Name = TaskName.EmptyKeg;
                    break;
                case Action.EmptyTapper:
                    this.objectFilter = this.GeneralMachineFilter;
                    this.generalMachineFilterName = "Tapper";
                    this.OptionMenuLabel = "Empty Tappers";
                    this.TaskDoneMessage = "All tappers have been emptied";
                    this.Name = TaskName.EmptyTapper;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public enum Action
        {
            CrabPot, EmptyKeg, EmptyTapper
        }

        protected override bool HasPerItemOverlay => false; // all harvestable machines have their own overlay already

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

        private bool CrabPotObjectFilter(StardewValley.Object obj)
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

        private bool GeneralMachineFilter(StardewValley.Object obj)
        {
            return obj.bigCraftable.Value &&
                obj.readyForHarvest.Value &&
                obj.Name == this.generalMachineFilterName;
        }
    }
}