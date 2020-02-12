namespace DynamicChecklist.ObjectLists
{
    using System;
    using Microsoft.Xna.Framework;
    using StardewValley;

    public class StardewObjectInfo
    {
        public StardewObjectInfo()
        {
        }

        public StardewObjectInfo(FarmAnimal animal, GameLocation location, bool needAction = true)
        {
            this.Coordinate = animal.getStandingPosition();
            this.Location = location;
            this.NeedAction = needAction;
        }

        public StardewObjectInfo(Vector2 coordinate, GameLocation location, bool needAction = true)
        {
            this.Coordinate = coordinate * Game1.tileSize + new Vector2(Game1.tileSize / 2, Game1.tileSize / 2);
            this.Location = location;
            this.NeedAction = needAction;
        }

        public GameLocation Location { get; set; }

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
    }
}