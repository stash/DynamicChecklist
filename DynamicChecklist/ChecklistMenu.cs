using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley;


namespace DynamicChecklist
{    
    public class ChecklistMenu : IClickableMenu
    {
        public static ObjectCollection objectCollection;
        private Rectangle MenuRect;
        public OptionsElement options;

        private static int iSelectedTab = 0;

        private List<ClickableComponent> tabs = new List<ClickableComponent>();
        private List<string> tabNames = new List<string>{ "Crops", "Crabs" };

        private ClickableComponent selectedTab;

        public ChecklistMenu()
        {

            MenuRect = createCenteredRectangle(Game1.viewport, 800, 600);

            initialize(MenuRect.X, MenuRect.Y, MenuRect.Width, MenuRect.Height, true);

            int i = 0;

            int lblWidth = Game1.tileSize * 3;
            int lblx = (int)(this.xPositionOnScreen - lblWidth);
            int lbly = (int)(this.yPositionOnScreen + Game1.tileSize * 2f);
            int lblSeperation = (int)(Game1.tileSize * 0.9F);
            int lblHeight = 40;

            
            foreach(string s in tabNames)
            {
                this.tabs.Add(new ClickableComponent(new Rectangle(lblx, lbly + lblSeperation * i++, lblWidth, lblHeight), s));
            }

            selectedTab = tabs[iSelectedTab];

            
        }
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }
        public override void draw(SpriteBatch b)
        {
            IClickableMenu.drawTextureBox(Game1.spriteBatch, MenuRect.X, MenuRect.Y, MenuRect.Width, MenuRect.Height, Color.White);
            // Crop menu
            var mouseX = Game1.getMouseX();
            var mouseY = Game1.getMouseY();

            int j = 0;
            foreach(ClickableComponent t in tabs)
            {                
                IClickableMenu.drawTextureBox(Game1.spriteBatch, t.bounds.X, t.bounds.Y, t.bounds.Width, t.bounds.Height, Color.White* (iSelectedTab == j ? 1F : 0.7F));
                b.DrawString(Game1.smallFont, t.name, new Vector2(t.bounds.X+5, t.bounds.Y+5), Color.Black);
                j++;
            }

            switch (selectedTab.name)
            {
                case "Crops":
                    drawCropMenu(b);
                    break;
                case "Crabs":

                    break; 
            }
            
            base.draw(b);
            IClickableMenu.drawHoverText(b, $"{mouseX},{mouseY}", Game1.smallFont);
            drawMouse(b);
        }
        public void drawCropMenu(SpriteBatch b)
        {
            objectCollection.cropList.crops[0].drawInMenu(b, new Vector2(1000, 500), Color.Black, 0, 1, -100);

            int i = 1;
            foreach (CropStruct cropStruct in objectCollection.cropList.cropStructs)
            {
                drawCropLine(b, cropStruct, i);
                i++;
            }
        }
        public void drawCropLine(SpriteBatch b, CropStruct cropStruct,int line)
        {
            var texRow = cropStruct.uniqueCrop.rowInSpriteSheet;

            var lineHeight = 130;

            var destRect = new Rectangle(30 + MenuRect.X, lineHeight*line-32*3 + MenuRect.Y, 16*4, 32*4);
            var sourceRect = new Rectangle(16 * 4, 32 * (texRow-1), 16, 32);

            var posVect = new Vector2(destRect.X, destRect.Y);

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

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            for(int i=0; i<tabs.Count; i++ )
            {
                if (tabs[i].bounds.Contains(x, y))
                {
                    iSelectedTab = i;
                    Game1.playSound("dwop");
                    Game1.activeClickableMenu = new ChecklistMenu();
                }
            }
        }
        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
        }
        public static void Open()
        {
            Game1.activeClickableMenu = new ChecklistMenu();
        }
    }

}


