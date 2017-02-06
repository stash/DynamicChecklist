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
        private ObjectCollection objectCollection;
        private Rectangle MenuRect;
        public ChecklistMenu(ObjectCollection objectCollection)
        {
            this.objectCollection = objectCollection;
            MenuRect = createCenteredRectangle(Game1.viewport, 800, 600);
            //this.xPositionOnScreen = MenuRect.X;
            //this.yPositionOnScreen = MenuRect.Y;
            //this.width = MenuRect.Width;
            //this.height = MenuRect.Height;

            initialize(MenuRect.X, MenuRect.Y, MenuRect.Width, MenuRect.Height, true);
        }
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }
        public override void draw(SpriteBatch b)
        {
            
            
            IClickableMenu.drawTextureBox(Game1.spriteBatch, MenuRect.X, MenuRect.Y, MenuRect.Width, MenuRect.Height, Color.White);
            // Crop menu
            //IClickableMenu.drawHoverText(b, "asd", Game1.smallFont);          
            objectCollection.cropList.crops[0].drawInMenu(b, new Vector2(100,100), Color.White, 0, 50, -50);
            base.draw(b);
            drawMouse(b);
        }
        public static Rectangle createCenteredRectangle(xTile.Dimensions.Rectangle v, int width, int height)
        {
            var x = v.Width / 2 - width/2;
            var y = v.Height / 2 - height/2;
            return new Rectangle(x, y, width, height);

        }
    }

}


