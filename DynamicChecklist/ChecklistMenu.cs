using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley;
using System.Linq;
using Microsoft.Xna.Framework.Input;
using DynamicChecklist.ObjectLists;
using StardewValley.BellsAndWhistles;

namespace DynamicChecklist
{    
    public class ChecklistMenu : IClickableMenu
    {
        public static List<ObjectList> objectLists;

        private Rectangle MenuRect;
        public List<OptionsElement> options = new List<OptionsElement>();

        private static int iSelectedTab = 0;

        private List<ClickableComponent> tabs = new List<ClickableComponent>();
        private List<string> tabNames = new List<string>{"Checklist"};

        private ClickableComponent selectedTab;

        public ChecklistMenu()
        {
            Game1.playSound("bigSelect");
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
                tabs.Add(new ClickableComponent(new Rectangle(lblx, lbly + lblSeperation * i++, lblWidth, lblHeight), s));
            }

            selectedTab = tabs[iSelectedTab];
            int lineHeight = 60;
            switch (selectedTab.name)
            {
                case "Checklist":
                    int j = 0;
                    foreach(ObjectList ol in objectLists)
                    {
                        if (ol.ShowInMenu )
                        {
                            var checkbox = new DynamicSelectableCheckbox(ol);
                            checkbox.bounds = new Rectangle(MenuRect.X + 50, MenuRect.Y + 50 + lineHeight * j, 100, 50);
                            options.Add(checkbox);
                            j++;
                        }                                           
                    }
                    break;
                default:
                    throw (new NotImplementedException());
            }


        }

        public override void receiveKeyPress(Keys key)
        {
            foreach(OptionsElement o in options)
            {
                o.receiveKeyPress(key);
            }
            base.receiveKeyPress(key);
        }
        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            SpriteText.drawStringWithScrollCenteredAt(b, "Checklist", this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen - Game1.tileSize, "", 1f, -1, 0, 0.88f, false);
            drawTextureBox(Game1.spriteBatch, MenuRect.X, MenuRect.Y, MenuRect.Width, MenuRect.Height, Color.White);
            var mouseX = Game1.getMouseX();
            var mouseY = Game1.getMouseY();

            int j = 0;
            foreach(ClickableComponent t in tabs)
            {                
                //drawTextureBox(Game1.spriteBatch, t.bounds.X, t.bounds.Y, t.bounds.Width, t.bounds.Height, Color.White* (iSelectedTab == j ? 1F : 0.7F));
                //b.DrawString(Game1.smallFont, t.name, new Vector2(t.bounds.X+5, t.bounds.Y+5), Color.Black);
                j++;
            }
            foreach(OptionsElement o in options)
            {
                o.draw(b, -1, -1);
            }
            base.draw(b);
            drawMouse(b);
        }

        private static Rectangle createCenteredRectangle(xTile.Dimensions.Rectangle v, int width, int height)
        {
            var x = v.Width / 2 - width/2;
            var y = v.Height / 2 - height/2;
            return new Rectangle(x, y, width, height);

        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            for(int i=0; i<tabs.Count; i++ )
            {
                if (tabs[i].bounds.Contains(x, y))
                {
                    iSelectedTab = i;
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
        public static void Open()
        {
            Game1.activeClickableMenu = new ChecklistMenu();
        }

        public override void receiveRightClick(int x, int y, bool playSound = false)
        {

        }
    }

}


