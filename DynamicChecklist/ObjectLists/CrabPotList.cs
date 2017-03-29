using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Objects;
using Microsoft.Xna.Framework;

namespace DynamicChecklist.ObjectLists
{
    class CrabPotList : ObjectList
    {
        public override string OptionMenuLabel {get;protected set;}

        public override string TaskDoneMessage { get; protected set; }

        protected override Texture2D ImageTexture { get; set; }

        public CrabPotList(ModConfig config) : base(config)
        {
            ImageTexture = OverlayTextures.Crab;
            OptionMenuLabel = "Collect From And Bait Crab Pots";
            TaskDoneMessage = "All crab pots have been collected from and baited";
            Name = TaskName.CrabPot;
            ObjectInfoList = new List<StardewObjectInfo>();
        }

        public override void BeforeDraw()
        {
            UpdateObjectInfoList(Game1.currentLocation);
            TaskDone = !(ObjectInfoList.Any(soi => soi.NeedAction));
        }

        public override void OnMenuOpen()
        {
        }

        protected override void UpdateObjectInfoList()
        {
            foreach(GameLocation loc in Game1.locations)
            {
                UpdateObjectInfoList(loc);
            }
            TaskDone = !(ObjectInfoList.Any(soi => soi.NeedAction));
        }
        private void UpdateObjectInfoList(GameLocation loc)
        {

            ObjectInfoList.RemoveAll(soi => soi.Location == loc);

            foreach (KeyValuePair<Vector2, StardewValley.Object> o in loc.Objects)
            {
                if (o.Value is CrabPot)
                {
                    CrabPot currentCrabPot = (CrabPot)o.Value;
                    var soi = new StardewObjectInfo();
                    soi.Coordinate = o.Key*Game1.tileSize + new Vector2(Game1.tileSize/2, Game1.tileSize / 2);
                    soi.Location = loc;
                    if (currentCrabPot.readyForHarvest)
                    {
                        soi.NeedAction = true;
                    }
                    if (currentCrabPot.bait == null && !(Game1.player.professions.Contains(11)))// if player is luremaster, crab pots dont need bait
                    {
                        soi.NeedAction = true;
                    }
                    ObjectInfoList.Add(soi);
                }
            }
        }
    }
}
