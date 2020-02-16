namespace DynamicChecklist.Graph
{
    using Microsoft.Xna.Framework;
    using StardewValley;

    public class WarpVertex : StardewVertex
    {
        public WarpVertex(GameLocation location, Vector2 position, GameLocation targetLocation, Vector2 targetPosition)
            : base(location, position)
        {
            this.TargetLocation = targetLocation;
            this.TargetPosition = targetPosition;
        }

        public GameLocation TargetLocation { get; private set; }

        public Vector2 TargetPosition { get; private set; }

        public void SetTargetPosition(Vector2 targetPosition)
        {
            this.TargetPosition = targetPosition;
        }
    }
}
