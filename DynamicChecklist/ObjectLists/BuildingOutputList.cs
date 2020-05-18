namespace DynamicChecklist.ObjectLists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Buildings;
    using StardewValley.Locations;

    public class BuildingOutputList : ObjectList
    {
        private Vector2 tileOffset = Vector2.Zero;
        private bool showOverlay = false;
        private Func<BuildableGameLocation, IEnumerable<Building>> locationScanner;

        public BuildingOutputList(TaskName name)
            : base(name)
        {
            this.ImageTexture = GameTexture.Plus;
            switch (name)
            {
                case TaskName.JunimoHut:
                    this.ImageTexture = GameTexture.JunimoHutBag;
                    this.showOverlay = true;
                    this.OptionMenuLabel = "Collect From Junimo Huts";
                    this.TaskDoneMessage = "All Junimo huts emptied";
                    this.tileOffset = new Vector2(1, 1);
                    this.locationScanner = ScanJunimoHuts;
                    break;
                case TaskName.Mill:
                    this.showOverlay = false;
                    this.OptionMenuLabel = "Collect Mill Output";
                    this.TaskDoneMessage = "All mills emptied";
                    this.tileOffset = new Vector2(3, 1); // point to output chest
                    this.locationScanner = ScanMills;
                    break;
            }
        }

        protected override bool NeedsPerItemOverlay => this.showOverlay;

        protected override void InitializeObjectInfoList()
        {
            this.FindBuildings(Game1.locations.OfType<BuildableGameLocation>());
        }

        protected override void UpdateObjectInfoList(uint ticks)
        {
            this.FindBuildings(ListHelper.GetActiveLocations().OfType<BuildableGameLocation>());
        }

        private static IEnumerable<Building> ScanJunimoHuts(BuildableGameLocation location)
        {
            return location.buildings.OfType<JunimoHut>().Where(b => !b.output.Value.isEmpty());
        }

        private static IEnumerable<Building> ScanMills(BuildableGameLocation location)
        {
            return location.buildings.OfType<Mill>().Where(b => !b.output.Value.isEmpty());
        }

        private void FindBuildings(IEnumerable<BuildableGameLocation> locations)
        {
            foreach (var location in locations)
            {
                var locRef = LocationReference.For(location);
                this.ObjectInfoList.RemoveAll(soi => soi.Location == locRef);
                foreach (var building in this.locationScanner(location))
                {
                    var tileCoord = new Vector2(building.tileX.Value, building.tileY.Value) + this.tileOffset;
                    var soi = new StardewObjectInfo(tileCoord, locRef) { NeedAction = true };
                    this.ObjectInfoList.Add(soi);
                }
            }
        }
    }
}
