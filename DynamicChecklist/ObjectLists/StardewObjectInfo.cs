namespace DynamicChecklist.ObjectLists
{
    using System;
    using DynamicChecklist.Graph2;
    using Microsoft.Xna.Framework;
    using StardewValley;

    public class StardewObjectInfo
    {
        public static readonly Vector2 HalfTileOffset = new Vector2(Game1.tileSize / 2, Game1.tileSize / 2);
        public static readonly Vector2 CharacterOffset = new Vector2(0, -Game1.tileSize / 2);
        public static readonly Vector2 TallCharacterOffset = new Vector2(0, -Game1.tileSize);

        public StardewObjectInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StardewObjectInfo"/> class using some <see cref="Character"/>'s current position.
        /// </summary>
        /// <param name="c">The character to use the current position of</param>
        /// <param name="needAction">Whether this task needs doing or not</param>
        public StardewObjectInfo(Character c, bool needAction = true)
        {
            this.SetCharacterPosition(c);
            this.NeedAction = needAction;
            this.DrawOffset = CharacterOffset;
        }

        public StardewObjectInfo(Vector2 tileCoord, LocationReference location, bool needAction = true)
        {
            this.SetTilePosition(tileCoord, location);
            this.NeedAction = needAction;
        }

        public StardewObjectInfo(xTile.Dimensions.Location tile, LocationReference location, bool needAction = true)
            : this(new Vector2(tile.X, tile.Y), location, needAction)
        {
        }

        public StardewObjectInfo(WorldPoint worldPoint, bool needAction = true)
        {
            this.SetWorldPoint(worldPoint);
            this.NeedAction = needAction;
        }

        public LocationReference Location { get; set; }

        /// <summary>
        /// Gets or sets the screen-space coordinates of this task. Use <see cref="TileCoordinate"/> for tile-space coordinates.
        /// </summary>
        /// <seealso cref="TileCoordinate"/>
        public Vector2 Coordinate { get; set; }

        public Vector2 DrawOffset { get; set; } = HalfTileOffset;

        public bool NeedAction { get; set; }

        public Vector2 TileCoordinate => this.Coordinate / Game1.tileSize;

        public Vector2 DrawCoordinate => this.Coordinate + this.DrawOffset;

        public WorldPoint WorldPoint => new Graph2.WorldPoint(this.Location, this.Coordinate);

        public float GetDistance(Character c)
        {
            var charPos = c.getStandingPosition();
            return Vector2.Distance(charPos, this.Coordinate);
        }

        public bool IsOnScreen()
        {
            return Game1.viewport.Contains(new xTile.Dimensions.Location((int)this.Coordinate.X, (int)this.Coordinate.Y));
        }

        public float GetDirection(Character c)
        {
            var v = this.Coordinate - c.getStandingPosition() + this.DrawOffset;
            return (float)Math.Atan2(v.Y, v.X);
        }

        public void SetCharacterPosition(Character c)
        {
            this.Location = c.currentLocation;
            this.Coordinate = c.getStandingPosition(); // already in screen-space coordinates
        }

        public void SetTilePosition(Vector2 tileCoord, LocationReference location)
        {
            this.Coordinate = tileCoord * Game1.tileSize;
            this.Location = location;
        }

        public void SetWorldPoint(WorldPoint worldPoint)
        {
            this.Coordinate = (Vector2)worldPoint;
            this.Location = worldPoint.Location;
        }
    }
}