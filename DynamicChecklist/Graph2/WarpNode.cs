namespace DynamicChecklist.Graph2
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using Microsoft.Xna.Framework;
    using Priority_Queue;
    using StardewValley;

    /// <summary>
    /// A logical extension of the game <c>Warp</c> object
    /// </summary>
    [DebuggerDisplay("{ToString()}")]
    internal class WarpNode : FastPriorityQueueNode, IEquatable<WarpNode>
    {
        public WarpNode(Warp warp, LocationReference location)
            : base()
        {
            this.Priority = float.PositiveInfinity;
            this.Source = new WorldPoint(location, warp.X, warp.Y);
            this.Target = new WorldPoint(LocationReference.For(warp.TargetName), warp.TargetX, warp.TargetY);
        }

        public WarpNode(LocationReference sourceLocation, int sourceX, int sourceY, LocationReference targetLocation, int targetX, int targetY)
            : base()
        {
            this.Priority = float.PositiveInfinity;
            this.Source = new WorldPoint(sourceLocation, sourceX, sourceY);
            this.Target = new WorldPoint(targetLocation, targetX, targetY);
        }

        public WarpNode(WorldPoint source, WorldPoint target)
            : base()
        {
            this.Priority = float.PositiveInfinity;
            this.Source = source;
            this.Target = target;
        }

        public WorldPoint Source { get; private set; }

        public WorldPoint Target { get; private set; }

        public static bool operator ==(WarpNode left, WarpNode right) => object.ReferenceEquals(left, right) || ((left is object) && left.Equals(right));

        public static bool operator !=(WarpNode left, WarpNode right) => !(left == right);

        public static WarpNode ExtractSourceCentroid(LinkedList<WarpNode> cluster)
        {
            float sumX = cluster.Sum(warp => warp.Source.X);
            float sumY = cluster.Sum(warp => warp.Source.Y);
            float factor = Game1.tileSize / (float)cluster.Count;
            var centroid = new Vector2(factor * sumX, factor * sumY);
            float nearestSqrDist = float.PositiveInfinity;
            WarpNode nearest = null;
            foreach (var warp in cluster)
            {
                var sqrDist = Vector2.DistanceSquared(centroid, warp.Source);
                if (sqrDist < nearestSqrDist)
                {
                    nearest = warp;
                    nearestSqrDist = sqrDist;
                }
            }

            cluster.Remove(nearest);
            return nearest;
        }

        public override bool Equals(object obj) => this.Equals(obj as WarpNode);

        public bool Equals(WarpNode node)
        {
            return (node is object) &&
                   this.Source.Equals(node.Source) &&
                   this.Target.Equals(node.Target);
        }

        public override int GetHashCode()
        {
            int hashCode = -1031959520;
            hashCode = hashCode * -1521134295 + this.Source.GetHashCode();
            hashCode = hashCode * -1521134295 + this.Target.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(this.Source.ToString());
            sb.Append(" -> ");
            sb.Append(this.Target.ToString());
            return sb.ToString();
        }

        public string ToShortString()
        {
            var sb = new StringBuilder();
            sb.Append(this.Source.ToShortString());
            sb.Append(" -> ");
            sb.Append(this.Target.ToShortString());
            return sb.ToString();
        }

        public string ToCoordString()
        {
            var sb = new StringBuilder();
            sb.Append(this.Source.ToCoordString());
            sb.Append(" -> ");
            sb.Append(this.Target.ToCoordString());
            return sb.ToString();
        }
    }
}