namespace DynamicChecklist.Graph.Vertices
{
    using Microsoft.Xna.Framework;
    using StardewValley;

    public class MovableVertex : StardewVertex
    {
        public MovableVertex(GameLocation location, Vector2 position)
            : base(location, position)
        {
        }

        public void SetPosition(Vector2 position)
        {
            this.Position = position;
        }
    }
}
