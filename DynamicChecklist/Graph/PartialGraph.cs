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
    public class PartialGraph : AdjacencyGraph<ExtendedWarp, LabelledEdge<ExtendedWarp>>
    {
        public ExtendedWarp PlayerVertex { get; private set; }
        public ExtendedWarp TargetVertex { get; private set; }

        public List<LabelledEdge<ExtendedWarp>> PlayerEdges { get; private set; }
        public List<LabelledEdge<ExtendedWarp>> TargetEdges { get; private set; } // only connected from outside the partial graph

        public PartialGraph() : base(false)
        {

        }
        public void AddPlayerVertex(ExtendedWarp w)
        {
            if (PlayerVertex == null)
            {
                this.AddVertex(w);
                PlayerVertex = w;
            }
            else
            {
                throw new InvalidOperationException("Player vertex already added");
            }
        }
        public void ConnectPlayerVertex()
        {
            foreach (ExtendedWarp warp in this.Vertices)
            {
                this.AddEdge(new LabelledEdge<ExtendedWarp>(PlayerVertex, warp, "Player Edge", new GraphvizColor(255, 255, 255, 255), 0));
            }
        }
        public void UpdatePlayerEdgeDistances(Farmer who)
        {
            foreach (LabelledEdge<ExtendedWarp> edge in PlayerEdges)
            {
                edge.Cost = Vector2.Distance(who.getTileLocation(), new Vector2(edge.Target.X, edge.Target.Y));
            }
        }

        public void AddTargetVertex(ExtendedWarp w)
        {
            if (PlayerVertex == null)
            {
                this.AddVertex(w);
                TargetVertex = w;
            }
            else
            {
                throw new InvalidOperationException("Player vertex already added");
            }
        }
        public void SetPlayerVertexPosition()
        {
            throw new NotImplementedException();
        }
    }
}
