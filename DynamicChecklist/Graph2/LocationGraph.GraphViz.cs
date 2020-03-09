namespace DynamicChecklist.Graph2
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using StardewValley;

    internal partial class LocationGraph
    {
        public class GraphViz
        {
            private LocationGraph graph;
            private Dictionary<GameLocation, string> locationPrefixes;
            private string prefix;

            public GraphViz(LocationGraph graph, Dictionary<GameLocation, string> locationPrefixes)
            {
                this.graph = graph;
                this.locationPrefixes = locationPrefixes;
                this.prefix = locationPrefixes[graph.Location];
            }

            public void WriteNode(TextWriter io)
            {
                var g = this.graph;
                io.WriteLine($"subgraph cluster{this.prefix} {{");
                io.WriteLine($" label = \"{g.Name}\";");

                WorldPoint prev = null;
                WorldPoint first = null;
                int n = 0;
                foreach (var waypoint in g.waypoints.OrderBy(wp => wp.Y))
                {
                    this.WriteNode(waypoint, io);
                    if (n == 0)
                    {
                        first = waypoint;
                    }
                    else
                    {
                        io.WriteLine($"{this.prefix}_{prev.ToPortName()} -> {this.prefix}_{waypoint.ToPortName()} [weight=1000, style=invis];");
                    }

                    prev = waypoint;
                    n++;
                }

                if (n >= 2)
                {
                    io.WriteLine($"{this.prefix}_{prev.ToPortName()} -> {this.prefix}_{first.ToPortName()} [weight=1000, style=invis];");
                }

                io.WriteLine('}');
            }

            public void WriteEdges(TextWriter io)
            {
                foreach (var node in this.graph.warpOutNodes.Values)
                {
                    var targetPrefix = this.locationPrefixes[node.Target.Location];
                    io.WriteLine($"{this.prefix}_{node.Source.ToPortName()} -> {targetPrefix}_{node.Target.ToPortName()} [weight=1];");
                }
            }

            private void WriteNode(WorldPoint waypoint, TextWriter io)
            {
                var i = this.graph.waypointIndex[waypoint];
                string attrs = $"label=\"{waypoint.ToCoordString()}\"";
                var hasWarpIn = this.graph.warpInNodes.ContainsKey(i);
                var hasWarpOut = this.graph.warpOutNodes.ContainsKey(i);
                if (hasWarpIn && hasWarpOut)
                {
                    attrs += ", color=\"#88FFFF\"";
                }
                else if (hasWarpIn)
                {
                    attrs += ", color=\"#8888FF\"";
                }
                else if (hasWarpOut)
                {
                    attrs += ", color=\"#88FF88\"";
                }
                else
                {
                    attrs += ", color=\"#888888\"";
                }

                io.WriteLine($" {this.prefix}_{waypoint.ToPortName()} [{attrs}];");
            }
        }
    }
}
