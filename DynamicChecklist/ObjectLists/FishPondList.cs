namespace DynamicChecklist.ObjectLists
{
    using System.Collections.Generic;
    using System.Linq;
    using StardewValley;
    using StardewValley.Buildings;
    using StardewValley.GameData.FishPond;

    public class FishPondList : ObjectList
    {
        private List<Subject> tracked;

        public FishPondList(ModConfig config, TaskName name)
            : base(config, name)
        {
            this.tracked = new List<Subject>();
            this.ImageTexture = GameTexture.GenericFish;
            this.OptionMenuLabel = "Check Fish Ponds";
            this.TaskDoneMessage = "All fish ponds checked!";
        }

        protected override bool NeedsPerItemOverlay => false; // Game already has good indicators

        protected override void InitializeObjectInfoList()
        {
            this.tracked.Clear();
            var farm = Game1.getFarm();
            foreach (var pond in farm.buildings.OfType<FishPond>())
            {
                if (PondNotReady(pond))
                {
                    continue;
                }

                var subject = new Subject { Pond = pond };
                if (this.Check(subject))
                {
                    subject.SOI = new StardewObjectInfo(pond.GetCenterTile(), farm);
                    this.ObjectInfoList.Add(subject.SOI);
                    this.tracked.Add(subject);
                }
            }
        }

        protected override void UpdateObjectInfoList(uint ticks)
        {
            foreach (var subject in this.tracked)
            {
                subject.SOI.NeedAction = this.Check(subject);
            }

            // Assumes fish ponds can't go back to needing an action after completion
            this.tracked.RemoveAll(subject => !subject.SOI.NeedAction);
        }

        private static bool PondNotReady(FishPond pond)
        {
            return pond.isUnderConstruction() || pond.currentOccupants.Value == 0;
        }

        private bool Check(Subject subject)
        {
            var pond = subject.Pond;
            return pond.output.Value != null /* bucket item ready */ ||
                pond.HasUnresolvedNeeds(); /* active quest */
        }

        public struct Subject
        {
            public StardewObjectInfo SOI;
            public FishPond Pond;
        }
    }
}