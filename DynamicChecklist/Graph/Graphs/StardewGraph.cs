namespace DynamicChecklist.Graph
{
    using QuickGraph;

    public abstract class StardewGraph : AdjacencyGraph<StardewVertex, StardewEdge>
    {
        public StardewGraph()
            : base(false)
        {
        }
    }
}
