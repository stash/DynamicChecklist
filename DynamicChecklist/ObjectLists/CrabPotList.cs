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

    public class CrabPotList : ObjectList
    {
        public CrabPotList(ModConfig config)
            : base(config)
        {
            this.ImageTexture = OverlayTextures.Crab;
            this.OptionMenuLabel = "Collect From And Bait Crab Pots";
            this.TaskDoneMessage = "All crab pots have been collected from and baited";
            this.Name = TaskName.CrabPot;
            this.ObjectInfoList = new List<StardewObjectInfo>();
        }

        public override string OptionMenuLabel { get; protected set; }

        public override string TaskDoneMessage { get; protected set; }

        protected override Texture2D ImageTexture { get; set; }

        private bool IsPlayerLuremaster { get; set; }

        public override void BeforeDraw()
        {
            this.UpdateObjectInfoList(Game1.currentLocation);
            this.TaskDone = !this.ObjectInfoList.Any(soi => soi.NeedAction);
        }

        public override void OnMenuOpen()
        {
        }

        protected override void UpdateObjectInfoList()
        {
            this.IsPlayerLuremaster = Game1.player.professions.Contains(11);
            foreach (GameLocation loc in Game1.locations)
            {
                this.UpdateObjectInfoList(loc);
            }

            this.TaskDone = !this.ObjectInfoList.Any(soi => soi.NeedAction);
        }

        private void UpdateObjectInfoList(GameLocation loc)
        {
            this.ObjectInfoList.RemoveAll(soi => soi.Location == loc);
            foreach (var pair in from pair in loc.Objects.Pairs
                                 where pair.Value is CrabPot
                                 select pair)
            {
                var currentCrabPot = (CrabPot)pair.Value;

                bool needAction = false;
                if (currentCrabPot.readyForHarvest.Value)
                {
                    needAction = true;
                }

                // if player is luremaster, crab pots dont need bait
                if (currentCrabPot.bait.Value == null && !this.IsPlayerLuremaster)
                {
                    needAction = true;
                }

                this.ObjectInfoList.Add(new StardewObjectInfo(pair.Key, loc, needAction));
            }
        }
    }
}
