namespace DynamicChecklist.Graph2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Priority_Queue;
    using StardewValley;

    /// <summary>
    /// Stores the directed Single-Source Shortest Path tree from some "Root" <see cref="WarpNode"/> to all other <see cref="WarpNode"/>s in the world.
    /// </summary>
    internal class ExteriorShortestPathTree
    {
        private LocationGraph parent;
        private Dictionary<WarpNode, float> distances;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExteriorShortestPathTree"/> class and calculates the Single-Source Shortest Path tree.
        /// </summary>
        /// <param name="graph">The parent <see cref="LocationGraph"/></param>
        /// <param name="root">The root of the tree</param>
        public ExteriorShortestPathTree(LocationGraph graph, WarpNode root)
        {
#if DEBUG
            if (root.Source.Location != graph.Location)
            {
                throw new ArgumentException($"Root {nameof(WarpNode)} isn't for the correct location", nameof(root));
            }
#endif
            this.parent = graph;
            this.Root = root;
            this.distances = new Dictionary<WarpNode, float>();
            this.Calculate();
        }

        public WarpNode Root { get; private set; }

        /// <summary>
        /// Gets the distance from the root node to some other <see cref="WarpNode"/>.
        /// </summary>
        /// <param name="node">Some node</param>
        /// <returns>The walking distance, which is zero for the root node itself, or <c>float.PositiveInfinity</c> if the nodes are not connected</returns>
        public float DistanceTo(WarpNode node)
        {
            if (this.Root == node)
            {
                return 0f;
            }

            return this.distances.TryGetValue(node, out var distance) ? distance : float.PositiveInfinity;
        }

        /// <summary>
        /// Calculates the Shortest Path spanning tree for the root node to all other <see cref="WarpNode"/>s in the world.
        /// </summary>
        private void Calculate()
        {
            var q = this.SetupQueue();
            while (q.Count > 0)
            {
                var inNode = q.Dequeue();
                var distance = inNode.Priority;
                this.distances.Add(inNode, distance);
                q.ResetNode(inNode); // recycle!

                var targetLocation = inNode.Target.Location;
                var targetGraph = this.parent.World.GetLocationGraph(targetLocation);
                foreach (var outNode in targetGraph.WarpOutNodes)
                {
                    var between = targetGraph.GetInterWarpDistance(inNode, outNode); // may be +infinity ...
                    var newDistance = distance + between; // ... which also makes this +infinity
                    var oldDistance = outNode.Priority;
                    if (newDistance < oldDistance)
                    {
                        q.UpdatePriority(outNode, newDistance);
                    }
                }
            }
        }

        private FastPriorityQueue<WarpNode> SetupQueue()
        {
            var allNodes = this.parent.World.AllWarpNodes;
            var queue = new FastPriorityQueue<WarpNode>(allNodes.Count);
            queue.Enqueue(this.Root, 0f);

            foreach (var node in allNodes)
            {
                if (node != this.Root)
                {
                    queue.Enqueue(node, float.PositiveInfinity);
                }
            }

            return queue;
        }
    }
}
