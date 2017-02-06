using System;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley;

namespace DynamicChecklist
{
    public class ChecklistMenu : IClickableMenu
    {
        public ChecklistMenu()
        {

        }
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }
        public override void draw(SpriteBatch b)
        {
            var MenuRect = createCenteredRectangle(Game1.viewport, 800, 600);
            IClickableMenu.drawTextureBox(Game1.spriteBatch, MenuRect.X, MenuRect.Y, MenuRect.Width, MenuRect.Height, Color.White);
            base.draw(b);
        }
        public static Rectangle createCenteredRectangle(xTile.Dimensions.Rectangle v, int width, int height)
        {
            var x = v.Width / 2 - width/2;
            var y = v.Height / 2 - height/2;
            return new Rectangle(x, y, width, height);


        }
    }

}


