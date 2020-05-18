namespace DynamicChecklist.Graph2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using StardewModdingAPI;
    using StardewValley;
    using StardewValley.Locations;

    public partial class WorldGraph : IDisposable
    {
        private static Direction[] nearbyDirectionSearch = { Direction.Up, Direction.Down, Direction.Left, Direction.Right, Direction.UpLeft, Direction.UpRight, Direction.DownLeft, Direction.DownRight };

        private Dictionary<LocationReference, LocationGraph> locationGraphs;
        private List<WarpNode> cachedAllWarpNodes;
        private List<WarpNode> cachedAllReachableWarpNodes;

        public WorldGraph(IEnumerable<GameLocation> initialLocations)
            : this(initialLocations.Select(location => LocationReference.For(location)))
        {
        }

        public WorldGraph(IEnumerable<LocationReference> initialReferences)
        {
            this.locationGraphs = new Dictionary<LocationReference, LocationGraph>();
            this.RebuildLocations(initialReferences);
        }

        /// <summary>
        /// Gets the list of all known warp nodes
        /// </summary>
        internal List<WarpNode> AllWarpNodes
        {
            get
            {
                if (this.cachedAllWarpNodes == null)
                {
                    this.cachedAllWarpNodes = new List<WarpNode>();
                    foreach (var graph in this.locationGraphs.Values)
                    {
                        this.cachedAllWarpNodes.AddRange(graph.WarpOutNodes);
                    }
                }

                return this.cachedAllWarpNodes;
            }
        }

        /// <summary>
        /// Gets the list of all WarpNodes for locations with in-degree > 0, i.e., that can be reached through some warp
        /// </summary>
        internal List<WarpNode> AllReachableWarpNodes
        {
            get
            {
                if (this.cachedAllReachableWarpNodes == null)
                {
                    this.cachedAllReachableWarpNodes = new List<WarpNode>();
                    foreach (var graph in this.locationGraphs.Values)
                    {
                        if (graph.InDegree > 0)
                        {
                            this.cachedAllReachableWarpNodes.AddRange(graph.WarpOutNodes);
                        }
                    }
                }

                return this.cachedAllReachableWarpNodes;
            }
        }

        public static IEnumerable<LocationReference> AllLocations()
        {
            var seen = new HashSet<LocationReference>();
            foreach (var location in Game1.locations)
            {
                // if successfully added, means we haven't seen it yet
                if (seen.Add(location))
                {
                    yield return location;
                }

                if (location is BuildableGameLocation bgl)
                {
                    foreach (var building in bgl.buildings)
                    {
                        var indoors = building.indoors.Value;
                        if (indoors != null && seen.Add(indoors))
                        {
                            yield return indoors;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the tile size of some location.
        /// </summary>
        /// <param name="location">Some location</param>
        /// <returns>The tile-space dimensions</returns>
        public static xTile.Dimensions.Size LocationSize(GameLocation location)
        {
            return location.Map.GetLayer("Back").LayerSize;
        }

        /// <summary>
        /// Transform an x and y coordinate using a <see cref="Direction"/>
        /// </summary>
        /// <param name="dir">The direction</param>
        /// <param name="x">The x coordinate to alter</param>
        /// <param name="y">The y coordinate to alter</param>
        public static void DirectionTransform(Direction dir, ref int x, ref int y)
        {
            if ((dir & Direction.AnyRight) != 0)
            {
                x++;
            }
            else if ((dir & Direction.AnyLeft) != 0)
            {
                x--;
            }

            if ((dir & Direction.AnyDown) != 0)
            {
                y++;
            }
            else if ((dir & Direction.AnyUp) != 0)
            {
                y--;
            }
        }

        /// <summary>
        /// Is the given location a procedurally-generated one? (e.g., UndergroundMine levels)
        /// </summary>
        /// <param name="location">The game location</param>
        /// <returns>If it is a procedurally-generated location</returns>
        public static bool IsProceduralLocation(GameLocation location)
        {
            // Both the regular and desert/skull mine levels are prefixed with this
            return location.Name.IndexOf("UndergroundMine") == 0 || location.Name == "Temp";
        }

        public void Dispose()
        {
            this.ClearCache();
            this.locationGraphs.Clear();
        }

        /// <summary>
        /// React to the SMAPI LocationListChanged event by recomputing the world graph.
        /// </summary>
        /// <param name="added">Added locations</param>
        /// <param name="removed">Removed locations</param>
        public void LocationListChanged(IEnumerable<GameLocation> added, IEnumerable<GameLocation> removed)
        {
            this.ClearCache();

            // TODO: make this whole method more efficient. Can make some optimizations for "Leaf" (a.k.a., Dead-End) locations.
            // Until then, yes, have to recompute the entire world graph :(
            // Doing a minor optimization by ignoring procedurally-generated locations (e.g., mines)
            var locations = this.locationGraphs.Keys.ToList();
            int changes = 0;

            foreach (var location in removed)
            {
                if (!IsProceduralLocation(location) && locations.Remove(location))
                {
                    MainClass.Log($"Location removed: {location.NameOrUniqueName}");
                    changes++;
                }
            }

            foreach (var location in added)
            {
                if (!IsProceduralLocation(location))
                {
                    MainClass.Log($"Location added: {location.NameOrUniqueName}");
                    locations.Add(location);
                    changes++;
                }
            }

            if (changes > 0)
            {
                this.RebuildLocations(locations);
            }
        }

        /// <summary>
        /// Tests to see if the player only has one warp out of the current location.
        /// </summary>
        /// <param name="onlyHop">The screen-space coordinates of the warp if there's one way out, <c>default</c> otherwise</param>
        /// <returns>If there's only one way out</returns>
        public bool PlayerHasOnlyOneWayOut(out Vector2 onlyHop)
        {
            var graph = this.GetLocationGraph(Game1.player.currentLocation);
            if (graph.OutDegree == 1)
            {
                onlyHop = graph.FirstWarpOutNode.Source;
                return true;
            }

            onlyHop = default;
            return false;
        }

        /// <summary>
        /// Try to find the "Next Hop" leading away from some start point to reach an arbitrary point in the world.
        /// </summary>
        /// <remarks>
        /// When a path to the end point is not found, <paramref name="distance"/> is set to <see cref="float.PositiveInfinity"/> and <paramref name="nextHop"/> is set to <c>default</c>.
        /// When searching through a list of end points to find the closest one, be sure to set <paramref name="limit"/> to speed up the search.
        /// </remarks>
        /// <param name="start">The starting point</param>
        /// <param name="end">The ending point</param>
        /// <param name="distance">The total walking distance to the end point, or <see cref="float.PositiveInfinity"/> if no path possible</param>
        /// <param name="nextHop">The screen-space coordinates of the best warp going out of the player's location (<c>default</c> if no path possible)</param>
        /// <param name="limit">Limits the maximum walking distance of the path (paths equal to or longer than this limit will be skipped)</param>
        /// <returns>If a path to the end point could be found (if a <paramref name="limit"/> was supplied</returns>
        public bool TryFindNextHop(WorldPoint start, WorldPoint end, out float distance, out Vector2 nextHop, float limit = float.PositiveInfinity)
        {
            var startGraph = this.GetLocationGraph(start.Location);
            if (!startGraph.IsPassable(start))
            {
                this.FindClosestPassablePoint(ref start);
            }

            if (this.TryFindNextWarpNode(start, end, out distance, out var next, limit))
            {
                nextHop = next.Source;
                return true;
            }

            nextHop = default;
            distance = float.PositiveInfinity;
            return false;
        }

        internal bool TryFindNextWarpNode(WorldPoint start, WorldPoint end, out float distance, out WarpNode next, float limit = float.PositiveInfinity)
        {
            next = default;
            distance = float.PositiveInfinity;
            var startGraph = this.GetLocationGraph(start.Location);
            var endGraph = this.GetLocationGraph(end.Location);

#if DEBUG
            if (start.Location == end.Location)
            {
                throw new ArgumentException("Cannot have same start and target location", nameof(end));
            }
#endif
            if (startGraph.OutDegree == 0 /* trapped! */ || endGraph.InDegree == 0 /* unreachable! */)
            {
                return false;
            }

            var endNodes = endGraph.WarpInNodes.ToArray();

            foreach (var startNode in startGraph.WarpOutNodes)
            {
                float startDistance = startGraph.GetDistanceToWarp(start, startNode);
                if (startDistance >= limit || startDistance >= distance)
                {
                    continue;
                }

                foreach (var endNode in endNodes)
                {
                    float middleDistance = startDistance + this.GetDistanceBetweenWarps(startNode, endNode);
                    if (middleDistance >= limit || middleDistance >= distance)
                    {
                        continue;
                    }

                    float totalDistance = middleDistance + endGraph.GetDistanceFromWarp(endNode, end);
                    if (totalDistance < limit && totalDistance < distance)
                    {
                        distance = totalDistance;
                        next = startNode;
                    }
                }
            }

            return next != null;
        }

        internal LocationGraph GetLocationGraph(LocationReference location)
        {
            LocationGraph graph;
            if (!this.locationGraphs.TryGetValue(location, out graph))
            {
                throw new KeyNotFoundException("Location not registered!");
            }

            return graph;
        }

        internal void BuildAllInteriors()
        {
            foreach (var graph in this.locationGraphs.Values)
            {
                graph.BuildAllInteriors();
            }
        }

        internal void BuildAllExteriors()
        {
            foreach (var graph in this.locationGraphs.Values)
            {
                graph.BuildAllExteriors();
            }
        }

        private void FindClosestPassablePoint(ref WorldPoint start)
        {
            var graph = this.GetLocationGraph(start.Location);
            foreach (var dir in nearbyDirectionSearch)
            {
                var x = start.X;
                var y = start.Y;
                DirectionTransform(dir, ref x, ref y);
                if (y >= 0 && y < graph.Height && x >= 0 && x < graph.Width && graph.Passable[y, x])
                {
                    start = new WorldPoint(start.Location, x, y);
                    return;
                }
            }
        }

        private void ClearCache()
        {
            this.cachedAllReachableWarpNodes = null;
            this.cachedAllWarpNodes = null;
        }

        private void RebuildLocations(IEnumerable<LocationReference> locations)
        {
            this.ClearCache();
            this.locationGraphs.Clear();

            foreach (var location in locations)
            {
                this.locationGraphs.Add(location, new LocationGraph(location, this));
            }

            foreach (var graph in this.locationGraphs.Values)
            {
                graph.BuildWarpOuts();
            }

            foreach (var graph in this.locationGraphs.Values)
            {
                graph.BuildWarpIns();
            }

            foreach (var graph in this.locationGraphs.Values)
            {
                graph.ConcentrateWarps();
            }
        }

        private float GetDistanceBetweenWarps(WarpNode from, WarpNode to)
        {
            if (from == to)
            {
                return 0f;
            }
            else if (from.Target.Location == to.Source.Location)
            {
                var graph = this.GetLocationGraph(from.Target.Location);
                return graph.GetInteriorDistance(from, to);
            }
            else
            {
                var graph = this.GetLocationGraph(from.Source.Location);
                return graph.GetExteriorDistance(from, to);
            }
        }
    }
}
