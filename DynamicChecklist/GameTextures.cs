namespace DynamicChecklist
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewValley;
    using StardewValley.Menus;

    public static class GameTextures
    {
        public static readonly Entry UpArrow = new Entry { Tex = Game1.mouseCursors, Src = new Rectangle(421, 459, 11, 12) };
        public static readonly Entry DownArrow = new Entry { Tex = Game1.mouseCursors, Src = new Rectangle(421, 472, 11, 12) };
        public static readonly Entry Scrollbar = new Entry { Tex = Game1.mouseCursors, Src = new Rectangle(435, 463, 6, 10) };
        public static readonly Entry ScrollbarRunner = new Entry { Tex = Game1.mouseCursors, Src = new Rectangle(403, 383, 6, 6) };
        public static readonly Entry SetButton = new Entry { Tex = Game1.mouseCursors, Src = new Rectangle(294, 428, 21, 11) };

        public struct Entry
        {
            public Texture2D Tex;
            public Rectangle Src;

            public int X => this.Src.X;

            public int Y => this.Src.Y;

            public int Width => this.Src.Width;

            /// <summary>
            /// Gets the width of this texture multiplied by <c>Game1.pixelZoom</c>
            /// </summary>
            public int ZoomWidth => this.Src.Width * Game1.pixelZoom;

            public int Height => this.Src.Height;

            /// <summary>
            /// Gets the height of this texture multiplied by <c>Game1.pixelZoom</c>
            /// </summary>
            public int ZoomHeight => this.Src.Height * Game1.pixelZoom;

            /// <summary>
            /// Gets the bounds of this texture multiplied by <c>Game1.pixelZoom</c>
            /// </summary>
            public Rectangle ZoomBounds => new Rectangle(0, 0, this.ZoomWidth, this.ZoomHeight);

            public ClickableTextureComponent AsClickableTextureComponent(string name, int x, int y, int scale = Game1.pixelZoom, bool drawShadow = false)
            {
                var bounds = this.ZoomBounds;
                bounds.X = x;
                bounds.Y = y;
                return new ClickableTextureComponent(name, bounds, string.Empty, string.Empty, this.Tex, this.Src, scale, drawShadow);
            }
        }
    }
}