namespace DynamicChecklist.ObjectLists
{
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Buildings;

    public class JunimoHutList : ObjectList
    {
        public JunimoHutList(ModConfig config, TaskName name)
            : base(config, name)
        {
            this.ImageTexture = GameTexture.JunimoHutBag;
            this.OptionMenuLabel = "Collect From Junimo Huts";
            this.TaskDoneMessage = "All Junimo huts emptied";
        }

        protected IEnumerable<JunimoHut> Huts => Game1.getFarm().buildings.OfType<JunimoHut>();

        protected override void InitializeObjectInfoList()
        {
            var farm = Game1.getFarm();
            foreach (var hut in this.Huts)
            {
                var tileCoord = new Vector2(hut.tileX.Value + 1, hut.tileY.Value + 1); // hard-coded door offset
                var soi = new StardewObjectInfo(tileCoord, farm) { NeedAction = false };
                this.ObjectInfoList.Add(soi);
            }
        }

        protected override void UpdateObjectInfoList(uint ticks)
        {
            var i = 0;
            foreach (var hut in this.Huts)
            {
                var soi = this.ObjectInfoList[i++];
                soi.NeedAction = !hut.output.Value.isEmpty();
            }
        }
    }
}
