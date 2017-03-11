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

        public override bool TaskDone { get; protected set; }

        public override string TaskDoneMessage { get; protected set; }

        protected override Texture2D ImageTexture { get; set; }

        public CrabPotList()
        {
            ImageTexture = OverlayTextures.Heart;
            OptionMenuLabel = "Collect From And Bait Crab Pots";
            TaskDoneMessage = "All crab pots have been collected from and baited";
        }

        public override void BeforeDraw()
        {
            UpdateObjectInfoList(Game1.currentLocation);
            throw new NotImplementedException();
        }

        public override void OnMenuOpen()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateObjectInfoList()
        {
            foreach(GameLocation loc in Game1.locations)
            {

            }
            throw new NotImplementedException();
        }
        private void UpdateObjectInfoList(GameLocation loc)
        {
            var queryIsHere = from soi in ObjectInfoList
                              select soi.Location == loc;
            var queryIsNotHere = from soi in ObjectInfoList
                                 select soi.Location != loc;
            foreach (KeyValuePair<Vector2, StardewValley.Object> o in loc.Objects)
            {
                if (o.Value is CrabPot)
                {
                    CrabPot currentCrabPot = (CrabPot)o.Value;
                    var soi = new StardewObjectInfo();
                    soi.Coordinate = o.Key;
                    soi.Location = loc;
                    if (currentCrabPot.readyForHarvest)
                    {
                        soi.NeedAction = true;
                    }
                    if (currentCrabPot.bait == null && !(Game1.player.professions.Contains(11)))// if player is luremaster, crab pots dont need bait
                    {
                        soi.NeedAction = true;
                    }
                }
            }
        }
    }
}
