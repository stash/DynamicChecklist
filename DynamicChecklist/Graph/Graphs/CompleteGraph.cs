using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicChecklist.Graph.Graphs
{
    public class CompleteGraph : StardewGraph
    {
        public List<PartialGraph> PartialGraphs { get; private set; }
        private List<GameLocation> gameLocations;

        public CompleteGraph(List<GameLocation> gameLocations)
        {
            this.gameLocations = gameLocations;
        }
        private void Populate()
        {
            foreach(GameLocation location in gameLocations)
            {
                var partialGraph = new PartialGraph(location);
                partialGraph.Populate();
                AddPartialGraph(partialGraph);
            }
            // TODO Add algorithm
        }
        private void AddPartialGraph(PartialGraph partialGraph)
        {
            PartialGraphs.Add(partialGraph);
            // TODO Connect partialGraph to whole graph
        }
        public void CalculatePathToTarget()
        {
            // TODO Figure out return type
        }
        public void SetTargetLocation(GameLocation location, Vector2 position)
        {

        }
        public void SetPlayerLocation(GameLocation location, Vector2 position)
        {
            // TODO Update the corresponding partial graph
        }

    }
}
