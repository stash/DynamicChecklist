namespace DynamicChecklist
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Xna.Framework;
    using StardewModdingAPI;
    using StardewModdingAPI.Events;
    using StardewValley;

    /// <summary>
    /// Provides a stable, validated, weak reference to a <see cref="GameLocation"/>.
    /// </summary>
    /// <remarks>
    /// Keeping a standard C# pointer to some <see cref="GameLocation"/> doesn't work under multiplayer.
    /// Use <see cref="Resolve"/> or the implicit cast to get the location via <see cref="Game1.getLocationFromName(string)"/>
    /// </remarks>
    [DebuggerDisplay("{Name}")]
    public class LocationReference : IEquatable<LocationReference>, IComparable<LocationReference>
    {
        private static Dictionary<string, LocationReference> allReferences;
        private static IModHelper helper;

        private WeakReference<GameLocation> cached = null;

        private LocationReference(GameLocation location)
        {
            this.Name = location.NameOrUniqueName;
            this.cached = new WeakReference<GameLocation>(location);
        }

        public string Name { get; private set; }

        public GameLocation Resolve
        {
            get
            {
                if (this.cached == null || !this.cached.TryGetTarget(out var value))
                {
                    value = Game1.getLocationFromName(this.Name);
                    if (this.cached == null)
                    {
                        this.cached = new WeakReference<GameLocation>(value);
                    }
                    else
                    {
                        this.cached.SetTarget(value);
                    }
                }

                return value;
            }
        }

        public static implicit operator GameLocation(LocationReference reference) => reference.Resolve;

        public static implicit operator LocationReference(GameLocation location) => For(location);

        public static bool operator ==(LocationReference left, LocationReference right)
        {
            return object.ReferenceEquals(left, right) || ((left is object) && left.Equals(right));
        }

        public static bool operator !=(LocationReference left, LocationReference right) => !(left == right);

        public static bool operator ==(LocationReference left, GameLocation right)
        {
            return (left is object) && (right is object) && left.Name.Equals(right.NameOrUniqueName);
        }

        public static bool operator !=(LocationReference left, GameLocation right) => !(left == right);

        /// <summary>
        /// Create a reference to a location.
        /// </summary>
        /// <param name="location">Actual location, which must not be <c>null</c></param>
        /// <exception cref="ArgumentNullException">When <paramref name="location"/> is null</exception>
        /// <returns>The reference</returns>
        public static LocationReference For(GameLocation location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            var name = location.NameOrUniqueName;
            if (!allReferences.TryGetValue(name, out var reference))
            {
                reference = new LocationReference(location);
                allReferences.Add(name, reference);
            }

            return reference;
        }

        /// <summary>
        /// Create a reference to a location by looking it up by name.
        /// </summary>
        /// <param name="nameOrUniqueName">The unique name of the location</param>
        /// <exception cref="ArgumentNullException">When <paramref name="nameOrUniqueName"/> is null</exception>
        /// <exception cref="ArgumentException">When <paramref name="nameOrUniqueName"/> cannot be resolved to a location</exception>
        /// <returns>The reference</returns>
        public static LocationReference For(string nameOrUniqueName)
        {
            if (nameOrUniqueName == null)
            {
                throw new ArgumentNullException(nameof(nameOrUniqueName));
            }
            else if (nameOrUniqueName == "null")
            {
                throw new ArgumentException("Location name was stringified \"null\"", nameof(nameOrUniqueName));
            }

            var location = Game1.getLocationFromName(nameOrUniqueName);
            if (location == null)
            {
                throw new ArgumentException("Invalid location name", nameof(nameOrUniqueName));
            }

            return For(location);
        }

        /// <summary>
        /// Called from mod <c>Entry</c> to set up the static reference tables.
        /// </summary>
        /// <param name="newHelper">The current helper</param>
        public static void Setup(IModHelper newHelper)
        {
            if (allReferences == null)
            {
                // Mod Entry called for first time
                allReferences = new Dictionary<string, LocationReference>();
            }
            else
            {
                // Mod Entry called again. Clear old listeners and caches.
                helper.Events.GameLoop.UpdateTicking -= GameLoop_UpdateTicking;
                ClearAll();
            }

            helper = newHelper;

            // Aggressively invalidate references
            // TODO: figure out a more reactive solution (like watching for a netref to update or something)
            helper.Events.GameLoop.UpdateTicking += GameLoop_UpdateTicking;
        }

        /// <summary>
        /// Invalidate all references.
        /// </summary>
        public static void ClearAll()
        {
            foreach (var reference in allReferences.Values)
            {
                reference.Clear();
            }
        }

        /// <summary>
        /// Invalidates the weak reference to the <see cref="GameLocation"/>.
        /// </summary>
        public void Clear()
        {
            this.cached = null;
        }

        public int CompareTo(LocationReference other)
        {
            return string.Compare(this.Name, (other is object) ? other.Name : null);
        }

        public override bool Equals(object obj) => this.Equals(obj as LocationReference);

        public bool Equals(LocationReference other) => (other is object) && this.Name == other.Name;

        public bool Equals<T>(T other)
            where T : GameLocation
        {
            return (other is object) && this.Name == other.NameOrUniqueName;
        }

        public override int GetHashCode() => 539060726 + this.Name.GetHashCode();

        private static void GameLoop_UpdateTicking(object sender, UpdateTickingEventArgs e)
        {
            ClearAll();
        }
    }
}
