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
        public static Texture2D Droplet { get; private set; }        

        public static void loadTextures(string directory)
        {
            ArrowRight = loadTexture("arrowRight.png", directory);
            Droplet    = loadTexture("tint-drop.png", directory);
             
        }
        private static Texture2D loadTexture(string texName, string directory)
        {
            var textureStream = new FileStream(Path.Combine(directory, "Resources", texName), FileMode.Open);
            var t = Texture2D.FromStream(Game1.graphics.GraphicsDevice, textureStream);
            return t;
        }


    }
}
