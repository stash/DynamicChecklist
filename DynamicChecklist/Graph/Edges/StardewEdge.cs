namespace DynamicChecklist.Graph
{
    using Microsoft.Xna.Framework;
    using QuickGraph;

    public class StardewEdge : Edge<StardewVertex>
    {
        public StardewEdge(StardewVertex source, StardewVertex target, string label)
             : base(source, target)
        {
            this.Label = label;
        }

        public string Label { get; private set; }

        public double Cost
        {
            get
            {
                if (this.Source.Location == this.Target.Location)
                {
                    return Vector2.Distance(this.Source.Position, this.Target.Position);
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
