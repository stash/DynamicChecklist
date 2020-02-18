namespace DynamicChecklist
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewValley;
    using StardewValley.Menus;

    /// <summary>
    /// References a sprite in a <see cref="Texture2D"/> sprite-sheet.
    /// </summary>
    public class GameTexture
    {
        public static readonly GameTexture ArrowDown = new GameTexture(0, 64, 64, 64);
        public static readonly GameTexture ArrowDownSmall = new GameTexture(421, 472, 12, 12);
        public static readonly GameTexture ArrowLeft = new GameTexture(0, 256, 64, 64);
        public static readonly GameTexture ArrowLeftSmall = new GameTexture(352, 494, 12, 12);
        public static readonly GameTexture ArrowRight = new GameTexture(0, 192, 64, 64);
        public static readonly GameTexture ArrowRightSmall = new GameTexture(365, 494, 12, 12);
        public static readonly GameTexture ArrowUp = new GameTexture(64, 64, 64, 64);
        public static readonly GameTexture ArrowUpSmall = new GameTexture(421, 459, 12, 12);
        public static readonly GameTexture BerryBush = new GameTexture(16, 656, 16, 16);
        public static readonly GameTexture CropGenericA = new GameTexture(16, 624, 16, 16);
        public static readonly GameTexture CropGenericB = new GameTexture(80, 624, 16, 16);
        public static readonly GameTexture GenericFish = new GameTexture(0, 640, 16, 16);
        public static readonly GameTexture Handbasket = new GameTexture(32, 624, 16, 16);
        public static readonly GameTexture HandCursor = new GameTexture(32, 0, 16, 16);
        public static readonly GameTexture HandPointingRight = new GameTexture(32, 16, 16, 16, ObjectSpriteSheet);
        public static readonly GameTexture HandSmall = new GameTexture(32, 0, 10, 10);
        public static readonly GameTexture Hay = new GameTexture(160, 112, 16, 16);
        public static readonly GameTexture Heart = new GameTexture(170, 512, 13, 14);
        public static readonly GameTexture HeartSmol = new GameTexture(211, 428, 7, 6);
        public static readonly GameTexture MilkPail = new GameTexture(96, 0, 16, 16, ToolSpriteSheet);
        public static readonly GameTexture Plus = new GameTexture(0, 411, 16, 16);
        public static readonly GameTexture PointingHandCursor = new GameTexture(0, 16, 16, 16);
        public static readonly GameTexture PointingHandSmall = new GameTexture(0, 16, 10, 10);
        public static readonly GameTexture Present = new GameTexture(228, 409, 16, 16);
        public static readonly GameTexture SetButton = new GameTexture(294, 428, 21, 11);
        public static readonly GameTexture Scrollbar = new GameTexture(435, 463, 6, 10);
        public static readonly GameTexture ScrollbarRunner = new GameTexture(403, 383, 6, 6);
        public static readonly GameTexture Shears = new GameTexture(112, 0, 16, 16, ToolSpriteSheet);
        public static readonly GameTexture SpeechBubble = new GameTexture(141, 465, 20, 24);
        public static readonly GameTexture TravellingMerchant = new GameTexture(194, 1414, 16, 16);
        public static readonly GameTexture WateringCan = new GameTexture(49, 224, 16, 16, ToolSpriteSheet);

        public static readonly GameTexture Empty = new GameTexture(Color.Transparent);
        public static readonly GameTexture White = new GameTexture(Color.White);
        public static readonly GameTexture Black = new GameTexture(Color.Black);

        public static readonly GameTexture Sign = new GameTexture(11, 14, "Sign.png");
        public static readonly GameTexture TaskBubble = new GameTexture(20, 24, "TaskBubble.png");

        private bool loaded = false;
        private Func<Texture2D> textureThunk = MainSpriteSheet;
        private Texture2D texture = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameTexture"/> class from the specified sprite-sheet.
        /// </summary>
        /// <param name="x">X coordinate of sprite</param>
        /// <param name="y">Y coordinate of sprite</param>
        /// <param name="width">Width of sprite</param>
        /// <param name="height">Height of sprite</param>
        /// <param name="textureThunk">Function that lazy-loads the sprite-sheet, defaults to the "cursors" sprite-sheet</param>
        protected GameTexture(int x, int y, int width, int height, Func<Texture2D> textureThunk = default)
        {
            this.Src = new Rectangle(x, y, width, height);
            this.textureThunk = (textureThunk == default) ? MainSpriteSheet : textureThunk;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameTexture"/> class from a file.
        /// The texture is loaded from the file specified by <c>filename</c> in the Mod's Resources directory.
        /// </summary>
        /// <param name="width">Width of the image</param>
        /// <param name="height">Height of the image</param>
        /// <param name="filename">Name of file containing the image (PNG with Alpha)</param>
        protected GameTexture(int width, int height, string filename)
        {
            this.Src = new Rectangle(0, 0, width, height);
            this.textureThunk = () =>
            {
                using (var textureStream = new FileStream(Path.Combine(TextureDirectory, filename), FileMode.Open))
                {
                    var fileTex = Texture2D.FromStream(Game1.graphics.GraphicsDevice, textureStream);
                    if (fileTex.Width != this.Src.Width || fileTex.Height != this.Src.Height)
                    {
                        throw new ArgumentException("Image does not match configured height and width");
                    }

                    return fileTex;
                }
            };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameTexture"/> class as a single-pixel texture.
        /// </summary>
        /// <param name="color">The color of the pixel (RGBA)</param>
        protected GameTexture(Color color)
        {
            this.Src = new Rectangle(0, 0, 1, 1);
            this.texture = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            this.texture.SetData(new Color[] { color });
            this.textureThunk = null;
            this.loaded = true;
        }

        public static string TextureDirectory { get; set; }

        public Rectangle Src { get; private set; }

        public Texture2D Tex
        {
            get
            {
                if (!this.loaded)
                {
                    this.texture = this.textureThunk.Invoke();
                    this.textureThunk = null;
                    this.loaded = true;
                }

                return this.texture;
            }
        }

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
        /// Gets the bounds of this texture multiplied by <c>Game1.pixelZoom</c>. Texture assumed to be at <c>(0,0)</c>.
        /// </summary>
        public Rectangle ZoomBounds => new Rectangle(0, 0, this.ZoomWidth, this.ZoomHeight);

        public ClickableTextureComponent AsClickableTextureComponent(string name, int x, int y, int scale = Game1.pixelZoom, bool drawShadow = false)
        {
            var bounds = this.ZoomBounds;
            bounds.X = x;
            bounds.Y = y;
            return new ClickableTextureComponent(name, bounds, string.Empty, string.Empty, this.Tex, this.Src, scale, drawShadow);
        }

        private static Texture2D MainSpriteSheet() => Game1.mouseCursors;

        private static Texture2D ToolSpriteSheet() => Game1.toolSpriteSheet;

        private static Texture2D ObjectSpriteSheet() => Game1.objectSpriteSheet;
    }
}