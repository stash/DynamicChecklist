using Microsoft.Xna.Framework;
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
        public double Cost
        {
            get
            {
                if(Source.Location == Target.Location)
                {
                    return Vector2.Distance(this.Source.Position, this.Target.Position);
                }
                else
                {
                    return 0;
                }               
            }
        }

        public StardewEdge(StardewVertex source, StardewVertex target, string label) : base(source, target)
        {
            this.Label = label;
        }
    }
}
