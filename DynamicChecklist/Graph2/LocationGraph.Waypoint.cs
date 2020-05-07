namespace DynamicChecklist.Graph2
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Xna.Framework.Graphics;
    using StardewValley;

    internal partial class LocationGraph
    {
        public class Waypoint
        {
            private InteriorShortestPathTree interiorTree;
            private ExteriorShortestPathTree exteriorTree;

            public Waypoint(LocationGraph parent, WorldPoint point)
            {
                this.Parent = parent;
                this.Point = point;
                this.InboundWarps = new HashSet<WarpNode>();
            }

            public LocationGraph Parent { get; private set; }

            public WorldPoint Point { get; private set; }

            public WarpNode OutboundWarp { get; set; }

            public bool IsOutbound => this.OutboundWarp != null;

            public HashSet<WarpNode> InboundWarps { get; private set; }

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
        }
    }
}