using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicChecklist.Graph
{
    public class ShortestPath
    {
        private List<Step> Steps { get; set; } = new List<Step>();
        public void AddStep(GameLocation location, Vector2 position)
        {   
            Steps.Add(new Step(location, position));
        }
        public Step GetNextStep(GameLocation playerLocation)
        {
            return Steps.FirstOrDefault();
        }
    }
}

public struct Step
{
    public GameLocation Location { get; }
    public Vector2 Position { get; }
    public Step(GameLocation location, Vector2 position)
    {
        Location = location;
        Position = position;
    }
}