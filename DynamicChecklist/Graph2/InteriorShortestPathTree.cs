namespace DynamicChecklist.Graph2
{
    using System;
    using System.Collections.Generic;
    using Priority_Queue;
    using StardewValley;

    /// <summary>
    /// Stores the undirected Single-Source Shortest Path tree from some root <see cref="WorldPoint"/> to all other passable coordinates in the same <see cref="GameLocation"/>, with shortest path directionality to allow reconstruction of the path.
    /// </summary>
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

        /// <summary>
        /// The LocationGraph this belongs to.
        /// </summary>
        private readonly LocationGraph parent;

        public InteriorShortestPathTree(LocationGraph parent, WorldPoint root)
        {
            this.parent = parent;
            this.Root = root;
            this.directions = new Direction[this.Height, this.Width];
            this.distances = new float[this.Height, this.Width];
            this.parent = parent;

            this.InitializeArrays();
            this.Calculate();
        }

        [Flags]
        public enum Direction : byte
        {
            None = 0,
            East = 0x01,
            NorthEast = 0x02,
            North = 0x04,
            NorthWest = 0x08,
            West = 0x10,
            SouthWest = 0x20,
            South = 0x40,
            SouthEast = 0x80,

            Orthogonal = NorthEast | NorthWest | SouthEast | SouthWest,
            Diagonal = East | North | West | South,
            AnyNorth = North | NorthEast | NorthWest,
            AnyEast = East | NorthEast | SouthEast,
            AnySouth = South | SouthEast | SouthWest,
            AnyWest = West | NorthWest | SouthWest,
        }

        /// <summary>
        /// Gets the height, in tile units, of the location
        /// </summary>
        public int Width => this.parent.Width;

        /// <summary>
        /// Gets the width, in tile units, of the location.
        /// </summary>
        public int Height => this.parent.Height;

        /// <summary>
        /// Gets the root node for this tree.
        /// </summary>
        public WorldPoint Root { get; private set; }

        /// <summary>
        /// Returns the walking distance between the <see cref="Root"/> to the specified point, if they are connected.
        /// </summary>
        /// <param name="point">The point to calculate the distance between</param>
        /// <returns>Walking distance, or <c>float.PositiveInfinity</c> if disconnected</returns>
        public float DistanceTo(WorldPoint point)
        {
#if DEBUG
            if (point.Location != this.parent.Location)
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
#if DEBUG
            if (point.Location != this.parent.Location)
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

                if ((dir & Direction.AnyNorth) != 0)
                {
                    y++;
                }
                else if ((dir & Direction.AnySouth) != 0)
                {
                    y--;
                }

                if ((dir & Direction.AnyEast) != 0)
                {
                    x++;
                }
                else if ((dir & Direction.AnyWest) != 0)
                {
                    x--;
                }

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

        private void InitializeArrays()
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
            this.distances[this.Root.Y, this.Root.X] = 0f;

            var q = new FastPriorityQueue<CoordNode>(this.parent.NumPassable);
            var allNodes = new CoordNode[this.Height, this.Width];
            var rootNode = allNodes[this.Root.Y, this.Root.X] = new CoordNode(this.Root.X, this.Root.Y);
            q.Enqueue(rootNode, 0f);

            while (q.Count > 0)
            {
                var node = q.Dequeue();

                // Directions here are where we're coming from, the relative coordinates are where we're going to.
                // Group by Y coordinate for cache-line coherency.
                // Diagonals first, since those are heuristically shorter.
                this.VisitNode(q, node, allNodes, Direction.NorthEast, -1, -1);
                this.VisitNode(q, node, allNodes, Direction.NorthWest, 1, -1);
                this.VisitNode(q, node, allNodes, Direction.SouthEast, -1, 1);
                this.VisitNode(q, node, allNodes, Direction.SouthWest, 1, 1);

                // Orthogonals second
                this.VisitNode(q, node, allNodes, Direction.North, 0, -1);
                this.VisitNode(q, node, allNodes, Direction.East, -1, 0);
                this.VisitNode(q, node, allNodes, Direction.West, 1, 0);
                this.VisitNode(q, node, allNodes, Direction.South, 0, 1);
            }
        }

        /// <summary>
        /// Visit some place relative to the current node that was popped off the Dijkstra's Algorithm queue.
        /// </summary>
        /// <param name="q">The Dijkstra's algorithm queue</param>
        /// <param name="source">The node we're visiting FROM</param>
        /// <param name="allNodes">The grid of all possible nodes</param>
        /// <param name="sourceDir">The direction we're visiting FROM</param>
        /// <param name="dx">The relative X coordinate of this node [-1,0,1]</param>
        /// <param name="dy">The relative Y coordinate of this node [-1,0,1]</param>
        private void VisitNode(FastPriorityQueue<CoordNode> q, CoordNode source, CoordNode[,] allNodes, Direction sourceDir, int dx, int dy)
        {
            int x = source.X + dx;
            int y = source.Y + dy;
            if (x < 0 || x >= this.Width || y < 0 || y >= this.Height)
            {
                return;
            }

            bool canPass = this.parent.Passable[y, x];
            float distance = 1f;

            if (canPass && (sourceDir == Direction.NorthEast || sourceDir == Direction.SouthEast || sourceDir == Direction.SouthWest || sourceDir == Direction.NorthWest))
            {
                // Diagonals must be passable via at least one corner. Two adjacent corners being impassable prevents movement
                // Corners are simply the other two coordinates in the "square" defined by the source and destination.
                // Since both the source and destination are valid coordinates, no need to re-check before turning into an index.
                if (this.parent.Passable[source.Y, x] || this.parent.Passable[y, source.X])
                {
                    distance = SQRT2;
                }
                else
                {
                    canPass = false;
                }
            }

            if (!canPass)
            {
                return;
            }

            var oldDistance = this.distances[y, x];
            var newDistance = this.distances[source.Y, source.X] + distance;
            if (newDistance < oldDistance)
            {
                this.distances[y, x] = newDistance;
                this.directions[y, x] = sourceDir; // Change incoming direction

                var destNode = allNodes[y, x];
                if (destNode == null)
                {
                    destNode = allNodes[y, x] = new CoordNode(x, y);
                    q.Enqueue(destNode, newDistance);
                }
                else
                {
                    q.UpdatePriority(destNode, newDistance);
                }
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
