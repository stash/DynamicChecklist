using DynamicChecklist.Graph.Edges;
using DynamicChecklist.Graph.Vertices;
using Microsoft.Xna.Framework;
using QuickGraph;
using QuickGraph.Graphviz.Dot;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DynamicChecklist.Graph
{
    public class PartialGraph : StardewGraph
    {
        public MovableVertex PlayerVertex { get; private set; }
        public MovableVertex TargetVertex { get; private set; }

        public List<StardewEdge> PlayerEdges { get; private set; }
        public List<StardewEdge> TargetEdges { get; private set; } // only connected from outside the partial graph

        public GameLocation Location { get; private set; }

        public PartialGraph(GameLocation location)
        {
            
        }
        public void Populate()
        {
            // TODO get code from MyGraph
        }
        private void AddPlayerVertex(MovableVertex vertex)
        {
            if (PlayerVertex == null)
            {
                AddVertex(vertex);
                PlayerVertex = vertex;
            }
            else
            {
                throw new InvalidOperationException("Player vertex already added");
            }
        }
        private void ConnectPlayerVertex()
        {
            foreach (StardewVertex vertex in this.Vertices)
            {
                if(vertex != PlayerVertex)
                {
                    var newEdge = new PlayerEdge(PlayerVertex, vertex);
                    AddEdge(newEdge);
                    PlayerEdges.Add(newEdge);
                }
            }
        }
        public void UpdatePlayerEdgeCosts(Vector2 position)
        {
            PlayerVertex.SetPosition(position);
            foreach(PlayerEdge edge in PlayerEdges)
            {
                edge.UpdateCost();
            }
        }

        private void AddTargetVertex(MovableVertex w)
        {
            if (PlayerVertex == null)
            {
                AddVertex(w);
                TargetVertex = w;
            }
            else
            {
                throw new InvalidOperationException("Player vertex already added");
            }
        }
    }
}
