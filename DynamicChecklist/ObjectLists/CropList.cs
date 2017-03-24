using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace DynamicChecklist.ObjectLists
{
    class CropList : ObjectList
    {
        public enum Action { Water, Harvest };
        private Action action;

        public override string OptionMenuLabel { get; protected set; }
        public override string TaskDoneMessage { get; protected set; }

        protected override Texture2D ImageTexture { get; set; }

        public CropList(Action action)
        {
            this.action = action;
            switch (action)
            {
                case Action.Water:
                    ImageTexture = OverlayTextures.Heart;
                    OptionMenuLabel = "Water Crops";
                    TaskDoneMessage = "All crops have been watered";
                    break;
                case Action.Harvest:
                    ImageTexture = OverlayTextures.Heart;
                    OptionMenuLabel = "Harvest Crops";
                    TaskDoneMessage = "All crops have been harvested";
                    break;
                default:
                    throw (new NotImplementedException());
            }

            ObjectInfoList = new List<StardewObjectInfo>();

        }

        public override void OnMenuOpen()
        {
            throw new NotImplementedException();
        }

        public override void BeforeDraw()
        {
            throw new NotImplementedException();
        }

        protected override void UpdateObjectInfoList()
        {
            throw new NotImplementedException();
        }
    }
}
