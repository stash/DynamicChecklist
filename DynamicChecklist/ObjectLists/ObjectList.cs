using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using System.ComponentModel;

namespace DynamicChecklist.ObjectLists
{
    public abstract class ObjectList
    {
        protected int Count { get; set; }
        protected int CountNeedAction { get; set; }
        public bool OverlayActive { get; set; }
        protected abstract Texture2D ImageTexture { get; set; }
        public abstract string OptionMenuLabel { get; protected set; }
        public List<StardewObjectInfo> ObjectInfoList { get ; set; }
        public abstract bool TaskDone { get; protected set; }
        public abstract string TaskDoneMessage { get; protected set; }
        public bool TaskExistsToday { get; protected set; }

        protected void OnTaskFinished(EventArgs e)
        {
            TaskFinished?.Invoke(this, e);
        }

        public event EventHandler TaskFinished;

        public abstract void OnMenuOpen();
        //public abstract void OnNewDay();
        public abstract void BeforeDraw();
        protected abstract void UpdateObjectInfoList();

        private Texture2D arrow;

        public void OnNewDay()
        {
            UpdateObjectInfoList();
            TaskExistsToday = ObjectInfoList.Count > 0;
        }
       
        public void Draw(SpriteBatch b)
        {
            if (OverlayActive)
            {
                var currentPlayerLocation = Game1.currentLocation;
                var viewport = Game1.viewport;
                var smallestDistanceFromPlayer = float.PositiveInfinity;
                StardewObjectInfo closestSOI = null;
                bool anyOnScreen = false;
                foreach (StardewObjectInfo objectInfo in ObjectInfoList)
                {                    
                    if (objectInfo.NeedAction)
                    {
                        if (objectInfo.IsOnScreen())
                        {
                            anyOnScreen = true;
                        }
                        if(objectInfo.Location == currentPlayerLocation)
                        {
                            // TODO only draw if on screen
                            var drawLoc = new Vector2(objectInfo.Coordinate.X - viewport.X, objectInfo.Coordinate.Y - viewport.Y);
                            var spriteBox = new Rectangle((int)drawLoc.X - ImageTexture.Width/2 * Game1.pixelZoom, (int)drawLoc.Y - ImageTexture.Height/2*Game1.pixelZoom, ImageTexture.Width * Game1.pixelZoom, ImageTexture.Height * Game1.pixelZoom);
                            Game1.spriteBatch.Draw(ImageTexture, spriteBox, Color.White);

                            var distanceFromPlayer = objectInfo.GetDistance(Game1.player);
                            if(distanceFromPlayer < smallestDistanceFromPlayer)
                            {
                                closestSOI = objectInfo;
                            }
                        }
                        else
                        {
                            // TODO implement drawing arrows to different location
                        }
                    }
                     
                }
                if (!(closestSOI == null) && !anyOnScreen)
                {
                    DrawArrow(closestSOI.GetDirection(Game1.player), 3 * Game1.tileSize);
                }

            }
        }

        public class StardewObjectInfo
        {
            public GameLocation Location { get; set; }
            public Vector2 Coordinate { get; set; }
            public bool NeedAction { get; set; }

            public float GetDistance(Character c)
            {
                var charPos = c.getStandingPosition();
                return Vector2.Distance(charPos, Coordinate);
            }

            public bool IsOnScreen()
            {
                var v = Game1.viewport;
                bool leftOrRight = Coordinate.X < v.X || Coordinate.X > v.X + v.Width;
                bool belowOrAbove = Coordinate.Y < v.Y || Coordinate.Y > v.Y + v.Height;
                return !leftOrRight && !belowOrAbove;
            }

            public float GetDirection(Character c)
            {
                var v = Coordinate - c.getStandingPosition();
                return (float)Math.Atan2(v.Y, v.X);
            }
        }

        private static void DrawArrow(float rotation, float distanceFromCenter)
        {
            // TODO Implement distance frome center
            var tex = OverlayTextures.ArrowRight;
            Point center = new Point(Game1.viewport.Width / 2, Game1.viewport.Height / 2);

            var destinationRectangle = new Rectangle(center.X-tex.Width/2, center.Y-tex.Height/2, tex.Width, tex.Height);
            destinationRectangle.X += (int)(Math.Cos(rotation)*distanceFromCenter);
            destinationRectangle.Y += (int)(Math.Sin(rotation)*distanceFromCenter);

            destinationRectangle.X += destinationRectangle.Width / 2;
            destinationRectangle.Y += destinationRectangle.Height / 2;
            Game1.spriteBatch.Draw(tex, destinationRectangle, null, Color.White, rotation, new Vector2(tex.Width / 2, tex.Height / 2), SpriteEffects.None, 0);
        }

    }
}
