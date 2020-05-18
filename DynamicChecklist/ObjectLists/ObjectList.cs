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

    public abstract class ObjectList : IDisposable
    {
        private static readonly Color BubbleTint = Color.White * 0.75f;
        private bool enabled;
        private bool taskDone;
        private bool anyOnScreen;

        public ObjectList(TaskName name)
        {
            this.TaskName = name;
            this.ObjectInfoList = new List<StardewObjectInfo>();
            this.enabled = MainClass.Instance.Config.IncludeTask[this.TaskName];
        }

        public TaskName TaskName { get; private set; }

        public string OptionMenuLabel { get; protected set; }

        public List<StardewObjectInfo> ObjectInfoList { get; set; }

        public string TaskDoneMessage { get; protected set; }

        public bool ShowInMenu
        {
            get
            {
                return MainClass.Instance.Config.ShowAllTasks || (this.Enabled && !this.TaskDone);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this Task is enabled.
        /// </summary>
        /// <remarks>
        /// Passes through the set value to the <see cref="ModConfig"/>, which is written whenever the game is saved.
        /// </remarks>
        public bool Enabled
        {
            get
            {
                return this.enabled;
            }

            set
            {
                if (!this.enabled && value)
                {
                    MainClass.Instance.OnOverlayActivated(this);
                }

                MainClass.Instance.Config.IncludeTask[this.TaskName] = value;
                this.enabled = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this list of tasks is complete.
        /// Triggers <see cref="TaskFinished"/> when changed from false to true.
        /// </summary>
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
                    Game1.showGlobalMessage(this.TaskDoneMessage);
                }
            }
        }

        protected bool TaskExistedAtStartOfDay { get; private set; }

        protected GameTexture ImageTexture { get; set; } = GameTexture.Empty;

        protected Color ImageTint { get; set; } = Color.White;

        protected bool AnyTasksNeedAction => this.ObjectInfoList.Any(soi => soi.NeedAction);

        protected bool NoTasksNeedAction => this.ObjectInfoList.All(soi => !soi.NeedAction);

        protected virtual bool NeedsPerItemOverlay => true;

        protected StardewObjectInfo ClosestSOI { get; set; }

        protected Vector2 ClosestHop { get; set; }

        public virtual void Dispose()
        {
            this.Cleanup();
        }

        public void OnNewDay()
        {
            this.taskDone = false; // skip accessor
            this.ObjectInfoList.Clear();
            try
            {
                this.InitializeObjectInfoList();
                this.TaskDone = this.NoTasksNeedAction;
                this.TaskExistedAtStartOfDay = !this.TaskDone;
            }
            catch (Exception e)
            {
                MainClass.Log($"Exception in {this.TaskName} Init: {e.Message}\n{e.StackTrace}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Game tick event, by default just calls <c>UpdateObjectInfoList()</c> and updates <c>TaskDone</c>
        /// </summary>
        /// <param name="ticks">Game time in ticks</param>
        public void OnUpdateTicked(uint ticks)
        {
            try
            {
                this.UpdateObjectInfoList(ticks);
                this.TaskDone = this.NoTasksNeedAction;
            }
            catch (Exception e)
            {
                MainClass.Log($"Exception in {this.TaskName} Update ({ticks} ticks): {e.Message}\n{e.StackTrace}", LogLevel.Error);
            }
        }

        /// <summary>
        /// Invoked by the Mod to draw overlays for this list.
        /// Either <c>InitializeObjectInfoList</c> or <c>UpdateObjectInfoList</c> is guaranteed to have been called before this.
        /// </summary>
        /// <param name="b">Batch in which to draw the overlay</param>
        public void Draw(SpriteBatch b)
        {
            this.anyOnScreen = false;
            if (!this.Enabled || this.TaskDone)
            {
                return;
            }

            if (!MainClass.Instance.Config.ShowOverlay && !MainClass.Instance.Config.ShowArrow)
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

                if (soi.IsOnScreen() && this.NeedsPerItemOverlay && MainClass.Instance.Config.ShowOverlay)
                {
                    this.DrawObjectInfo(b, soi);
                    continue;
                }

                if (MainClass.Instance.Config.ShowArrow)
                {
                    var distance = soi.GetDistance(Game1.player);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestLocal = soi;
                    }
                }
            }

            if (MainClass.Instance.Config.ShowArrow && !this.anyOnScreen)
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
            var oldClosestSOI = this.ClosestSOI;
            var oldClosestHop = this.ClosestHop;
            this.ClearPath();

            if (!this.Enabled || !MainClass.Instance.Config.ShowArrow || this.anyOnScreen || !this.AnyTasksNeedAction)
            {
                return; // don't pathfind if we don't need to
            }

            var currentLocation = Game1.player.currentLocation;
            if (WorldGraph.IsProceduralLocation(currentLocation))
            {
                return; // can't pathfind out of the mines yet
            }

            var externalSOIs = this.ObjectInfoList.Where(soi => soi.NeedAction && soi.Location != currentLocation);
            bool found = false;
            try
            {
                if (MainClass.Instance.WorldGraph.PlayerHasOnlyOneWayOut(out var onlyHop))
                {
                    var firstSoi = externalSOIs.FirstOrDefault();
                    if (firstSoi != default)
                    {
                        this.ClosestSOI = firstSoi;
                        this.ClosestHop = onlyHop;
                        found = true;
                    } // else, no external SOIs need action
                }
                else
                {
                    var start = WorldPoint.ForPlayer(); // can throw if out of range
                    found = this.FindClosestExternalSOI(externalSOIs, start);
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                // Ignore; sometimes the player position is out of range
            }
            catch (KeyNotFoundException e)
            {
                // Ignore; sometimes the game makes fake locations
                MainClass.Log($"While attempting to update path for {this.TaskName}: {e.Message}\n{e.StackTrace}");
            }

            if (!found && oldClosestSOI != null && oldClosestSOI.NeedAction)
            {
                this.ClosestSOI = oldClosestSOI;
                this.ClosestHop = oldClosestHop;
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
            this.taskDone = true; // avoid triggering event
            foreach (var soi in this.ObjectInfoList)
            {
                soi.NeedAction = false;
            }
        }

        public virtual void Cleanup()
        {
            this.ClearPath();
            this.taskDone = true; // avoid triggering event
            this.ObjectInfoList.Clear();
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
                sprite.Width * 3 / 4,
                sprite.Height * 3 / 4);
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
            var zoom = Game1.pixelZoom / 2;
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

        private bool FindClosestExternalSOI(IEnumerable<StardewObjectInfo> externalSOIs, WorldPoint player)
        {
            bool found = false;
            float limit = float.PositiveInfinity;
            foreach (var soi in externalSOIs)
            {
                try
                {
                    if (MainClass.Instance.WorldGraph.TryFindNextHop(player, soi.WorldPoint, out var distance, out var nextHop, limit))
                    {
                        if (distance < limit)
                        {
                            limit = distance;
                            this.ClosestSOI = soi;
                            this.ClosestHop = nextHop;
                            found = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    MainClass.Log($"Error finding next hop for player for {soi.ToTileCoordString()}: {e.Message}\n{e.StackTrace}", LogLevel.Error);
                }
            }

            return found;
        }
    }
}
