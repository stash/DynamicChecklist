namespace DynamicChecklist.Graph2
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Priority_Queue;
    using StardewModdingAPI;

    /// <summary>
    /// Stores the undirected Single-Source Shortest Path tree from some root <see cref="WorldPoint"/> to all other passable coordinates in the same location, with shortest path directionality to allow reconstruction of the path.
    /// </summary>
    /// <remarks>
    /// Makes the assumption that the grid origin is in the top left, i.e., Right is X+ and Down is Y+.
    /// </remarks>
    [DebuggerDisplay("IntTree Root={Root}")]
    internal class InteriorShortestPathTree
    {
        private static readonly float SQRT2 = (float)Math.Sqrt(2);

        /// <summary>
        /// The Direction that the shortest path comes FROM, in y,x coordinates.
        /// </summary>
        private readonly Direction[,] directions; // y,x tile coordinates

        /// <summary>
        /// The walking distance between a point and the root, in y,x coordinates.
        /// </summary>
        private readonly float[,] distances; // y,x tile coordinates

        private readonly WeakReference<LocationGraph> parent;

        public InteriorShortestPathTree(LocationGraph parent, WorldPoint root)
        {
            this.parent = new WeakReference<LocationGraph>(parent);
            this.Root = root;
            this.RootIsPassable = parent.IsPassable(this.Root);
            if (!this.RootIsPassable)
            {
                MainClass.Log($"Root node {this.Root} is not passable", LogLevel.Warn);
                return;
            }

            this.directions = new Direction[this.Height, this.Width];
            this.distances = new float[this.Height, this.Width];
            this.InitializeDistances();
            this.Calculate();
        }

        /// <summary>
        /// Gets the LocationGraph this belongs to.
        /// </summary>
        public LocationGraph Parent => this.parent.TryGetTarget(out var value) ? value : null;

        /// <summary>
        /// Gets the height, in tile units, of the location
        /// </summary>
        public int Width => this.Parent.Width;

        /// <summary>
        /// Gets the width, in tile units, of the location.
        /// </summary>
        public int Height => this.Parent.Height;

        /// <summary>
        /// Gets the root node for this tree.
        /// </summary>
        public WorldPoint Root { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the root node is passable.
        /// </summary>
        public bool RootIsPassable { get; private set; }

        /// <summary>
        /// Returns the walking distance between the <see cref="Root"/> to the specified point, if they are connected.
        /// </summary>
        /// <param name="point">The point to calculate the distance between</param>
        /// <returns>Walking distance, or <c>float.PositiveInfinity</c> if disconnected</returns>
        public float DistanceTo(WorldPoint point)
        {
            if (!this.RootIsPassable)
            {
                return float.PositiveInfinity;
            }

#if DEBUG
            if (point.Location != this.Parent.Location)
            {
                throw new ArgumentException("Point is for different Location", nameof(point));
            }
#endif
            if (this.Root.Equals(point))
            {
                return 0;
            }

            if (point.X < 0 || point.X >= this.Width)
            {
                throw new ArgumentOutOfRangeException(nameof(point), point.X, "X coordinate out of range");
            }
            else if (point.Y < 0 || point.Y >= this.Height)
            {
                throw new ArgumentOutOfRangeException(nameof(point), point.Y, "Y coordinate out of range");
            }

            return this.distances[point.Y, point.X];
        }

        /// <summary>
        /// Generates the step-by-step path from the point to the root.
        /// </summary>
        /// <param name="point">Some point in this location</param>
        /// <returns>The path</returns>
        public IEnumerable<WorldPoint> PathToPoint(WorldPoint point)
        {
            if (!this.RootIsPassable)
            {
                yield break;
            }

#if DEBUG
            if (point.Location != this.Parent.Location)
            {
                throw new ArgumentException("Point is for different Location", nameof(point));
            }
#endif
            int x = point.X;
            int y = point.Y;
            var dir = this.directions[y, x];
            if (dir == Direction.None)
            {
                yield break; // impassable
            }

            while (true)
            {
                yield return new WorldPoint(point.Location, x, y);
                if (x == this.Root.X && y == this.Root.Y)
                {
                    break;
                }

                WorldGraph.DirectionTransform(dir, ref x, ref y);
                dir = this.directions[y, x];
            }
        }

        /// <summary>
        /// Generates the reverse of <see cref="PathToPoint(WorldPoint)"/>; if possible use that method as it's more efficient.
        /// </summary>
        /// <param name="point">Some point in this location</param>
        /// <returns>The path</returns>
        public IEnumerable<WorldPoint> PathToRoot(WorldPoint point)
        {
            var path = new List<WorldPoint>(this.PathToPoint(point));
            path.Reverse();
            return path;
        }

        private void InitializeDistances()
        {
            for (var y = 0; y < this.Height; y++)
            {
                for (var x = 0; x < this.Width; x++)
                {
                    this.distances[y, x] = float.PositiveInfinity;
                }
            }
        }

        /// <summary>
        /// Calculates Single-Source Shortest Path tree for this location using Dijkstra's algorithm.
        /// </summary>
        /// <remarks>
        /// Uses a heuristic to prefer diagonals over orthorganal direction visits to produce less priority queue churn.
        /// </remarks>
        private void Calculate()
        {
            const int down = 1;
            const int up = -1;
            const int right = 1;
            const int left = -1;

            this.SetupQueue(out var q, out var allNodes);

            while (q.Count > 0)
            {
                var source = q.Dequeue();
                this.distances[source.Y, source.X] = source.Priority;

                // Directions here are where we're coming FROM, the relative coordinates are where we're going TO.
                // That is, from the node we're going to visit, go this direction to get to `source`
                // Group by Y coordinate then sort by X coordinate for cache-line coherency.
                // Diagonals first, since those are heuristically shorter than taking two orthogonal steps.
                this.VisitNode(q, allNodes, source, Direction.UpRight, left, down);
                this.VisitNode(q, allNodes, source, Direction.UpLeft, right, down);
                this.VisitNode(q, allNodes, source, Direction.DownRight, left, up);
                this.VisitNode(q, allNodes, source, Direction.DownLeft, right, up);

                // Orthogonals second
                this.VisitNode(q, allNodes, source, Direction.Up, 0, down);
                this.VisitNode(q, allNodes, source, Direction.Right, left, 0);
                this.VisitNode(q, allNodes, source, Direction.Left, right, 0);
                this.VisitNode(q, allNodes, source, Direction.Down, 0, up);
            }
        }

        private void SetupQueue(out FastPriorityQueue<CoordNode> q, out CoordNode[,] allNodes)
        {
            q = new FastPriorityQueue<CoordNode>(this.Height * this.Width);
            allNodes = new CoordNode[this.Height, this.Width];
            for (var y = 0; y < this.Height; y++)
            {
                for (var x = 0; x < this.Width; x++)
                {
                    if (this.Parent.Passable[y, x])
                    {
                        allNodes[y, x] = new CoordNode(x, y);
                        q.Enqueue(allNodes[y, x], float.PositiveInfinity);
                    }
                }
            }

            // Root node has to be first at distance zero
            q.UpdatePriority(allNodes[this.Root.Y, this.Root.X], 0f);
        }

        /// <summary>
        /// Visit some place relative to the current node that was popped off the Dijkstra's Algorithm queue.
        /// </summary>
        /// <param name="q">The Dijkstra's algorithm queue</param>
        /// <param name="allNodes">The grid of all possible nodes</param>
        /// <param name="source">The node we're visiting FROM</param>
        /// <param name="sourceDir">The direction we're visiting FROM</param>
        /// <param name="dx">The relative X coordinate of this node [-1,0,1]</param>
        /// <param name="dy">The relative Y coordinate of this node [-1,0,1]</param>
        private void VisitNode(FastPriorityQueue<CoordNode> q, CoordNode[,] allNodes, CoordNode source, Direction sourceDir, int dx, int dy)
        {
            int x = source.X + dx;
            int y = source.Y + dy;
            if (x < 0 || x >= this.Width || y < 0 || y >= this.Height)
            {
                return;
            }

            var node = allNodes[y, x];
            if (node == null /*not passable*/ || !q.Contains(node) /*already visited*/)
            {
                return;
            }

            float distance = 1f;
            if ((sourceDir & Direction.Diagonal) != 0)
            {
                // Diagonal movement must be passable via at least one corner. Two adjacent corners being impassable prevents movement.
                // Corners are simply the other two coordinates in the "square" defined by the source and destination.
                if (this.Parent.Passable[source.Y, x] || this.Parent.Passable[y, source.X])
                {
                    distance = SQRT2;
                }
                else
                {
                    return; // Can't pass diagonally
                }
            }

            var sourceDistance = source.Priority;
            var oldDistance = node.Priority;
            var newDistance = sourceDistance + distance;
            if (newDistance < oldDistance)
            {
                this.directions[y, x] = sourceDir; // Change incoming direction
                q.UpdatePriority(node, newDistance);
            }
        }

        internal class CoordNode : FastPriorityQueueNode
        {
            public CoordNode(int x, int y)
                : base()
            {
                this.X = x;
                this.Y = y;
            }

            public int X { get; private set; }

            public int Y { get; private set; }
        }
    }
}
