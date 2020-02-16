namespace DynamicChecklist.ObjectLists
{
    using System;
    using Microsoft.Xna.Framework;
    using StardewValley;

    public class StardewObjectInfo
    {
        public static readonly Vector2 HalfTile = new Vector2(Game1.tileSize / 2, Game1.tileSize / 2);

        public StardewObjectInfo()
        {
        }

        public StardewObjectInfo(Character c, bool needAction = true)
        {
            this.SetCharacterPosition(c);
            this.NeedAction = needAction;
        }

        public StardewObjectInfo(Character c, GameLocation location, bool needAction = true)
        {
            this.SetCharacterPosition(c);
            this.Location = location; // overrides call above
            this.NeedAction = needAction;
        }

        public StardewObjectInfo(Vector2 tileCoord, GameLocation location, bool needAction = true)
        {
            this.SetTileCoordinate(tileCoord);
            this.Location = location;
            this.NeedAction = needAction;
        }

        public GameLocation Location { get; set; }

        /// <summary>
        /// Gets or sets the screen-space coordinates of this task.
        /// </summary>
        public Vector2 Coordinate { get; set; }

        public bool NeedAction { get; set; }

        public float GetDistance(Character c)
        {
            var charPos = c.getStandingPosition();
            return Vector2.Distance(charPos, this.Coordinate);
        }

        public bool IsOnScreen()
        {
            var v = Game1.viewport;
            bool leftOrRight = this.Coordinate.X < v.X || this.Coordinate.X > v.X + v.Width;
            bool belowOrAbove = this.Coordinate.Y < v.Y || this.Coordinate.Y > v.Y + v.Height;
            return !leftOrRight && !belowOrAbove;
        }

        public float GetDirection(Character c)
        {
            var v = this.Coordinate - c.getStandingPosition();
            return (float)Math.Atan2(v.Y, v.X);
        }

        public void SetTileCoordinate(Vector2 tilePosition)
        {
            this.Coordinate = tilePosition * Game1.tileSize + HalfTile;
        }

        public void SetCharacterPosition(Character c)
        {
            this.Location = c.currentLocation;
            this.Coordinate = c.getStandingPosition();
        }
    }
}