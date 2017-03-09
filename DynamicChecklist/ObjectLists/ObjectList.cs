using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace DynamicChecklist.ObjectLists
{
    public abstract class ObjectList
    {
        protected int Count { get; set; }
        protected int CountNeedAction { get; set; }
        public bool OverlayActive { get; set; }
        protected Texture2D ImageTexture { get; set; }
        public abstract string OptionMenuLabel { get; }
        public List<StardewObjectInfo> ObjectInfoList { get ; set; }
        public bool TaskDone
        {
            get
            {
                foreach(StardewObjectInfo soi in ObjectInfoList)
                {
                    if (soi.NeedAction) return false;
                }
                return true;
            }
        }

        public abstract void beforeMenuOpenUpdate();
        public abstract void updateObjectInfo();

        public void draw(SpriteBatch b)
        {
            if (OverlayActive)
            {
                var currentPlayerLocation = Game1.currentLocation;
                var viewport = Game1.viewport;
                foreach(StardewObjectInfo objectInfo in ObjectInfoList)
                {
                    if (objectInfo.NeedAction)
                    {
                        if(objectInfo.Location == currentPlayerLocation)
                        {
                            var drawLoc = new Vector2(objectInfo.Coordinate.X - viewport.X, objectInfo.Coordinate.Y - viewport.Y);
                            var spriteBox = new Rectangle((int)drawLoc.X - ImageTexture.Width/2, (int)drawLoc.Y - ImageTexture.Height/2, ImageTexture.Width, ImageTexture.Height);
                            Game1.spriteBatch.Draw(ImageTexture, spriteBox, Color.White);
                        }
                        else
                        {
                            // TODO implement drawing arrows to different location
                        }
                    }
                }

            }
        }

        public struct StardewObjectInfo
        {
            public GameLocation Location { get; set; }
            public Vector2 Coordinate { get; set; }
            public bool NeedAction { get; set; }
        }

        private static void drawArrowTo(Viewport v, Vector2 coordinate)
        {

        }

    }
}
