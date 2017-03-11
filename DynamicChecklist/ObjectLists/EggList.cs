using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicChecklist.ObjectLists
{
    class EggList : ObjectList
    {
        public override string OptionMenuLabel { get; protected set; }

        public override bool TaskDone { get; protected set; }

        public override string TaskDoneMessage { get; protected set; }

        protected override Texture2D ImageTexture { get; set; }

        public EggList()
        {
            ImageTexture = OverlayTextures.Heart;
            OptionMenuLabel = "Collect From And Bait Crab Pots";
            TaskDoneMessage = "All crab pots have been collected from and baited";
        }

        public override void BeforeDraw()
        {
            throw new NotImplementedException();
        }

        public override void OnMenuOpen()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateObjectInfoList()
        {
            throw new NotImplementedException();
        }
    }
}
