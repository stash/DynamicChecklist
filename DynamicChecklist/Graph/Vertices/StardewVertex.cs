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

    }
}
