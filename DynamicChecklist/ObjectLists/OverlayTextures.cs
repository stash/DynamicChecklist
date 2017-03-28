using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using System.IO;

namespace DynamicChecklist.ObjectLists
{
    public static class OverlayTextures
    {
        public static Texture2D ArrowRight { get; private set; }
        public static Texture2D Heart { get; private set; }
        public static Texture2D MilkPail { get; private set; }
        public static Texture2D Shears { get; private set; }
        public static Texture2D WateringCan { get; private set; }
        public static Texture2D Crab { get; private set; }
        public static Texture2D SpeechBubble { get; private set; }
        public static Texture2D Sign { get; private set; }

        public static void loadTextures(string directory)
        {
            ArrowRight   = loadTexture("arrowRight.png", directory);
            Heart        = loadTexture("heart.png", directory);
            MilkPail     = loadTexture("milkPail.png", directory);
            Shears       = loadTexture("shears.png", directory);
            WateringCan  = loadTexture("wateringCan.png", directory);
            Crab         = loadTexture("crab.png", directory);
            SpeechBubble = loadTexture("speechBubble.png", directory);
            Sign         = loadTexture("Sign.png", directory);
        }
        private static Texture2D loadTexture(string texName, string directory)
        {
            var textureStream = new FileStream(Path.Combine(directory, "Resources", texName), FileMode.Open);
            var t = Texture2D.FromStream(Game1.graphics.GraphicsDevice, textureStream);
            return t;
        }


    }
}
