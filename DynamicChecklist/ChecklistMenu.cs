using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace DynamicChecklist
{    
    public class ChecklistMenu : IClickableMenu
    {
        public static ObjectCollection objectCollection;
        private Rectangle MenuRect;
        public List<OptionsElement> options = new List<OptionsElement>();

        private static int iSelectedTab = 0;

        private List<ClickableComponent> tabs = new List<ClickableComponent>();
        private List<string> tabNames = new List<string>{"Checklist", "Crops", "Crabs" };

        private ClickableComponent selectedTab;

        public ChecklistMenu()
        {

            MenuRect = createCenteredRectangle(Game1.viewport, 800, 600);
            initialize(MenuRect.X, MenuRect.Y, MenuRect.Width, MenuRect.Height, true);

            var cl = Game1.currentLocation;

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
            int lineHeight = 60;
            switch (selectedTab.name)
            {
                case "Checklist":
                    if (objectCollection.cropList.crops.Count > 0)
                    {
                        var checkbox = new DynamicSelectableCheckbox("Watered Crops", 2, objectCollection);
                        checkbox.bounds = new Rectangle(MenuRect.X + 50, MenuRect.Y + 50 + lineHeight * 0, 100, 50);
                        options.Add(checkbox);
                    }
                    if (objectCollection.crabTrapList.nTotal > 0)
                    {
                        var checkbox = new DynamicSelectableCheckbox("Baited Crab Pots", 1, objectCollection);
                        checkbox.bounds = new Rectangle(MenuRect.X + 50, MenuRect.Y + 50 + lineHeight * 1, 100, 50);
                        options.Add(checkbox);
                    }

                    if (objectCollection.coopList.coops.Count > 0)
                    {
                        var checkbox = new DynamicSelectableCheckbox("Collected Eggs", 3, objectCollection);
                        checkbox.bounds = new Rectangle(MenuRect.X + 50, MenuRect.Y + 50 + lineHeight * 2, 100, 50);
                        options.Add(checkbox);

                        checkbox = new DynamicSelectableCheckbox("Milked Cows/Goats", 4, objectCollection);
                        checkbox.bounds = new Rectangle(MenuRect.X + 50, MenuRect.Y + 50 + lineHeight * 3, 100, 50);
                        options.Add(checkbox);

                        checkbox = new DynamicSelectableCheckbox("Petted Animals", 5, objectCollection);
                        checkbox.bounds = new Rectangle(MenuRect.X + 50, MenuRect.Y + 50 + lineHeight * 4, 100, 50);
                        options.Add(checkbox);

                        checkbox = new DynamicSelectableCheckbox("Provided Animals Hay", 6, objectCollection);
                        checkbox.bounds = new Rectangle(MenuRect.X + 50, MenuRect.Y + 50 + lineHeight * 5, 100, 50);
                        options.Add(checkbox);
                    }

                    break;
                case "Crops":
                    break;
                case "Crabs":
                    break;
            }


        }
        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }
        public override void receiveKeyPress(Keys key)
        {
            foreach(OptionsElement o in options)
            {
                // TODO Find out which option is selected
                o.receiveKeyPress(key);
            }
            base.receiveKeyPress(key);
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
                case "Checklist":
                    //drawChecklist(b);
                    //options.Add(new AddToChecklistElement(300));
                    break;
                case "Crops":
                    drawCropMenu(b);
                    break;
                case "Crabs":
                    drawCrabMenu(b);
                    break;
                case "Coops":
                    drawCoopMenu(b);
                    break;
            }
            foreach(OptionsElement o in options)
            {
                o.draw(b, -1, -1);
            }
            base.draw(b);
            IClickableMenu.drawHoverText(b, $"{mouseX},{mouseY}", Game1.smallFont);
            drawMouse(b);
        }
        private void drawCoopMenu(SpriteBatch b)
        {

        }
        private void drawChecklist(SpriteBatch b)
        {

            //var addNewItemToList = new AddToChecklistElement(300);
            //addNewItemToList.draw(b, 1, 1);

            foreach (OptionsElement o in options)
            {
                o.draw(b, 1, 1);
            }

        }
        private void drawCrabMenu(SpriteBatch b)
        {
            var crabTrapList = objectCollection.crabTrapList;            
            drawCrabLine(b, "All", crabTrapList.nNeedAction, crabTrapList.nTotal, 1);
            int i = 2;
            foreach (CrabTrapList.CrabTrapsLoc ctl in crabTrapList.crabTrapsLoc)
            {
                if (ctl.nTotal > 0)
                {
                    drawCrabLine(b, ctl.loc.name, ctl.nNeedAction, ctl.nTotal, i);
                    i++;
                }

            }

        }
        private void drawCrabLine(SpriteBatch b, string locName, int nNeedAction, int nTotal, int line)
        {
            var lineHeight = 80;

            var a = Game1.cropSpriteSheet;
            b.DrawString(Game1.smallFont, $"Location: {locName}, Total: {nTotal}, Baited: {nTotal - nNeedAction}", new Vector2(100 + MenuRect.X, MenuRect.Y + lineHeight * line - 30), Color.Black);

            this.drawHorizontalPartition(b, this.MenuRect.Y + lineHeight * line, true);
        }
        private void drawCropMenu(SpriteBatch b)
        {
            objectCollection.cropList.crops[0].drawInMenu(b, new Vector2(1000, 500), Color.Black, 0, 1, -100);

            int i = 1;
            foreach (CropStruct cropStruct in objectCollection.cropList.cropStructs)
            {
                drawCropLine(b, cropStruct, i);
                i++;
            }
        }
        private void drawCropLine(SpriteBatch b, CropStruct cropStruct,int line)
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
            b.DrawString(Game1.smallFont, $"Total: {cropStruct.count}, Watered: {cropStruct.count- cropStruct.countUnwatered}", new Vector2(100 + MenuRect.X, MenuRect.Y + lineHeight*line - 30), Color.Black );
            
            this.drawHorizontalPartition(b, this.MenuRect.Y + lineHeight * line, true);
        }
        private static Rectangle createCenteredRectangle(xTile.Dimensions.Rectangle v, int width, int height)
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
            foreach(OptionsElement o in options)
            {
                if (o.bounds.Contains(x, y))
                {
                    o.receiveLeftClick(x, y);
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


