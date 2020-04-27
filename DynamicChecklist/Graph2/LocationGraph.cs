namespace DynamicChecklist.Graph2
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Locations;
    using xTile.Dimensions;
    using xTile.Layers;

    [DebuggerDisplay("For:{Name} In:{InDegree} Out:{OutDegree}")]
    internal partial class LocationGraph
    {
        // Copied from Pathoschild/Datalayers
        private static readonly HashSet<string> WarpActionStrings = new HashSet<string> { "EnterSewer", "LockedDoorWarp", "Mine", "Theater_Entrance", "Warp", "WarpCommunityCenter", "WarpGreenhouse", "WarpMensLocker", "WarpWomensLocker", "WizardHatch" };

        // Copied from Pathoschild/Datalayers
        private static readonly HashSet<string> TouchWarpActionStrings = new HashSet<string> { "Door", "MagicWarp" };

        private Dictionary<WorldPoint, int> waypointIndex = new Dictionary<WorldPoint, int>();
        private List<WorldPoint> waypoints = new List<WorldPoint>();
        private Dictionary<int, WarpNode> warpOutNodes = new Dictionary<int, WarpNode>();
        private Dictionary<int, HashSet<WarpNode>> warpInNodes = new Dictionary<int, HashSet<WarpNode>>();
        private Dictionary<int, InteriorShortestPathTree> interiorTrees = new Dictionary<int, InteriorShortestPathTree>();
        private Dictionary<int, ExteriorShortestPathTree> exteriorTrees = new Dictionary<int, ExteriorShortestPathTree>();

        public LocationGraph(LocationReference location, WorldGraph world)
        {
            this.World = world;
            this.Location = location;

            var dim = WorldGraph.LocationSize(location);
            this.Width = dim.Width;
            this.Height = dim.Height;
#if DEBUG
            if (this.Width <= 0 || this.Width > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(location));
            }
            else if (this.Height <= 0 || this.Height > ushort.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(location));
            }
#endif

            this.Passable = new bool[this.Height, this.Width];
            this.BuildPassability();

        }

        public WorldGraph World { get; private set; }

        public LocationReference Location { get; private set; }

        public int Height { get; private set; }

        public int Width { get; private set; }

        public int OutDegree => this.warpOutNodes.Count;

        public int InDegree { get; private set; } = 0;

        public string Name => this.Location.Name;

        public bool IsDisconnected => this.OutDegree == 0 && this.InDegree == 0;

        /// <summary>Gets a collection of outbound <see cref="WarpNode"/>s</summary>
        public ICollection<WarpNode> WarpOutNodes => this.warpOutNodes.Values;

        public WarpNode FirstWarpOutNode => this.warpOutNodes.Values.FirstOrDefault();

        /// <summary>Gets an enumeration of inbound <see cref="WarpNode"/>s</summary>
        public IEnumerable<WarpNode> WarpInNodes => this.warpInNodes.SelectMany(pair => pair.Value.AsEnumerable());

        internal bool[,] Passable { get; private set; } // y, x

        /// <summary>
        /// Creates and adds new <see cref="WarpNode"/> for this location, corresponding to <paramref name="warp"/>.
        /// </summary>
        /// <remarks>Won't be added if out of tile coordinates range for the location (slightly out of range will be clamped).</remarks>
        /// <param name="warp">The warp to add</param>
        /// <param name="isDoorWarp">If the warp belongs to a door</param>
        /// <returns>If the warp was added (can't be duplicated, must be in range)</returns>
        /// <seealso cref="AddWarpOut(WarpNode)"/>
        public bool AddWarpOut(Warp warp)
        {
            if (!WorldPoint.InRange(LocationReference.For(warp.TargetName), warp.TargetX, warp.TargetY))
            {
                WorldGraph.Monitor.Log($"Warp target out of range {warp.X},{warp.Y}@{this.Name} -> {warp.TargetX},{warp.TargetY}@{warp.TargetName}", StardewModdingAPI.LogLevel.Trace);
                return false;
            }
            else if (!WorldPoint.InRange(this.Location, warp.X, warp.Y))
            {
                WorldGraph.Monitor.Log($"Warp out of range {warp.X},{warp.Y}@{this.Name} -> {warp.TargetX},{warp.TargetY}@{warp.TargetName}", StardewModdingAPI.LogLevel.Trace);
                return false;
            }

            return this.AddWarpOut(new WarpNode(warp, this.Location));
        }

        /// <summary>
        /// Add an outbound <see cref="WarpNode"/> to this location. Duplicate warps cannot be added.
        /// </summary>
        /// <param name="node">A node with this location as the Source</param>
        /// <returns>If the warp was added (can't be duplicated, must be in range)</returns>
        /// <seealso cref="AddWarpOut(Warp, bool)"/>
        public bool AddWarpOut(WarpNode node)
        {
#if DEBUG
            if (node.Source.Location != this.Location)
            {
                throw new ArgumentException(nameof(WarpNode) + " does not belong to this location", nameof(node));
            }
#endif

            var index = this.GetWaypointIndex(node.Source);
            if (!this.Passable[node.Source.Y, node.Source.X])
            {
                this.Passable[node.Source.Y, node.Source.X] = true; // Force source to be passable
                WorldGraph.Monitor.Log($"Forcing Passable warp source: {node.Source}", StardewModdingAPI.LogLevel.Warn);
            }

            if (!this.warpOutNodes.ContainsKey(index))
            {
                this.warpOutNodes.Add(index, node);
                return true;
            }
#if DEBUG
            else
            {
                WorldGraph.Monitor.Log($"Ignoring duplicate WarpOut: {node}");
            }
#endif

            return false;
        }

        /// <summary>
        /// Registers an inbound node to this location.
        /// </summary>
        /// <param name="node">A node with this location as the Target</param>
        /// <returns>True if the target point for the node was newly added, false otherwise</returns>
        private bool AddWarpIn(WarpNode node)
        {
#if DEBUG
            if (node.Target.Location != this.Location)
            {
                throw new ArgumentException(nameof(WarpNode) + " does not point to this location", nameof(node));
            }
#endif

            var index = this.GetWaypointIndex(node.Target);
            if (!this.warpInNodes.TryGetValue(index, out var nodeSet))
            {
                nodeSet = new HashSet<WarpNode>();
                this.warpInNodes.Add(index, nodeSet);
            }

            if (nodeSet.Add(node))
            {
                this.InDegree++;
                return true;
            }
#if DEBUG
            else
            {
                WorldGraph.Monitor.Log($"Ignoring duplicate WarpIn: {node}");
            }
#endif

            return false;
        }

        /// <summary>
        /// Gets the distance from an arbitrary point to the specified outbound warp in this location
        /// </summary>
        /// <param name="point">An arbitrary point inside this location</param>
        /// <param name="outNode">Outbound warp node, source is this location</param>
        /// <returns>The distance, or <c>float.PositiveInfinity</c> if no path is possible</returns>
        public float GetDistanceToWarp(WorldPoint point, WarpNode outNode)
        {
#if DEBUG
            if (point.Location != this.Location)
            {
                throw new ArgumentException("Point isn't for this location", nameof(point));
            }
            else if (outNode.Source.Location != this.Location)
            {
                throw new ArgumentException("Warp node doesn't start in this location", nameof(outNode));
            }
#endif

            return this.InteriorDistance(outNode.Source, point);
        }

        /// <summary>
        /// Gets the distance from the inbound warp to an arbitrary position.
        /// </summary>
        /// <param name="inNode">Inbound warp node, target is this location</param>
        /// <param name="point">An arbitrary point inside this location</param>
        /// <returns>The walking distance, or <c>float.PositiveInfinity</c> if no path is possible</returns>
        public float GetDistanceFromWarp(WarpNode inNode, WorldPoint point)
        {
#if DEBUG
            if (point.Location != this.Location)
            {
                throw new ArgumentException("Point isn't for this location", nameof(point));
            }
            else if (inNode.Target.Location != this.Location)
            {
                throw new ArgumentException("Warp node doesn't target this location", nameof(inNode));
            }
#endif

            return inNode.Target == point ? 0 : this.InteriorDistance(inNode.Target, point);
        }

        /// <summary>
        /// Gets the distance between a pair of inbound and outbound nodes using this location.
        /// </summary>
        /// <param name="inbound">Inbound warp node, i.e., target is this location</param>
        /// <param name="outbound">Outbound warp node, i.e., source is this location</param>
        /// <returns>Walking distance</returns>
        public float GetInteriorDistance(WarpNode inbound, WarpNode outbound)
        {
#if DEBUG
            if (inbound.Target.Location != this.Location)
            {
                throw new ArgumentException("Inbound target not for this location", nameof(inbound));
            }
            else if (outbound.Source.Location != this.Location)
            {
                throw new ArgumentException("Outbound source not for this location", nameof(outbound));
            }
#endif

            return this.InteriorDistance(outbound.Source, inbound.Target);
        }

        /// <summary>
        /// Gets the distance from a warp node sourced in this location, to another warp node targetting another location.
        /// </summary>
        /// <param name="outNodeHere">Outbound warp node, source is this location</param>
        /// <param name="inNodeThere">Inbound warp node, target is some other location</param>
        /// <returns>Walking distance</returns>
        public float GetExteriorDistance(WarpNode outNodeHere, WarpNode inNodeThere)
        {
#if DEBUG
            if (outNodeHere.Source.Location != this.Location || !this.waypointIndex.ContainsKey(outNodeHere.Source))
            {
                throw new ArgumentException("Warp isn't for this location", nameof(outNodeHere));
            }
            else if (inNodeThere.Target.Location == this.Location)
            {
                throw new ArgumentException("Target must be for a different location", nameof(inNodeThere));
            }
#endif
            if (outNodeHere == inNodeThere)
            {
                return 0;
            }

            var tree = this.GetOrCreateExteriorTree(this.waypointIndex[outNodeHere.Source]);
            return tree.DistanceTo(inNodeThere);
        }

        internal void BuildWarpOuts()
        {
            WorldGraph.Monitor.Log($"{this.Name} Build Warps", StardewModdingAPI.LogLevel.Trace);
            this.BuildPlainWarps();
            this.BuildDoorWarps();
            this.BuildBuildingDoorWarps();
            this.BuildTileActionWarps();

            // TODO: collapse adjacent
        }

        internal void BuildWarpIns()
        {
            foreach (var index in this.warpOutNodes.Keys.ToList())
            {
                var warpNode = this.warpOutNodes[index];
                var targetGraph = this.World.GetLocationGraph(warpNode.Target.Location);
                if (targetGraph.FixupDoorWarp(ref warpNode))
                {
                    this.warpOutNodes[index] = warpNode;
                }

                targetGraph.AddWarpIn(warpNode);
            }
        }

        private void BuildPassability()
        {
            var backLayer = this.Location.Resolve.Map.GetLayer("Back");
            var tile = new xTile.Dimensions.Location { X = 0, Y = 0 };
            for (var y = 0; y < this.Height; y++)
            {
                tile.Y = y;
                for (var x = 0; x < this.Width; x++)
                {
                    tile.X = x;
                    this.Passable[y, x] = this.IsPassable(backLayer, tile);
                }
            }
        }

        private int GetWaypointIndex(WorldPoint waypoint)
        {
            if (!this.waypointIndex.TryGetValue(waypoint, out int index))
            {
                index = this.waypoints.Count;
                this.waypoints.Add(waypoint);
                this.waypointIndex.Add(waypoint, index);
            }

            return index;
        }

        private void BuildPlainWarps()
        {
            foreach (var warp in this.Location.Resolve.warps)
            {
                this.AddWarpOut(warp);
            }
        }

        private void BuildDoorWarps()
        {
            var location = this.Location.Resolve;
            foreach (var doorPoint in location.doors.Keys)
            {
                var warp = location.getWarpFromDoor(doorPoint);
                this.AddWarpOut(warp);
            }
        }

        private void BuildBuildingDoorWarps()
        {
            if (this.Location.Resolve is BuildableGameLocation bgl)
            {
                foreach (var building in bgl.buildings.Where(building => building.indoors.Value != null))
                {
                    var point = new Point(building.humanDoor.X + building.tileX.Value, building.humanDoor.Y + building.tileY.Value);
                    var warp = bgl.getWarpFromDoor(point);
                    this.AddWarpOut(warp);
                }
            }
        }

        private void BuildTileActionWarps()
        {
            var layer = this.Location.Resolve.Map.GetLayer("Buildings");
            for (int y = 0; y < this.Height; y++)
            {
                for (int x = 0; x < this.Width; x++)
                {
                    var tiles = layer.Tiles;
                    var tile = tiles[x, y];
                    if (tile != null && tile.Properties.TryGetValue("Action", out var value))
                    {
                        var parts = value.ToString().Split(' ');
                        var action = parts[0];
                        if (WarpActionStrings.Contains(action))
                        {
                            WorldGraph.Monitor.Log($"Found warp action string at {x},{y}@{this.Location.Name}: {value}");
                            if (action == "Warp" || parts.Length >= 4)
                            {
                                var warp = new Warp(x, y, parts[3], int.Parse(parts[1]), int.Parse(parts[2]), false);
                                this.AddWarpOut(warp);
                            }
                            else if (action == "WarpGreenhouse")
                            {
                                var greenhouse = LocationReference.For("Greenhouse");
                                var warp = greenhouse.Resolve.warps[0];
                                this.AddInverseWarpOut(greenhouse, warp, -1);
                            }
                            else if (action == "WarpCommunityCenter")
                            {
                                var center = LocationReference.For("CommunityCenter");
                                foreach (var warp in center.Resolve.warps)
                                {
                                    // search for horizontally aligned warp
                                    if (warp.TargetX == x)
                                    {
                                        this.AddInverseWarpOut(center, warp, y - warp.TargetY);
                                        break;
                                    }
                                }
                            }

                            // TODO: more special warps! "EnterSewer", "LockedDoorWarp", "Mine", "Theater_Entrance", "Warp", "WarpMensLocker", "WarpWomensLocker", "WizardHatch" };
                        }
                    }
                }
            }
        }

        private void AddInverseWarpOut(LocationReference location, Warp warp, int yOffset = 0)
        {
            var target = new WorldPoint(location, warp.X, warp.Y);
            var source = new WorldPoint(LocationReference.For(warp.TargetName), warp.TargetX, warp.TargetY + yOffset);
            this.AddWarpOut(new WarpNode(source, target));
        }

        private bool IsPassable(Layer backLayer, xTile.Dimensions.Location tile)
        {
            var location = this.Location.Resolve;

            // Code essentially copied from Pathoschild.Stardew.DataLayers.Layers.AccessibleLayer.IsAccessible
            if (location.isTilePassable(tile, Game1.viewport))
            {
                return true;
            }

            if (location.doesTileHaveProperty(tile.X, tile.Y, "Passable", "Buildings") != null)
            {
                // allow bridges
                var backTile = backLayer.Tiles[tile];
                return backTile == null || !backTile.TileIndexProperties.TryGetValue("Passable", out var value) || value != "F";
            }

            return false;
        }

        /// <summary>
        /// Gets or creates an interior tree for some waypoint index.
        /// </summary>
        /// <param name="index">Waypoint index</param>
        /// <returns>The tree</returns>
        private InteriorShortestPathTree GetOrCreateInteriorTree(int index)
        {
            if (!this.interiorTrees.TryGetValue(index, out var tree))
            {
                tree = new InteriorShortestPathTree(this, this.waypoints[index]);
                this.interiorTrees.Add(index, tree);
            }

            return tree;
        }

        /// <summary>
        /// Gets or creates an exterior tree for some waypoint index corresponding to some warp out node.
        /// </summary>
        /// <param name="index">Waypoint index</param>
        /// <returns>The tree</returns>
        private ExteriorShortestPathTree GetOrCreateExteriorTree(int index)
        {
            if (!this.exteriorTrees.TryGetValue(index, out var tree))
            {
                tree = new ExteriorShortestPathTree(this, this.warpOutNodes[index]);
                this.exteriorTrees.Add(index, tree);
            }

            return tree;
        }

        private float InteriorDistance(WorldPoint a, WorldPoint b)
        {
#if DEBUG
            if (a.Location != this.Location)
            {
                throw new ArgumentException("Not a point for this location", nameof(a));
            }
            else if (b.Location != this.Location)
            {
                throw new ArgumentException("Not a point for this location", nameof(b));
            }
#endif
            if (a == b)
            {
                return 0;
            }

            bool aIsWaypoint = this.waypointIndex.TryGetValue(a, out int aIndex);
            bool bIsWaypoint = this.waypointIndex.TryGetValue(b, out int bIndex);

#if DEBUG
            if (!aIsWaypoint && !bIsWaypoint)
            {
                throw new InvalidOperationException("Must supply at least one waypoint (both arguments are arbitrary points)");
            }
#endif

            // If b's tree already exists, use that
            if (bIsWaypoint && this.interiorTrees.TryGetValue(bIndex, out var bTree))
            {
                return bTree.DistanceTo(a);
            }

            // Otherwise, just get or create the tree for A
            var aTree = this.GetOrCreateInteriorTree(aIndex);
            return aTree.DistanceTo(b);
        }

        /// <summary>
        /// Attempt to fix up a potentially invalid door warp targetting this location.
        /// </summary>
        /// <param name="warpNode">Node to repair</param>
        /// <returns>If the target was changed</returns>
        private bool FixupDoorWarp(ref WarpNode warpNode)
        {
            var gameLocation = this.Location.Resolve;
            if (this.Name != "FarmHouse" && this.Name.IndexOf("Cabin") != 0 && !gameLocation.isFarmBuildingInterior())
            {
                return false;
            }

            var source = new Vector2(warpNode.Source.X, warpNode.Source.Y);
            float bestDistance = float.PositiveInfinity;
            Warp bestInverseWarp = null;
            foreach (var inverseWarp in gameLocation.warps)
            {
                // Distance squared is fine for comparisons. Discard if farther than 2^2 = 4 tile units.
                float distance = Vector2.DistanceSquared(source, new Vector2(inverseWarp.TargetX, inverseWarp.TargetY));
                if (distance < bestDistance && distance <= 4f)
                {
                    bestDistance = distance;
                    bestInverseWarp = inverseWarp;
                }
            }

            if (bestInverseWarp == null)
            {
                WorldGraph.Monitor.Log($"Unable to fixup door warp {warpNode}", StardewModdingAPI.LogLevel.Warn);
                return false;
            }

            // Re-target the warp node at the best inverse warp source
            var x = MathX.Clamp(bestInverseWarp.X, 0, this.Width - 1);
            var y = MathX.Clamp(bestInverseWarp.Y, 0, this.Height - 1);
            if (x == warpNode.Target.X && y == warpNode.Target.Y)
            {
                return false;
            }

            var oldTarget = warpNode.Target;
            warpNode = new WarpNode(warpNode.Source, new WorldPoint(this.Location, x, y));
            WorldGraph.Monitor.Log($"Fixed up door warp {warpNode} -- old target: {oldTarget}");
            return true;
        }
    }
}
