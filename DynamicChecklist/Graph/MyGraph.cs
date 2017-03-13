using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph.Graphviz;
using QuickGraph.Graphviz.Dot;
using StardewValley;
using QuickGraph;
using System.IO;
using Microsoft.Xna.Framework;

namespace DynamicChecklist.Graph
{
    public class MyGraph : AdjacencyGraph<ExtendedWarp, LabelledEdge<ExtendedWarp>>, IEdgeListGraph<ExtendedWarp, LabelledEdge<ExtendedWarp>>
    {
        public MyGraph() : base(false)
        {

        }

        public void Create()
        {
            var a = Game1.currentLocation.warps;

            foreach (GameLocation loc in Game1.locations)
            {
                //graph.AddVertex(loc.Name);
            }
            var partialGraphs = new List<AdjacencyGraph<ExtendedWarp, LabelledEdge<ExtendedWarp>>>();
            //var edgeCosts = new List<Dictionary<Edge<ExtendedWarp>, double>>();
            var edgeCost = new Dictionary<LabelledEdge<ExtendedWarp>, double>();
            foreach (GameLocation loc in Game1.locations)
            {
                if (loc.Name == "Farm")
                {

                }
                var partialGraph = new AdjacencyGraph<ExtendedWarp, LabelledEdge<ExtendedWarp>>(false);
                var extWarpsToInclude = new List<ExtendedWarp>();
                for (int i = 0; i < loc.warps.Count; i++)
                {
                    var extWarpNew = new ExtendedWarp(loc.warps.ElementAt(i), loc);
                    bool shouldAdd = true;
                    foreach (ExtendedWarp extWarpIncluded in extWarpsToInclude)
                    {
                        if (extWarpNew.TargetLocation == extWarpIncluded.TargetLocation && ExtendedWarp.Distance(extWarpNew, extWarpIncluded) < 5)
                        {
                            shouldAdd = false;
                            break;
                        }
                    }
                    if (shouldAdd)
                    {
                        extWarpsToInclude.Add(extWarpNew);
                        partialGraph.AddVertex(extWarpsToInclude.Last());
                    }

                }
                for (int i = 0; i < extWarpsToInclude.Count; i++)
                {
                    var extWarp1 = extWarpsToInclude.ElementAt(i);


                    for (int j = i + 1; j < extWarpsToInclude.Count; j++)
                    {
                        // TODO Dont add adjacant warp tiles or make Extended warp be combined from many warps
                        var LocTo = Game1.getLocationFromName(loc.warps.ElementAt(j).TargetName);
                        var extWarp2 = extWarpsToInclude.ElementAt(j);
                        var path = PathFindController.findPath(new Point(extWarp1.X, extWarp1.Y), new Point(extWarp2.X, extWarp2.Y), new PathFindController.isAtEnd(PathFindController.isAtEndPoint), loc, Game1.player, 9999);
                        float dist;
                        string edgeLabel;
                        if (path != null)
                        {
                            dist = (float)path.Count;
                            // TODO Player can run diagonally. Account for that.
                            edgeLabel = loc.Name + " - " + dist + "c";

                        }
                        else
                        {
                            dist = (int)Vector2.Distance(new Vector2(extWarp1.X, extWarp1.Y), new Vector2(extWarp2.X, extWarp2.Y));
                            edgeLabel = loc.Name + " - " + dist + "d";
                        }

                        var edge = new LabelledEdge<ExtendedWarp>(extWarp1, extWarp2, edgeLabel, new GraphvizColor(255, 255, 255, 255));
                        partialGraph.AddEdge(edge);
                        edgeCost.Add(edge, dist);

                    }
                    partialGraph.AddVertex(extWarp1);
                }
                partialGraphs.Add(partialGraph);
            }
            // Combine partial graphs into one
            var wholeGraph = new MyGraph();
            foreach (var partialGraph in partialGraphs)
            {
                wholeGraph.AddVertexRange(partialGraph.Vertices);
                wholeGraph.AddEdgeRange(partialGraph.Edges);
            }
            for (int i = 0; i < partialGraphs.Count; i++)
            {
                var graph1 = partialGraphs.ElementAt(i);
                for (int j = i + 1; j < partialGraphs.Count; j++)
                {
                    var graph2 = partialGraphs.ElementAt(j);
                    foreach (ExtendedWarp warp1 in graph1.Vertices)
                    {
                        if (warp1.OriginLocation.name == "Saloon")
                        {

                        }

                        foreach (ExtendedWarp warp2 in graph2.Vertices)
                        {
                            if (warp2.TargetLocation.Name == "Saloon")
                            {

                            }
                            if (ExtendedWarp.AreCorresponding(warp1, warp2))
                            {
                                var edge = new LabelledEdge<ExtendedWarp>(warp1, warp2, "Warp", new GraphvizColor(255, 255, 0, 0));
                                wholeGraph.AddEdge(edge);
                                edgeCost.Add(edge, 0);
                            }
                        }
                    }
                }
            }
            GraphvizAlgorithm<ExtendedWarp, LabelledEdge<ExtendedWarp>> graphviz = new GraphvizAlgorithm<ExtendedWarp, LabelledEdge<ExtendedWarp>>(wholeGraph);
            graphviz.FormatVertex += (sender2, args) => args.VertexFormatter.Label = args.Vertex.Label;
            graphviz.FormatEdge += (sender2, args) => { args.EdgeFormatter.Label.Value = args.Edge.Label; };
            graphviz.FormatEdge += (sender2, args) => { args.EdgeFormatter.FontGraphvizColor = args.Edge.Color; };
            graphviz.ImageType = GraphvizImageType.Jpeg;

            graphviz.Generate(new FileDotEngine(), "C:\\Users\\Gunnar\\Desktop\\graph123.jpeg");


            //var alg = new QuickGraph.Algorithms.ShortestPath.UndirectedDijkstraShortestPathAlgorithm<ExtendedWarp, Edge<ExtendedWarp>>;
        }
    }
    public class ExtendedWarp : Warp
    {
        public GameLocation OriginLocation;
        public GameLocation TargetLocation;
        public string Label;

        public ExtendedWarp(Warp w, GameLocation originLocation) : base(w.X, w.Y, w.TargetName, w.TargetX, w.TargetY, false)
        {
            this.OriginLocation = originLocation;
            TargetLocation = Game1.getLocationFromName(w.TargetName);
            this.Label = originLocation.name + " to " + w.TargetName;
        }

        public static bool AreCorresponding(ExtendedWarp warp1, ExtendedWarp warp2)
        {
            if (warp1.OriginLocation == warp2.TargetLocation && warp1.TargetLocation == warp2.OriginLocation)
            {
                if (Math.Abs(warp1.X - warp2.TargetX) + Math.Abs(warp1.Y - warp2.TargetY) < 5)
                {
                    return true;
                }
            }
            return false;
        }
        public static int Distance(ExtendedWarp warp1, ExtendedWarp warp2)
        {
            return Math.Abs(warp1.X - warp2.X) + Math.Abs(warp1.Y - warp2.Y);
        }
    }

    public class FileDotEngine : IDotEngine
    {
        public string Run(GraphvizImageType imageType, string dot, string outputFileName)
        {
            //using (StreamWriter writer = new StreamWriter(outputFileName))
            //{
            //    writer.Write(dot);
            //}

            //return System.IO.Path.GetFileName(outputFileName);

            string output = outputFileName;
            File.WriteAllText(output, dot);

            // assumes dot.exe is on the path:
            var args = string.Format(@"{0} -Tjpg -O", output);
            System.Diagnostics.Process.Start(@"C:\Users\Gunnar\Desktop\release\bin\dot.exe", args);
            return output;
        }
    }
    public class LabelledEdge<TVertex> : Edge<TVertex>
    {
        public string Label;
        public GraphvizColor Color;
        public LabelledEdge(TVertex source, TVertex target, string label, GraphvizColor color) : base(source, target)
        {
            this.Label = label;
            this.Color = color;
        }
    }
}
