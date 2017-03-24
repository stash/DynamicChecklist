using QuickGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicChecklist.Graph
{
    public abstract class StardewGraph : AdjacencyGraph<StardewVertex, StardewEdge>
    {
        public StardewGraph() : base(false)
        {
        }
    }
}
