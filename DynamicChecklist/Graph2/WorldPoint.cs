namespace DynamicChecklist.Graph2
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using StardewValley;

    /// <summary>
    /// A point in tile-space X,Y coordinates belonging to some location.
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    public class WorldPoint : IEquatable<WorldPoint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WorldPoint"/> class using the specified tile-space coordinates.
        /// </summary>
        /// <param name="location">Game location</param>
        /// <param name="x">X coordinate in tile-space</param>
        /// <param name="y">Y coordinate in tile-space</param>
        public WorldPoint(LocationReference location, int x, int y)
        {
            var size = WorldGraph.LocationSize(location);
#if DEBUG
            // Allow for "just outside" coordinates that SDV tends to use for push-against-edge-of-map Warps
            if (x < -1 || x > ushort.MaxValue || x > size.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(x));
            }
            else if (y < -1 || y > ushort.MaxValue || y > size.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(y));
            }
#endif

            this.Location = location;
            this.X = MathX.Clamp(x, 0, size.Width - 1);
            this.Y = MathX.Clamp(y, 0, size.Height - 1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorldPoint"/> class using screen-space coordinates.
        /// </summary>
        /// <param name="location">Game location</param>
        /// <param name="screenCoord">Coordinates in screen-space (will be divided by game tile size)</param>
        public WorldPoint(LocationReference location, Microsoft.Xna.Framework.Vector2 screenCoord)
            : this(location, (int)screenCoord.X / Game1.tileSize, (int)screenCoord.Y / Game1.tileSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorldPoint"/> class using screen-space coordinates.
        /// </summary>
        /// <param name="location">Game location</param>
        /// <param name="screenPoint">Point in screen-space (will be divided by game tile size)</param>
        public WorldPoint(LocationReference location, Microsoft.Xna.Framework.Point screenPoint)
            : this(location, screenPoint.X / Game1.tileSize, screenPoint.Y / Game1.tileSize)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorldPoint"/> class using tile-space coordinates.
        /// </summary>
        /// <param name="location">Game location</param>
        /// <param name="tilePoint">Point in tile-space</param>
        public WorldPoint(LocationReference location, xTile.Dimensions.Location tilePoint)
            : this(location, tilePoint.X, tilePoint.Y)
        {
        }

        /// <summary>Gets the X coordinate in tile-space.</summary>
        public int X { get; private set; }

        /// <summary>Gets the Y coordinate in tile-space.</summary>
        public int Y { get; private set; }

        /// <summary>Gets the location that this point belongs to.</summary>
        public LocationReference Location { get; private set; }

        private string PortX => this.X >= 0 ? this.X.ToString() : ("n" + (-this.X));

        private string PortY => this.Y >= 0 ? this.Y.ToString() : ("n" + (-this.Y));

        /// <summary>
        /// Converts to a screen-space Vector2.
        /// </summary>
        /// <param name="wp">The point to convert</param>
        public static implicit operator Microsoft.Xna.Framework.Vector2(WorldPoint wp) => new Microsoft.Xna.Framework.Vector2(wp.X * Game1.tileSize, wp.Y * Game1.tileSize);

        /// <summary>
        /// Converts to a screen-space Point.
        /// </summary>
        /// <param name="wp">The point to convert</param>
        public static implicit operator Microsoft.Xna.Framework.Point(WorldPoint wp) => new Microsoft.Xna.Framework.Point(wp.X * Game1.tileSize, wp.Y * Game1.tileSize);

        /// <summary>
        /// Converts to a tile-space Location.
        /// </summary>
        /// <param name="wp">The point to convert</param>
        public static implicit operator xTile.Dimensions.Location(WorldPoint wp) => new xTile.Dimensions.Location(wp.X, wp.Y);

        public static bool operator ==(WorldPoint left, WorldPoint right) => object.ReferenceEquals(left, right) || ((left is object) && left.Equals(right));

        public static bool operator !=(WorldPoint left, WorldPoint right) => !(left == right);

        /// <summary>
        /// Adds a point in tile-space, producing a new <see cref="WorldPoint"/> in the same location.
        /// </summary>
        /// <param name="wp">The point to add to</param>
        /// <param name="tilePoint">Coordinates in tile-space that will be added to the point</param>
        /// <returns>A <see cref="WorldPoint"/> in the same <see cref="LocationReference"/> at new coordinates</returns>
        public static WorldPoint operator +(WorldPoint wp, xTile.Dimensions.Location tilePoint)
        {
            return new WorldPoint(wp.Location, wp.X + tilePoint.X, wp.Y + tilePoint.Y);
        }

        /// <summary>
        /// Adds a vector in screen-space, producing a new <see cref="WorldPoint"/> in the same location.
        /// </summary>
        /// <param name="wp">The point to add to</param>
        /// <param name="screenPoint">Coordinates in screen space that will be added to the point</param>
        /// <returns>A <see cref="WorldPoint"/> in the same <see cref="LocationReference"/> at new coordinates</returns>
        public static WorldPoint operator +(WorldPoint wp, Microsoft.Xna.Framework.Vector2 screenPoint)
        {
            return new WorldPoint(wp.Location, wp.X + (int)screenPoint.X / Game1.tileSize, wp.Y + (int)screenPoint.Y / Game1.tileSize);
        }

        /// <summary>
        /// Subtracts a point in tile-space, producing a new <see cref="WorldPoint"/> in the same location.
        /// </summary>
        /// <param name="wp">The point to add to</param>
        /// <param name="tilePoint">Coordinates in tile-space that will be subtracted from the point</param>
        /// <returns>A <see cref="WorldPoint"/> in the same <see cref="LocationReference"/> at new coordinates</returns>
        public static WorldPoint operator -(WorldPoint wp, xTile.Dimensions.Location tilePoint)
        {
            return new WorldPoint(wp.Location, wp.X - tilePoint.X, wp.Y - tilePoint.Y);
        }

        /// <summary>
        /// Subtracts a vector in screen-space, producing a new <see cref="WorldPoint"/> in the same location.
        /// </summary>
        /// <param name="wp">The point to subtract from</param>
        /// <param name="location">Coordinates in screen space that will be subtracted from the point</param>
        /// <returns>A <see cref="WorldPoint"/> in the same <see cref="LocationReference"/> at new coordinates</returns>
        public static WorldPoint operator -(WorldPoint wp, Microsoft.Xna.Framework.Vector2 location)
        {
            return new WorldPoint(wp.Location, wp.X - (int)location.X / Game1.tileSize, wp.Y - (int)location.Y / Game1.tileSize);
        }

        public static bool InRange(GameLocation location, int x, int y)
        {
            var size = WorldGraph.LocationSize(location);

            // Allow for "just outside" coordinates that SDV tends to use for push-against-edge-of-map Warps
            return x >= -1 && x <= ushort.MaxValue && x <= size.Width
                && y >= -1 && y <= ushort.MaxValue && y <= size.Height;
        }

        /// <summary>
        /// Adds coordinates in tile space, producing a new <see cref="WorldPoint"/> in the same location.
        /// </summary>
        /// <param name="x">X coordinate in tile-space</param>
        /// <param name="y">Y coordinate in tile-space</param>
        /// <returns>A <see cref="WorldPoint"/> in the same <see cref="LocationReference"/> at new coordinates</returns>
        public WorldPoint Add(int x, int y) => new WorldPoint(this.Location, this.X + x, this.Y + y);

        /// <summary>
        /// Subtracts coordinates in tile space, producing a new <see cref="WorldPoint"/> in the same location.
        /// </summary>
        /// <param name="x">X coordinate in tile-space</param>
        /// <param name="y">Y coordinate in tile-space</param>
        /// <returns>A <see cref="WorldPoint"/> in the same <see cref="LocationReference"/> at new coordinates</returns>
        public WorldPoint Subtract(int x, int y) => new WorldPoint(this.Location, this.X - x, this.Y - y);

        public override bool Equals(object obj) => this.Equals(obj as WorldPoint);

        public bool Equals(WorldPoint other)
        {
            return (other is object) &&
                   this.Location == other.Location &&
                   this.X == other.X &&
                   this.Y == other.Y;
        }

        public override int GetHashCode()
        {
            int hashCode = -2142719751;
            hashCode = hashCode * -1521134295 + this.Location.GetHashCode();
            hashCode = hashCode * -1521134295 + this.X.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Y.GetHashCode();
            return hashCode;
        }

        public override string ToString() => $"{this.X},{this.Y}@{this.Location.Name}";

        public string ToShortString() => $"{this.X},{this.Y}@{this.Location.Resolve.Name}";

        public string ToCoordString() => $"{this.X},{this.Y}";

        public string ToPortName() => $"Wx{this.PortX}y{this.PortY}";
    }
}
