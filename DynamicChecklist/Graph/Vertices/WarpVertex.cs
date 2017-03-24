using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StardewValley;

namespace DynamicChecklist.Graph
{
    public class WarpVertex : StardewVertex
    {
        public GameLocation TargetLocation { get; private set; }
        public Vector2 TargetPosition { get; private set; }

        public WarpVertex(GameLocation location, Vector2 position, GameLocation targetLocation, Vector2 targetPosition)
            : base(location, position)
        {
            this.TargetLocation = targetLocation;
            this.TargetPosition = targetPosition;
        }

        public void SetTargetPosition(Vector2 targetPosition)
        {
            this.TargetPosition = targetPosition;
        }
    }
}
