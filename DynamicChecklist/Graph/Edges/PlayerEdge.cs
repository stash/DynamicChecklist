using DynamicChecklist.Graph.Vertices;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicChecklist.Graph.Edges
{
    public class PlayerEdge : StardewEdge
    {
        public PlayerEdge(MovableVertex source, StardewVertex target) : base(source, target, "Player", 0)
        {

        }

        public void UpdateCost()
        {
            this.Cost = Vector2.Distance(this.Source.Position, this.Target.Position);
        }
    }
}
