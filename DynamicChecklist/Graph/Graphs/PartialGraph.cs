using DynamicChecklist.Graph.Edges;
using DynamicChecklist.Graph.Vertices;
using Microsoft.Xna.Framework;
using QuickGraph;
using QuickGraph.Graphviz.Dot;
using StardewValley;
using StardewValley.Buildings;
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

        public List<PlayerEdge> PlayerEdges { get; private set; }
        //public List<StardewEdge> TargetEdges { get; private set; } // only connected from outside the partial graph

        public GameLocation Location { get; private set; }

        public PartialGraph(GameLocation location) : base()
        {
            Location = location;
        }
        public void Populate()
        {
            var vertexToInclude = new List<WarpVertex>();
            var warps = Location.warps;

            if(Location is Farm)
            {
                var farmBuildings = ((Farm)Location).buildings;
                foreach (Building building in farmBuildings)
                {
                    if (building.indoors != null && building.indoors.GetType() == typeof(AnimalHouse))
                    {
                        var doorLoc = new Vector2(building.tileX + building.humanDoor.X, building.tileY + building.humanDoor.Y);
                        // Target location does not matter since an animal house is always at the end of the path
                        var vertexNew = new WarpVertex(Location, doorLoc, building.indoors, new Vector2(0,0));
                        AddVertex(vertexNew);
                    }
                }
            }

            for (int i = 0; i < warps.Count; i++)
            {
                var warp = warps.ElementAt(i);
                var vertexNew = new WarpVertex(Location, new Vector2(warp.X, warp.Y), Game1.getLocationFromName(warp.TargetName), new Vector2(warp.TargetX, warp.TargetY));
                bool shouldAdd = true;
                foreach (WarpVertex extWarpIncluded in vertexToInclude)
                {
                    if (vertexNew.TargetLocation == extWarpIncluded.TargetLocation && StardewVertex.Distance(vertexNew, extWarpIncluded) < 5)
                    {
                        shouldAdd = false;
                        break;
                    }
                }
                if (shouldAdd)
                {
                    vertexToInclude.Add(vertexNew);                    
                    AddVertex(vertexToInclude.Last());
                }

            }
            for (int i = 0; i < vertexToInclude.Count; i++)
            {
                var vertex1 = vertexToInclude.ElementAt(i);


                for (int j = 0; j < vertexToInclude.Count; j++)
                {
                    var LocTo = Game1.getLocationFromName(Location.warps.ElementAt(j).TargetName);
                    var vertex2 = vertexToInclude.ElementAt(j);
                    var path = PathFindController.findPath(new Point((int)vertex1.Position.X, (int)vertex1.Position.Y), new Point((int)vertex2.Position.X, (int)vertex2.Position.Y), new PathFindController.isAtEnd(PathFindController.isAtEndPoint), Location, Game1.player, 9999);
                    // TODO Use Pathfinder distance
                    double dist;
                    string edgeLabel;
                    if (path != null)
                    {
                        dist = (float)path.Count;
                        // TODO Player can run diagonally. Account for that.
                        edgeLabel = Location.Name + " - " + dist + "c";

                    }
                    else
                    {
                        dist = (int)StardewVertex.Distance(vertex1, vertex2);
                        edgeLabel = Location.Name + " - " + dist + "d";
                    }
                    var edge = new StardewEdge(vertex1, vertex2, edgeLabel);
                    AddEdge(edge);

                }
                AddVertex(vertex1);
            }
            AddPlayerVertex(new MovableVertex(Location, new Vector2(0,0)));
            AddTargetVertex(new MovableVertex(Location, new Vector2(0, 0)));
            ConnectPlayerVertex();
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
            PlayerEdges = new List<PlayerEdge>();
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
        [Obsolete] // Maybe needed later for pathfinder calculation
        public void UpdatePlayerEdgeCosts(Vector2 position)
        {
            PlayerVertex.SetPosition(position);
            foreach(PlayerEdge edge in PlayerEdges)
            {
                
            }
        }

        private void AddTargetVertex(MovableVertex w)
        {
            if (TargetVertex == null)
            {
                AddVertex(w);
                TargetVertex = w;
            }
            else
            {
                throw new InvalidOperationException("Target vertex already added");
            }
        }
    }
}
