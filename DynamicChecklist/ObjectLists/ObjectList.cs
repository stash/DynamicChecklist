namespace DynamicChecklist.ObjectLists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Graph2;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewModdingAPI;
    using StardewValley;

    public abstract class ObjectList
    {
        private static readonly Color BubbleTint = Color.White * 0.75f;
        private ModConfig config;
        private bool overlayActive;
        private bool taskDone;
        private bool anyOnScreen;

        public ObjectList(ModConfig config)
        {
            this.config = config;
            this.ObjectInfoList = new List<StardewObjectInfo>();
        }

        public event EventHandler TaskFinished;

        public event EventHandler OverlayActivated;

        public event EventHandler OverlayActiveChanged;

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

        protected GameTexture ImageTexture { get; set; } = GameTexture.Empty;

        protected Color ImageTint { get; set; } = Color.White;

        protected bool AnyTasksNeedAction => this.ObjectInfoList.Any(soi => soi.NeedAction);

        protected bool NoTasksNeedAction => this.ObjectInfoList.All(soi => !soi.NeedAction);

        protected virtual bool NeedsPerItemOverlay => true;

        protected StardewObjectInfo ClosestSOI { get; set; }

        protected Vector2 ClosestHop { get; set; }

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
            this.anyOnScreen = false;
            if (!this.OverlayActive || this.TaskDone)
            {
                return;
            }

            if (!this.config.ShowOverlay && !this.config.ShowArrow)
            {
                return; // nothing to draw or calculate
            }

            var currentLocation = Game1.currentLocation;
            var nearestDistance = float.PositiveInfinity;
            StardewObjectInfo nearestLocal = null;
            foreach (var soi in from soi in this.ObjectInfoList
                                where soi.NeedAction && soi.Location == currentLocation
                                select soi)
            {
                var onScreen = soi.IsOnScreen();
                this.anyOnScreen |= onScreen;

                if (soi.IsOnScreen() && this.NeedsPerItemOverlay && this.config.ShowOverlay)
                {
                    this.DrawObjectInfo(b, soi);
                    continue;
                }

                if (this.config.ShowArrow)
                {
                    var distance = soi.GetDistance(Game1.player);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestLocal = soi;
                    }
                }
            }

            if (this.config.ShowArrow && !this.anyOnScreen)
            {
                if (nearestLocal != null)
                {
                    this.DrawArrow(b, nearestLocal, false);
                }
                else if (this.ClosestSOI != null)
                {
                    var virtualSOI = new StardewObjectInfo() { Location = Game1.currentLocation, Coordinate = this.ClosestHop };
                    this.DrawArrow(b, virtualSOI, true);
                }
            }
        }

        public void UpdatePath()
        {
            this.ClearPath();

            if (!this.OverlayActive || !this.config.ShowArrow || this.anyOnScreen || !this.AnyTasksNeedAction)
            {
                return; // don't pathfind if we don't need to
            }

            var currentLocation = Game1.player.currentLocation;
            if (WorldGraph.IsProceduralLocation(currentLocation))
            {
                return; // can't pathfind out of the mines yet
            }

            var externalSOIs = this.ObjectInfoList.Where(soi => soi.NeedAction && soi.Location != currentLocation);
            if (MainClass.WorldGraph.PlayerHasOnlyOneWayOut(out var onlyHop))
            {
                var firstSoi = externalSOIs.FirstOrDefault();
                if (firstSoi != default)
                {
                    this.ClosestSOI = firstSoi;
                    this.ClosestHop = onlyHop;
                } // else, no external SOIs need action

                return;
            }

            // find closest SOI that's outside of the current location
            float limit = float.PositiveInfinity;
            foreach (var soi in externalSOIs)
            {
                if (MainClass.WorldGraph.TryFindNextHopForPlayer(soi.WorldPoint, out var distance, out var nextHop, limit))
                {
                    if (distance < limit)
                    {
                        limit = distance;
                        this.ClosestSOI = soi;
                        this.ClosestHop = nextHop;
                    }
                }
                else if (limit == float.PositiveInfinity)
                {
                    Monitor.Log($"{this.Name}: Can't find path to {soi.ToTileCoordString()}!", LogLevel.Warn);
                }
            }
        }

        public void ClearPath()
        {
            this.ClosestSOI = null;
            this.ClosestHop = default;
        }

        /// <summary>
        /// Cancels all of the tasks in this list without triggering the <c>TaskFinished</c> event
        /// </summary>
        public virtual void Cancel()
        {
            this.taskDone = true;
            foreach (var soi in this.ObjectInfoList)
            {
                soi.NeedAction = false;
            }
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

        private static void DrawArrowCommon(SpriteBatch b, float rotation, float distance, bool isWarp)
        {
            var sprite = GameTexture.ArrowRight;
            var standPos = Game1.player.getStandingPosition();
            var destRect = new Rectangle(
                (int)(standPos.X - Game1.viewport.X + Math.Cos(rotation) * distance),
                (int)(standPos.Y - Game1.viewport.Y + Math.Sin(rotation) * distance),
                sprite.Width,
                sprite.Height);
            var color = isWarp ? Color.CornflowerBlue : Color.White;
            var origin = new Vector2(sprite.Width / 2, sprite.Height / 2);
            b.Draw(sprite.Tex, destRect, sprite.Src, color, rotation, origin, SpriteEffects.None, 0);
        }

        private static void DrawArrow(SpriteBatch b, float rotation, bool isWarp = false)
        {
            const float maxDistance = 3 * Game1.tileSize;

            DrawArrowCommon(b, rotation, maxDistance, isWarp);
        }

        private void DrawArrow(SpriteBatch b, StardewObjectInfo nearest, bool isWarp = false)
        {
            const float maxDistance = 3 * Game1.tileSize;

            var rotation = nearest.GetDirection(Game1.player);
            var distance = Math.Min(nearest.GetDistance(Game1.player), maxDistance);
            if (distance <= Game1.tileSize)
            {
                return;
            }

            DrawArrowCommon(b, rotation, distance, isWarp);
        }

        /// <summary>
        /// Draws the bubble for the selected object info.
        /// </summary>
        /// <param name="b">Drawing context</param>
        /// <param name="soi">Task item to draw a bubble for</param>
        private void DrawObjectInfo(SpriteBatch b, StardewObjectInfo soi)
        {
            var bubble = GameTexture.TaskBubble;
            var image = this.ImageTexture;
            var zoom = Game1.pixelZoom * 3 / 4;
            Rectangle dstImage = new Rectangle(2 * zoom, 2 * zoom, 16 * zoom, 16 * zoom); // draw area is inset two pixels
            Rectangle dstBubble = new Rectangle(0, 0, bubble.Width * zoom, bubble.Height * zoom);
            dstImage = MathX.CenteredScaledRectangle(dstImage, image.Width, image.Height, zoom);
            var drawCoord = soi.DrawCoordinate;

            int x = (int)drawCoord.X - Game1.viewport.X; // viewport translation
            x -= Game1.tileSize / 2; // middle of tile
            x += (Game1.tileSize - dstBubble.Width) / 2; // shifted over by the width of the bubble
            x += zoom; // the bubble is slightly to the left so that it's pointy, so shift over by one "apparent pixel" so it looks like it points correctly

            int y = (int)drawCoord.Y - Game1.viewport.Y; // viewport translation
            y -= dstBubble.Height; // align bottom of bubble with bottom of tile
            y -= Game1.tileSize / 4; // raise it up 1/4 tile

            dstBubble.Offset(x, y);
            dstImage.Offset(x, y);

            b.Draw(bubble.Tex, dstBubble, bubble.Src, BubbleTint);
            b.Draw(image.Tex, dstImage, image.Src, BubbleTint);
        }
    }
}
