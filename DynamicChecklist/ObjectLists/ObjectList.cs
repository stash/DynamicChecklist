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
using DynamicChecklist.Graph;
using DynamicChecklist.Graph.Graphs;

namespace DynamicChecklist.ObjectLists
{
    public abstract class ObjectList
    {
        public static CompleteGraph Graph;
        private ShortestPath path;

        private ModConfig config;

        protected int Count { get; set; }
        protected abstract Texture2D ImageTexture { get; set; }
        public abstract string OptionMenuLabel { get; protected set; }
        public List<StardewObjectInfo> ObjectInfoList { get; set; }
        public abstract string TaskDoneMessage { get; protected set; }
        protected bool TaskExistedAtStartOfDay { get; private set; }
        public bool TaskExistsNow { get; private set; }
        public bool ShowInMenu {
            get
            {
                return TaskExistsNow || config.ShowAllTasks;
            }
        }
        private bool taskDone;
        private bool overlayActive;      
        public bool OverlayActive
        {
            get
            {
                return overlayActive;
            }
            set
            {
                if(!overlayActive && value)
                {
                    OnOverlayActivated(new EventArgs());
                }
                if(overlayActive != value)
                {
                    OnOverlayActivateChanged(new EventArgs());
                }
                overlayActive = value;
            }
        }

        public bool TaskDone
        {
            get
            {
                return taskDone;
            }
            protected set
            {
                var oldTaskDone = taskDone;
                taskDone = value;
                if (taskDone && !oldTaskDone && Game1.timeOfDay>600)
                {
                    OnTaskFinished(new EventArgs());
                }
            }
        }

        protected int CountNeedAction
        {
            get
            {
                var count = 0;
                foreach (StardewObjectInfo soi in ObjectInfoList)
                {
                    if (soi.NeedAction)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        public ObjectList(ModConfig config)
        {
            this.config = config;
        }

        protected void OnTaskFinished(EventArgs e)
        {
            TaskFinished?.Invoke(this, e);
        }
        protected void OnOverlayActivated(EventArgs e)
        {
            OverlayActivated?.Invoke(this, e);
        }
        protected void OnOverlayActivateChanged(EventArgs e)
        {
            OverlayActiveChanged?.Invoke(this, e);
        }
        public void UpdatePath()
        {
            var targetLocation = ObjectInfoList.FirstOrDefault(x => x.NeedAction)?.Location;
            if (targetLocation != null)
            {
                path = Graph.GetPathToTarget(Game1.currentLocation, targetLocation);
            }
            else
            {
                path = null;
            }
        }
        public void ClearPath()
        {
            path = null;
        }

        public event EventHandler TaskFinished;
        public event EventHandler OverlayActivated;
        public event EventHandler OverlayActiveChanged;

        public abstract void OnMenuOpen();
        //public abstract void OnNewDay();
        public abstract void BeforeDraw();
        protected abstract void UpdateObjectInfoList();

        private Texture2D arrow;

        public void OnNewDay()
        {
            UpdateObjectInfoList();
            TaskExistedAtStartOfDay = ObjectInfoList.Count > 0;
            TaskExistedAtStartOfDay = CountNeedAction > 0;
            TaskExistsNow = TaskExistedAtStartOfDay;
        }
       
        public void Draw(SpriteBatch b)
        {
            if(!TaskExistedAtStartOfDay && !TaskExistsNow && CountNeedAction>0)
            {
                TaskExistsNow = true;
            }
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
                            var drawLoc = new Vector2(objectInfo.Coordinate.X - viewport.X, objectInfo.Coordinate.Y - viewport.Y - Game1.tileSize / 2);
                            var spriteBox = new Rectangle((int)drawLoc.X - ImageTexture.Width/4 * Game1.pixelZoom, (int)drawLoc.Y - ImageTexture.Height/4*Game1.pixelZoom, ImageTexture.Width * Game1.pixelZoom/2, ImageTexture.Height * Game1.pixelZoom/2);
                            var spriteBoxSpeechBubble = new Rectangle((int)drawLoc.X - OverlayTextures.SpeechBubble.Width / 4 * Game1.pixelZoom, (int)drawLoc.Y - OverlayTextures.SpeechBubble.Height / 4 * Game1.pixelZoom, OverlayTextures.SpeechBubble.Width * Game1.pixelZoom / 2, OverlayTextures.SpeechBubble.Height * Game1.pixelZoom / 2);
                            spriteBoxSpeechBubble.Offset(0, Game1.pixelZoom / 2);
                            Game1.spriteBatch.Draw(OverlayTextures.SpeechBubble, spriteBoxSpeechBubble, Color.White);
                            Game1.spriteBatch.Draw(ImageTexture, spriteBox, Color.White);
                            
                            var distanceFromPlayer = objectInfo.GetDistance(Game1.player);
                            if(distanceFromPlayer < smallestDistanceFromPlayer)
                            {
                                smallestDistanceFromPlayer = distanceFromPlayer;
                                closestSOI = objectInfo;
                            }
                        }
                    }                
                }
                if (smallestDistanceFromPlayer == float.PositiveInfinity)
                {
                    if(path != null)
                    {
                        Step nextStep = path.GetNextStep(Game1.currentLocation);
                        var warpSOI = new StardewObjectInfo();
                        warpSOI.Coordinate = nextStep.Position * Game1.tileSize;
                        DrawArrow(warpSOI.GetDirection(Game1.player), 3 * Game1.tileSize);
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
