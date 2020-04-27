namespace DynamicChecklist.Graph2
{
    using System.Collections.Generic;
    using System.IO;
    using StardewValley;

    public partial class WorldGraph
    {
        public class GraphViz
        {
            private WorldGraph world;

            public GraphViz(WorldGraph world)
            {
                this.world = world;
            }

            public void Write(string filename)
            {
                using (var filestream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
                {
                    filestream.SetLength(0L); // truncate
                    using (var buffer = new BufferedStream(filestream))
                    {
                        using (var io = new StreamWriter(buffer))
                        {
                            this.Write(io);
                            io.Flush();
                        }
                    }
                }
            }

            public void Write(TextWriter io)
            {
                var n = 0;
                var locationPrefixes = new Dictionary<LocationReference, string>();
                var gvs = new List<LocationGraph.GraphViz>();
                foreach (var graph in this.world.locationGraphs.Values)
                {
                    locationPrefixes.Add(graph.Location, $"L{n++}");
                    var gv = new LocationGraph.GraphViz(graph, locationPrefixes);
                    gvs.Add(gv);
                }

                io.WriteLine("digraph world {");

                // Display suggestions:
                io.WriteLine("// display suggestions:");
                io.WriteLine("ranksep = 1;");
                io.WriteLine("rankdir = LR;");
                io.WriteLine("nodesep = .01;");
                io.WriteLine("concentrate = true;");
                io.WriteLine("remincross = true;");
                io.WriteLine("node[fontsize = 10, shape = box, margin = \"0.01,0.01\", height = 0, width = 0];");
                io.WriteLine("edge[minlen = 4];");
                io.WriteLine();

                foreach (var gv in gvs)
                {
                    gv.WriteNode(io);
                }

                foreach (var gv in gvs)
                {
                    gv.WriteEdges(io);
                }

                io.WriteLine("}");
            }
        }
    }
}
