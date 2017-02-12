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
        public OptionsElement options;

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
            this.drawBackground(b);
            IClickableMenu.drawTextureBox(Game1.spriteBatch, MenuRect.X, MenuRect.Y, MenuRect.Width, MenuRect.Height, Color.White);
            // Crop menu
            var mouseX = Game1.getMouseX();
            var mouseY = Game1.getMouseY();
                     
            objectCollection.cropList.crops[0].drawInMenu(b, new Vector2(1000,500), Color.Black, 0, 1, -100);
            //objectCollection.cropList.crops[0].drawInMenu(b, new Vector2(MenuRect.X, MenuRect.Y), Color.Black, 0, 1, 0);
            //objectCollection.cropList.crops[10].draw(b, new Vector2(MenuRect.X, MenuRect.Y), Color.Black, 0);
            
            //this.drawHorizontalPartition(b, MenuRect.Y+50, true);
            int i = 1;
            foreach(CropStruct cropStruct in objectCollection.cropList.cropStructs)
            {
                drawCropLine(b, cropStruct, i);
                i++;
            }           
            base.draw(b);
            IClickableMenu.drawHoverText(b, $"{mouseX},{mouseY}", Game1.smallFont);
            drawMouse(b);
        }
        public void drawCropLine(SpriteBatch b, CropStruct cropStruct,int line)
        {
            var texRow = cropStruct.uniqueCrop.rowInSpriteSheet;

            var lineHeight = 130;

            var destRect = new Rectangle(30 + MenuRect.X, lineHeight*line-32*3 + MenuRect.Y, 16*4, 32*4);
            var sourceRect = new Rectangle(16 * 4, 32 * (texRow-1), 16, 32);
            //b.Draw(objectCollection.cropSpriteSheet, destRect, sourceRect, Color.White);

            var posVect = new Vector2(destRect.X, destRect.Y);
            //cropStruct.uniqueCrop.drawInMenu(b, posVect, Color.White, 0, 1, 0);
            //posVect.X -= -Game1.viewport.X; // wrong, dont need viewport
            //posVect.Y -= Game1.viewport.Y;
            //cropStruct.uniqueCrop.drawInMenu(b, posVect, Color.White, 0, 1, 0);

            //cropStruct.uniqueCrop.drawInMenu(b, posVect, Color.White, 1.6f, 1, 0);
            var oldPhaseToShow = cropStruct.uniqueCrop.phaseToShow;
            cropStruct.uniqueCrop.phaseToShow = 100;
            b.Draw(Game1.cropSpriteSheet, posVect, new Rectangle?(getSourceRect(cropStruct.uniqueCrop)), Color.White, 0, Vector2.Zero,4, SpriteEffects.None, 0);
            cropStruct.uniqueCrop.phaseToShow = oldPhaseToShow;
           // TODO use helper to get crop.getSourceRect(0) method or figure out drawInMenu method
           // Scaling or rotating changes the position a lot
            var a = Game1.cropSpriteSheet;
            b.DrawString(Game1.smallFont, $"Total: {cropStruct.count}, Watered: {cropStruct.count}", new Vector2(100 + MenuRect.X, MenuRect.Y + lineHeight*line - 30), Color.Black );
            
            this.drawHorizontalPartition(b, this.MenuRect.Y + lineHeight * line, true);
        }
        public static Rectangle createCenteredRectangle(xTile.Dimensions.Rectangle v, int width, int height)
        {
            var x = v.Width / 2 - width/2;
            var y = v.Height / 2 - height/2;
            return new Rectangle(x, y, width, height);

        }
        private Rectangle getSourceRect(Crop crop)
        {
            if (crop.dead)
            {
                return new Rectangle(192 + 0 % 4 * 16, 384, 16, 32);
            }
            return new Rectangle(Math.Min(240, (crop.fullyGrown ? ((crop.dayOfCurrentPhase <= 0) ? 6 : 7) : (((crop.phaseToShow != -1) ? crop.phaseToShow : crop.currentPhase) + ((((crop.phaseToShow != -1) ? crop.phaseToShow : crop.currentPhase) == 0 && 0 % 2 == 0) ? -1 : 0) + 1)) * 16 + ((crop.rowInSpriteSheet % 2 != 0) ? 128 : 0)), crop.rowInSpriteSheet / 2 * 16 * 2, 16, 32);
        }
    }

}


