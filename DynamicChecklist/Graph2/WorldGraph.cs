namespace DynamicChecklist.Graph2
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using StardewModdingAPI;
    using StardewValley;
    using StardewValley.Locations;

    public partial class WorldGraph
    {
        private static Dictionary<GameLocation, xTile.Dimensions.Size> locationSizes;
        private Dictionary<GameLocation, LocationGraph> locationGraphs;
        private List<WarpNode> cachedAllWarpNodes;
        private List<WarpNode> cachedAllReachableWarpNodes;

        public WorldGraph(IEnumerable<GameLocation> initialLocations)
        {
            locationSizes = new Dictionary<GameLocation, xTile.Dimensions.Size>();
            this.locationGraphs = new Dictionary<GameLocation, LocationGraph>();
            this.RebuildLocations(initialLocations);
        }

        public static IMonitor Monitor { get; set; }

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

        public static IEnumerable<GameLocation> AllLocations()
        {
            var seen = new HashSet<GameLocation>();
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
            if (!locationSizes.TryGetValue(location, out var size))
            {
                size = location.Map.GetLayer("Back").LayerSize;
                locationSizes.Add(location, size);
            }

            return size;
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
            var locations = this.locationGraphs.Keys.ToList();

            foreach (var location in removed)
            {
                locations.Remove(location);
            }

            foreach (var location in added)
            {
                locations.Add(location);
            }

            this.RebuildLocations(locations);
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
        /// Try to find the "Next Hop" for the player trying to reach an arbitrary point in the world.
        /// </summary>
        /// <remarks>
        /// When a path to the end point is not found, <paramref name="distance"/> is set to <see cref="float.PositiveInfinity"/> and <paramref name="nextHop"/> is set to <c>default</c>.
        /// If there is only one warp going out of the current location, the distance calculation is skipped and <paramref name="distance"/> is set to <see cref="float.MaxValue"/> and <paramref name="nextHop"/> is set to the position of the only available warp.
        /// When searching through a list of end points to find the closest one, be sure to set <paramref name="limit"/> to speed up the search.
        /// </remarks>
        /// <param name="end">The end point the player is trying to reach</param>
        /// <param name="distance">The total walking distance to the end point, or <see cref="float.PositiveInfinity"/> if no path possible</param>
        /// <param name="nextHop">The screen-space coordinates of the best warp going out of the player's location (<c>default</c> if no path possible)</param>
        /// <param name="limit">Limits the maximum walking distance of the path (paths equal to or longer than this limit will be skipped)</param>
        /// <returns>If a path to the end point could be found (if a <paramref name="limit"/> was supplied</returns>
        public bool TryFindNextHopForPlayer(WorldPoint end, out float distance, out Vector2 nextHop, float limit = float.PositiveInfinity)
        {
            var start = new WorldPoint(Game1.player.currentLocation, Game1.player.position.Value);
            return this.TryFindNextHop(start, end, out distance, out nextHop, limit);
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

            var endGraphInNodes = new List<WarpNode>(endGraph.WarpInNodes);
            WarpNode bestSourceNode = null;
            foreach (var sourceNode in startGraph.WarpOutNodes)
            {
                float startDistance = startGraph.GetDistanceToWarp(start, sourceNode);
                if (startDistance >= limit || startDistance >= distance)
                {
                    continue;
                }

                // TODO: sourceNode --> end distance can be cached!
                // Simple storage for time optimization.
                // Something like Dictionary<Tuple<WarpNode,WorldPoint>> and Add(new Tuple(sourceNode,end), distance)
                // or multi-layer it on end/WorldPoint, store it in the startGraph?
                // Cache should invalidate on any world graph change

                // Adjacent warp! No need to loop through all the end graph nodes; just check immediate distance!
                if (sourceNode.Target.Location == end.Location)
                {
                    float endDistance = startDistance + endGraph.GetDistanceFromWarp(sourceNode, end);
                    if (endDistance < limit && endDistance < distance)
                    {
                        distance = endDistance;
                        bestSourceNode = sourceNode;
                    }

                    continue;
                }

                // Otherwise, check the exterior and final leg
                foreach (var targetNode in endGraphInNodes)
                {
                    float middleDistance = startDistance + startGraph.GetExteriorDistance(sourceNode, targetNode);
                    if (middleDistance >= limit || middleDistance >= distance)
                    {
                        continue;
                    }

                    float endDistance = middleDistance + endGraph.GetDistanceFromWarp(targetNode, end);
                    if (endDistance < limit && endDistance < distance)
                    {
                        distance = endDistance;
                        bestSourceNode = sourceNode;
                    }
                }
            }

            if (bestSourceNode == null)
            {
                return false;
            }

            next = bestSourceNode;
            return true;
        }

        internal LocationGraph GetLocationGraph(GameLocation location)
        {
            LocationGraph graph;
            if (!this.locationGraphs.TryGetValue(location, out graph))
            {
                throw new KeyNotFoundException("Location not registered!");
            }

            return graph;
        }

        private void ClearCache()
        {
            this.cachedAllReachableWarpNodes = null;
            this.cachedAllWarpNodes = null;
        }

        private void RebuildLocations(IEnumerable<GameLocation> locations)
        {
            this.locationGraphs.Clear();

            foreach (var location in locations)
            {
                this.locationGraphs.Add(location, new LocationGraph(location, this));
            }

            foreach (var graph in this.locationGraphs.Values)
            {
                graph.Build();
            }
        }
    }
}
