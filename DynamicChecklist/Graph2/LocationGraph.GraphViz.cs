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
            private Dictionary<LocationReference, string> locationPrefixes;
            private string prefix;

            public GraphViz(LocationGraph graph, Dictionary<LocationReference, string> locationPrefixes)
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

                foreach (var point in g.waypoints.Keys.OrderBy(wp => wp.Y))
                {
                    this.WriteNode(point, io);
                }

                io.WriteLine('}');
            }

            public void WriteEdges(TextWriter io)
            {
                foreach (var node in this.graph.WarpOutNodes)
                {
                    var targetPrefix = this.locationPrefixes[node.Target.Location];
                    var label = node.ToString().Replace("\"", "\\\"");
                    io.WriteLine($"{this.prefix}_{node.Source.ToPortName()} -> {targetPrefix}_{node.Target.ToPortName()} [weight=1, tooltip=\"{label}\"];");
                }
            }

            private void WriteNode(WorldPoint point, TextWriter io)
            {
                var waypoint = this.graph.GetWaypoint(point);
                string attrs = $"label=\"{point.ToCoordString()}\"";
                var hasInbound = waypoint.HasInbound;
                var isOutbound = waypoint.IsOutbound;
                if (hasInbound && isOutbound)
                {
                    attrs += ", color=\"#88FFFF\"";
                }
                else if (hasInbound)
                {
                    attrs += ", color=\"#8888FF\"";
                }
                else if (isOutbound)
                {
                    attrs += ", color=\"#88FF88\"";
                }
                else
                {
                    attrs += ", color=\"#888888\"";
                }

                io.WriteLine($" {this.prefix}_{point.ToPortName()} [{attrs}];");
            }
        }
    }
}
