using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicChecklist.Graph
{
    public class StardewEdge : Edge<StardewVertex>
    {
        public string Label { get; private set; }
        public double Cost { get; protected set; }
        public StardewEdge(StardewVertex source, StardewVertex target, string label, double cost) : base(source, target)
        {
            this.Label = label;
            this.Cost = cost;
        }
    }
}
