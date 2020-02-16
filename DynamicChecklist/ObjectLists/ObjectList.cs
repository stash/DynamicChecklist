namespace DynamicChecklist.ObjectLists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Graph;
    using Graph.Graphs;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;
    using StardewValley;

    public abstract class ObjectList
    {
        private ShortestPath path;
        private ModConfig config;
        private bool overlayActive;
        private bool taskDone;

        public ObjectList(ModConfig config)
        {
            this.config = config;
            this.ObjectInfoList = new List<StardewObjectInfo>();
        }

        public event EventHandler TaskFinished;

        public event EventHandler OverlayActivated;

        public event EventHandler OverlayActiveChanged;

        public static CompleteGraph Graph { get; set; }

        public string OptionMenuLabel { get; protected set; }

        public List<StardewObjectInfo> ObjectInfoList { get; set; }

        public string TaskDoneMessage { get; protected set; }

        public bool ShowInMenu
        {
            get
            {
                return (!this.taskDone || this.config.ShowAllTasks) && this.config.IncludeTask[this.Name];
            }
        }

        public bool TaskLeft
        {
            get
            {
                return !this.taskDone && this.config.IncludeTask[this.Name];
            }
        }

        public bool OverlayActive
        {
            get
            {
                return this.overlayActive;
            }

            set
            {
                if (!this.overlayActive && value)
                {
                    this.OnOverlayActivated(new EventArgs());
                }

                if (this.overlayActive != value)
                {
                    this.OnOverlayActivateChanged(new EventArgs());
                }

                this.overlayActive = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this list of tasks is complete.
        /// Triggers <c>OnTaskFinished</c> when changed from false to true.
        /// </summary>
        /// <see cref="OnTaskFinished(EventArgs)"/>
        public bool TaskDone
        {
            get
            {
                return this.taskDone;
            }

            protected set
            {
                var oldTaskDone = this.taskDone;
                this.taskDone = value;
                if (this.taskDone && !oldTaskDone && Game1.timeOfDay > 600)
                {
                    this.OnTaskFinished(new EventArgs());
                }
            }
        }

        internal static IMonitor Monitor { get; set; }

        internal static IModHelper Helper { get; set; }

        protected bool TaskExistedAtStartOfDay { get; private set; }

        protected TaskName Name { get; set; }

        protected Texture2D ImageTexture { get; set; } = null;

        protected bool AnyTasksNeedAction => this.ObjectInfoList.Any(soi => soi.NeedAction);

        protected bool NoTasksNeedAction => this.ObjectInfoList.All(soi => !soi.NeedAction);

        protected virtual bool NeedsPerItemOverlay => true;

        public void OnNewDay()
        {
            this.taskDone = false; // skip accessor
            this.ObjectInfoList.Clear();
            this.InitializeObjectInfoList();
            this.TaskDone = this.NoTasksNeedAction;
            this.TaskExistedAtStartOfDay = !this.TaskDone;
        }

        /// <summary>
        /// Game tick event, by default just calls <c>UpdateObjectInfoList()</c> and updates <c>TaskDone</c>
        /// </summary>
        /// <param name="ticks">Game time in ticks</param>
        public void OnUpdateTicked(uint ticks)
        {
            this.UpdateObjectInfoList(ticks);
            this.TaskDone = this.NoTasksNeedAction;
        }

        /// <summary>
        /// Invoked by the Mod to draw overlays for this list.
        /// Either <c>InitializeObjectInfoList</c> or <c>UpdateObjectInfoList</c> is guaranteed to have been called before this.
        /// </summary>
        /// <param name="b">Batch in which to draw the overlay</param>
        public void Draw(SpriteBatch b)
        {
            if (!this.OverlayActive || this.TaskDone)
            {
                return;
            }

            if (!this.config.ShowOverlay && !this.config.ShowArrow)
            {
                return; // nothing to draw or calculate
            }

            var currentLocation = Game1.currentLocation;
            var smallestDistanceFromPlayer = float.PositiveInfinity;
            StardewObjectInfo closestSOI = null;
            bool anyOnScreen = false;
            foreach (var soi in from soi in this.ObjectInfoList
                                where soi.NeedAction && soi.Location == currentLocation
                                select soi)
            {
                anyOnScreen |= soi.IsOnScreen();
                if (this.NeedsPerItemOverlay && this.config.ShowOverlay)
                {
                    this.DrawObjectInfo(b, soi);
                }

                var distanceFromPlayer = soi.GetDistance(Game1.player);
                if (distanceFromPlayer < smallestDistanceFromPlayer)
                {
                    smallestDistanceFromPlayer = distanceFromPlayer;
                    closestSOI = soi;
                }
            }

            if (this.config.ShowArrow && !anyOnScreen)
            {
                if (smallestDistanceFromPlayer == float.PositiveInfinity && this.path != null)
                {
                    // closestSOI was never assigned to if the distance is PositiveInfinity
                    Step warp = this.path.GetNextStep(currentLocation);
                    closestSOI = new StardewObjectInfo(warp.Position, currentLocation);
                }

                if (closestSOI != null)
                {
                    DrawArrow(b, closestSOI.GetDirection(Game1.player));
                }
            }
        }

        public void UpdatePath()
        {
            var targetLocation = this.ObjectInfoList.FirstOrDefault(x => x.NeedAction)?.Location;
            if (targetLocation != null)
            {
                this.path = Graph.GetPathToTarget(Game1.currentLocation, targetLocation);
            }
            else
            {
                this.path = null;
            }
        }

        public void ClearPath()
        {
            this.path = null;
        }

        protected void OnTaskFinished(EventArgs e)
        {
            this.TaskFinished?.Invoke(this, e);
        }

        protected void OnOverlayActivated(EventArgs e)
        {
            this.OverlayActivated?.Invoke(this, e);
        }

        protected void OnOverlayActivateChanged(EventArgs e)
        {
            this.OverlayActiveChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Called on the first tick of the day.
        /// An empty <c>this.ObjectInfoList</c> will have been created.
        /// Subclasses are responsible for filling this list with individual tasks.
        /// </summary>
        /// <seealso cref="UpdateObjectInfoList"/>
        protected abstract void InitializeObjectInfoList();

        /// <summary>
        /// Called on every game update tick except the first.
        /// Subclasses should update <c>this.ObjectInfoList</c> with the current set of tasks (add, remove, and change are all OK!)
        /// </summary>
        /// <param name="ticks">Number of elapsed game ticks (approx 60Hz)</param>
        /// <seealso cref="InitializeObjectInfoList"/>
        protected abstract void UpdateObjectInfoList(uint ticks);

        private static void DrawArrow(SpriteBatch b, float rotation)
        {
            const float distanceFromCenter = 2 * Game1.tileSize;
            var viewport = Game1.viewport;
            var tex = OverlayTextures.ArrowRight;
            var standPos = Game1.player.getStandingPosition();
            var center = new Point((int)standPos.X - viewport.X, (int)standPos.Y - viewport.Y);

            var destinationRectangle = new Rectangle(
                (int)(standPos.X - viewport.X + Math.Cos(rotation) * distanceFromCenter),
                (int)(standPos.Y - viewport.Y + Math.Sin(rotation) * distanceFromCenter),
                tex.Width,
                tex.Height);
            destinationRectangle.X += (int)(Math.Cos(rotation) * distanceFromCenter);
            destinationRectangle.Y += (int)(Math.Sin(rotation) * distanceFromCenter);
            b.Draw(tex, destinationRectangle, null, Color.White, rotation, new Vector2(tex.Width / 2, tex.Height / 2), SpriteEffects.None, 0);
        }

        private void DrawObjectInfo(SpriteBatch b, StardewObjectInfo objectInfo)
        {
            var viewport = Game1.viewport;
            var drawLoc = new Vector2(objectInfo.Coordinate.X - viewport.X, objectInfo.Coordinate.Y - viewport.Y - Game1.tileSize / 2);
            var spriteBox = new Rectangle((int)drawLoc.X - this.ImageTexture.Width / 4 * Game1.pixelZoom, (int)drawLoc.Y - this.ImageTexture.Height / 4 * Game1.pixelZoom - Game1.tileSize / 2, this.ImageTexture.Width * Game1.pixelZoom / 2, this.ImageTexture.Height * Game1.pixelZoom / 2);
            var spriteBoxSpeechBubble = new Rectangle((int)drawLoc.X - OverlayTextures.SpeechBubble.Width / 4 * Game1.pixelZoom, (int)drawLoc.Y - OverlayTextures.SpeechBubble.Height / 4 * Game1.pixelZoom - Game1.tileSize / 2, OverlayTextures.SpeechBubble.Width * Game1.pixelZoom / 2, OverlayTextures.SpeechBubble.Height * Game1.pixelZoom / 2);
            spriteBoxSpeechBubble.Offset(0, Game1.pixelZoom / 2);
            b.Draw(OverlayTextures.SpeechBubble, spriteBoxSpeechBubble, Color.White);
            b.Draw(this.ImageTexture, spriteBox, Color.White);
        }
    }
}
