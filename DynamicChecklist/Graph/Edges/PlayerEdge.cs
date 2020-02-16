namespace DynamicChecklist.Graph.Edges
{
    using DynamicChecklist.Graph.Vertices;

    public class PlayerEdge : StardewEdge
    {
        public PlayerEdge(MovableVertex source, StardewVertex target)
            : base(source, target, "Player")
        {
        }
    }
}
