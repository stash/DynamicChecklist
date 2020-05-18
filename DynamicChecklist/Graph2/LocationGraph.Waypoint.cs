namespace DynamicChecklist.Graph2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using StardewValley;

    internal partial class LocationGraph
    {
        public class Waypoint
        {
            private readonly WeakReference<LocationGraph> parent;
            private InteriorShortestPathTree interiorTree;
            private ExteriorShortestPathTree exteriorTree;

            public Waypoint(LocationGraph parent, WorldPoint point)
            {
                this.parent = new WeakReference<LocationGraph>(parent);
                this.Point = point;
                this.InboundWarps = new HashSet<WarpNode>();
            }

            public LocationGraph Parent => this.parent.TryGetTarget(out var value) ? value : null;

            public WorldPoint Point { get; private set; }

            public WarpNode OutboundWarp { get; set; }

            public bool IsOutbound => this.OutboundWarp != null;

            public HashSet<WarpNode> InboundWarps { get; internal set; }

            public bool HasInbound => this.InboundWarps.Count > 0;

            public InteriorShortestPathTree InteriorTree
            {
                get
                {
                    if (this.interiorTree == null)
                    {
                        this.BuildInteriorTree();
                    }

                    return this.interiorTree;
                }
            }

            public bool HasCalculatedInteriorTree => this.interiorTree != null;

            public ExteriorShortestPathTree ExteriorTree
            {
                get
                {
                    if (this.exteriorTree == null)
                    {
                        this.BuildExteriorTree();
                    }

                    return this.exteriorTree;
                }
            }

            public bool HasCalculatedExteriorTree => this.exteriorTree != null;

            public HashSet<LocationReference> InboundLocations
            {
                get
                {
                    return new HashSet<LocationReference>(this.InboundWarps.Select(warp => warp.Source.Location));
                }
            }

            public void ClearTrees()
            {
                this.interiorTree = null;
                this.exteriorTree = null;
            }

            public void BuildInteriorTree()
            {
                this.interiorTree = new InteriorShortestPathTree(this.Parent, this.Point);
            }

            public void BuildExteriorTree()
            {
                this.exteriorTree = new ExteriorShortestPathTree(this.Parent, this.OutboundWarp);
            }

            /// <summary>
            /// Find adjacent clusters of <see cref="WarpNode"/>s that are inbound to this waypoint.
            /// </summary>
            /// <returns>A series of <see cref="LinkedList{T}"/> with at least two <see cref="WarpNode"/>s each</returns>
            public IEnumerable<LinkedList<WarpNode>> IdentifyWarpSourceClusters()
            {
                var remainder = new LinkedList<WarpNode>(this.InboundWarps);
                while (remainder.Count > 1)
                {
                    LinkedListNode<WarpNode> root = remainder.First;
                    remainder.Remove(root);
                    var cluster = new LinkedList<WarpNode>();
                    cluster.AddLast(root);

                    // Starting with the root, for each node in the contiguous set...
                    LinkedListNode<WarpNode> current = cluster.First;
                    while (current != null)
                    {
                        // ... compare it to every node in the remainder set ...
                        LinkedListNode<WarpNode> remnant = remainder.First;
                        while (remnant != null)
                        {
                            // ... and add that remnant to the contiguous set if it's Adjacent to the current node
                            // (Adjacent means in the same Location and within one tile, including diagonals)
                            var candidate = remnant;
                            remnant = remnant.Next; // Need to step forward before we test & extract
                            if (candidate.Value.Source.Location == current.Value.Source.Location &&
                                Math.Abs(candidate.Value.Source.X - current.Value.Source.X) <= 1 &&
                                Math.Abs(candidate.Value.Source.Y - current.Value.Source.Y) <= 1)
                            {
                                remainder.Remove(candidate);
                                cluster.AddLast(candidate);
                            }
                        }

                        current = current.Next;
                    }

                    if (cluster.Count >= 2)
                    {
                        yield return cluster;
                    }
                }
            }
        }
    }
}