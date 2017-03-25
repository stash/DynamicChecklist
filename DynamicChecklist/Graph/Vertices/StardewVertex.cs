using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicChecklist.Graph
{
    public class StardewVertex
    {
        public GameLocation Location { get; private set; }
        public Vector2 Position { get; protected set; }
        public StardewVertex(GameLocation location, Vector2 position)
        {
            this.Location = location;
            this.Position = position;
        }
        public static float Distance(StardewVertex vertex1, StardewVertex vertex2)
        {
            return Math.Abs(vertex1.Position.X - vertex2.Position.X) + Math.Abs(vertex1.Position.Y - vertex2.Position.Y);
        }

    }
}
