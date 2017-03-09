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
        private static Texture2D[] textures;

        public static void loadTextures(string directory)
        {
            textures = new Texture2D[8];
            textures[0] = loadTexture("tint-drop.png", directory);
        }
        public static Texture2D getTexture(int which)
        {
            return textures[which];
        }
        private static Texture2D loadTexture(string texName, string directory)
        {
            var textureStream = new FileStream(Path.Combine(directory, "Resources", texName), FileMode.Open);
            var t = Texture2D.FromStream(Game1.graphics.GraphicsDevice, textureStream);
            return t;
        }
    }
}
