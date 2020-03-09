namespace DynamicChecklist.Graph2
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Buildings;
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

        public LocationGraph(GameLocation location, WorldGraph world)
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
        }

        public WorldGraph World { get; private set; }

        public GameLocation Location { get; private set; }

        public int Height { get; private set; }

        public int Width { get; private set; }

        public bool[,] Passable { get; private set; } // y, x

        public int NumPassable { get; private set; } = 0;

        public int OutDegree => this.warpOutNodes.Count;

        public int InDegree { get; private set; } = 0;

        public string Name => this.Location.NameOrUniqueName;

        public bool IsDisconnected => this.OutDegree == 0 && this.InDegree == 0;

        /// <summary>Gets a collection of outbound <see cref="WarpNode"/>s</summary>
        public ICollection<WarpNode> WarpOutNodes => this.warpOutNodes.Values;

        public WarpNode FirstWarpOutNode => this.warpOutNodes.Values.FirstOrDefault();

        /// <summary>Gets an enumeration of inbound <see cref="WarpNode"/>s</summary>
        public IEnumerable<WarpNode> WarpInNodes => this.warpInNodes.SelectMany(pair => pair.Value.AsEnumerable());

        /// <summary>
        /// Creates and adds new <see cref="WarpNode"/> for this location, corresponding to <paramref name="warp"/>.
        /// </summary>
        /// <remarks>Won't be added if out of tile coordinates range for the location (slightly out of range will be clamped).</remarks>
        /// <param name="warp">The warp to add</param>
        /// <returns>True if the warp was added (can't be duplicated, must be in range), false otherwise</returns>
        /// <seealso cref="AddWarpOut(WarpNode)"/>
        public bool AddWarpOut(Warp warp)
        {
            if (WorldPoint.InRange(this.Location, warp.X, warp.Y) &&
                WorldPoint.InRange(Game1.getLocationFromName(warp.TargetName), warp.TargetX, warp.TargetY))
            {
                return this.AddWarpOut(new WarpNode(warp, this.Location));
            }

            WorldGraph.Monitor.Log($"Warp out of range {warp.X},{warp.Y}@{this.Name} -> {warp.TargetX},{warp.TargetY}@{warp.TargetName}", StardewModdingAPI.LogLevel.Warn);
            return false;
        }

        /// <summary>
        /// Add an outbound <see cref="WarpNode"> to this location.
        /// Triggers the OnWarpOutAdded event if it was added.
        /// Duplicate warps cannot be added.
        /// </summary>
        /// <param name="node">A node with this location as the Source</param>
        /// <returns>True if newly added, false otherwise</returns>
        /// <seealso cref="AddWarpOut(Warp)"/>
        public bool AddWarpOut(WarpNode node)
        {
#if DEBUG
            if (node.Source.Location != this.Location)
            {
                throw new ArgumentException(nameof(WarpNode) + " does not belong to this location", nameof(node));
            }
#endif

            var index = this.GetWaypointIndex(node.Source);
            if (!this.warpOutNodes.ContainsKey(index))
            {
                this.warpOutNodes.Add(index, node);
                this.World.GetLocationGraph(node.Target.Location).AddWarpIn(node);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Registers an inbound node to this location.
        /// </summary>
        /// <param name="node">A node with this location as the Target</param>
        /// <returns>True if the target point for the node was newly added, false otherwise</returns>
        public bool AddWarpIn(WarpNode node)
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

            return this.InteriorDistanceTo(outNode.Source, point);
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

            return this.InteriorDistanceTo(inNode.Target, point);
        }

        /// <summary>
        /// Gets the distance between the incoming target position and the outgoing source position.
        /// </summary>
        /// <param name="inNode">Inbound warp node, target is this location</param>
        /// <param name="outNode">Outbound warp node, source is this location</param>
        /// <returns>Walking distance</returns>
        public float GetInterWarpDistance(WarpNode inNode, WarpNode outNode)
        {
#if DEBUG
            if (inNode.Target.Location != this.Location)
            {
                throw new ArgumentException("Inbound target not for this location", nameof(inNode));
            }
            else if (outNode.Source.Location != this.Location)
            {
                throw new ArgumentException("Outbound source not for this location", nameof(outNode));
            }
#endif
            return this.InteriorDistanceTo(outNode.Source, inNode.Target);
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
            if (outNodeHere.Source.Location != this.Location)
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

            var index = this.waypointIndex[outNodeHere.Source];
            var tree = this.GetExteriorTree(index);
            return tree.DistanceTo(inNodeThere);
        }

        public void Build()
        {
            this.BuildPassability();

            this.BuildPlainWarps();
            this.BuildDoorWarps();
            this.BuildBuildingDoorWarps();
            this.BuildTileActionWarps();
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
            foreach (var warp in this.Location.warps)
            {
                this.AddWarpOut(warp);
            }
        }

        private void BuildDoorWarps()
        {
            var location = this.Location;
            foreach (var doorPoint in location.doors.Keys)
            {
                this.AddWarpOut(location.getWarpFromDoor(doorPoint));
            }
        }

        private void BuildBuildingDoorWarps()
        {
            if (this.Location is BuildableGameLocation bgl)
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
            var layer = this.Location.Map.GetLayer("Buildings");
            var viewportSize = Game1.viewport.Size;
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
                            WorldGraph.Monitor.Log($"Found warp action string at {x},{y}@{this.Location.NameOrUniqueName}: {value}");
                            if (action == "Warp" || parts.Length >= 4)
                            {
                                var warp = new Warp(x, y, parts[3], int.Parse(parts[1]), int.Parse(parts[2]), false);
                                this.AddWarpOut(warp);
                            }
                            else if (action == "WarpGreenhouse")
                            {
                                var warp = Game1.getLocationFromName("Greenhouse").warps[0];
                                this.AddInverseWarpOut(Game1.getLocationFromName("Greenhouse"), warp, -1);
                            }
                            else if (action == "WarpCommunityCenter")
                            {
                                var center = Game1.getLocationFromName("CommunityCenter");
                                foreach (var warp in center.warps)
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

        private void AddInverseWarpOut(GameLocation location, Warp warp, int yOffset = 0)
        {
            var target = new WorldPoint(location, warp.X, warp.Y);
            var source = new WorldPoint(Game1.getLocationFromName(warp.TargetName), warp.TargetX, warp.TargetY + yOffset);
            this.AddWarpOut(new WarpNode(source, target));
        }

        private void BuildPassability()
        {
            this.NumPassable = 0;

            var backLayer = this.Location.Map.GetLayer("Back");
            var tile = new xTile.Dimensions.Location { X = 0, Y = 0 };
            for (var y = 0; y < this.Height; y++)
            {
                tile.Y = y;
                for (var x = 0; x < this.Width; x++)
                {
                    tile.X = x;
                    if (this.IsPassable(backLayer, tile))
                    {
                        this.Passable[y, x] = true;
                        this.NumPassable++;
                    }
                }
            }
        }

        private bool IsPassable(Layer backLayer, Location tile)
        {
            // Code essentially copied from Pathoschild.Stardew.DataLayers.Layers.AccessibleLayer.IsAccessible
            if (this.Location.isTilePassable(tile, Game1.viewport))
            {
                return true;
            }

            if (this.Location.doesTileHaveProperty(tile.X, tile.Y, "Passable", "Buildings") != null)
            {
                // allow bridges
                var backTile = backLayer.Tiles[tile];
                return backTile == null || !backTile.TileIndexProperties.TryGetValue("Passable", out var value) || value != "F";
            }

            return false;
        }

        private InteriorShortestPathTree GetInteriorTree(int index)
        {
            if (!this.interiorTrees.TryGetValue(index, out var tree))
            {
                tree = new InteriorShortestPathTree(this, this.waypoints[index]);
                this.interiorTrees.Add(index, tree);
            }

            return tree;
        }

        private ExteriorShortestPathTree GetExteriorTree(int index)
        {
            if (!this.exteriorTrees.TryGetValue(index, out var tree))
            {
                tree = new ExteriorShortestPathTree(this, this.warpOutNodes[index]);
                this.exteriorTrees.Add(index, tree);
            }

            return tree;
        }

        private float InteriorDistanceTo(WorldPoint waypoint, WorldPoint point)
        {
#if DEBUG
            if (waypoint.Location != this.Location)
            {
                throw new ArgumentException("Not for this location", nameof(waypoint));
            }
#endif
            var index = this.waypointIndex[waypoint]; // will intentionally throw in non-Debug
            var tree = this.GetInteriorTree(index);
            return tree.DistanceTo(point);
        }
    }
}
