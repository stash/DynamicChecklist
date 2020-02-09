namespace DynamicChecklist
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DynamicChecklist.ObjectLists;
    using DynamicChecklist.Options;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using StardewModdingAPI;
    using StardewValley;
    using StardewValley.BellsAndWhistles;
    using StardewValley.Menus;

    public class ChecklistMenu : IClickableMenu
    {
        public const int MenuWidth = 13 * Game1.tileSize;
        public const int MenuHeight = 9 * Game1.tileSize;
        private static int selectedTab = 0; // persist between invocations
        private Rectangle menuRect;
        private List<OptionsElement> options = new List<OptionsElement>();
        private List<ClickableComponent> tabs = new List<ClickableComponent>();
        private ModConfig config;

        public ChecklistMenu(ModConfig config)
        {
            this.config = config;

            Game1.playSound("bigSelect");
            this.menuRect = CreateCenteredRectangle(Game1.viewport, MenuWidth, MenuHeight);
            this.initialize(this.menuRect.X, this.menuRect.Y, this.menuRect.Width, this.menuRect.Height, true);

            this.DrawTabs();
            this.DrawContents();
        }

        private enum TabName
        {
            Checklist, Settings
        }

        public static List<ObjectList> ObjectLists { get; set; }

        public static void Open(ModConfig config)
        {
            Game1.activeClickableMenu = new ChecklistMenu(config);
        }

        public override void receiveKeyPress(Keys key)
        {
            foreach (OptionsElement o in this.options)
            {
                o.receiveKeyPress(key);
            }

            base.receiveKeyPress(key);
        }

        public override void draw(SpriteBatch b)
        {
            b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            SpriteText.drawStringWithScrollCenteredAt(b, "Checklist", this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen - Game1.tileSize, string.Empty, 1f, -1, 0, 0.88f, false);
            drawTextureBox(Game1.spriteBatch, this.menuRect.X, this.menuRect.Y, this.menuRect.Width, this.menuRect.Height, Color.White);

            int j = 0;
            foreach (ClickableComponent t in this.tabs)
            {
                var highlight = Color.White * (selectedTab == j ? 1f : 0.7f);
                drawTextureBox(Game1.spriteBatch, t.bounds.X, t.bounds.Y, t.bounds.Width, t.bounds.Height, highlight);
                b.DrawString(Game1.smallFont, t.name, new Vector2(t.bounds.X + 15, t.bounds.Y + 15), Color.Black);
                j++;
            }

            foreach (OptionsElement o in this.options)
            {
                o.draw(b, -1, -1);
            }

            base.draw(b);
            this.drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);
            for (int i = 0; i < this.tabs.Count; i++)
            {
                if (this.tabs[i].bounds.Contains(x, y))
                {
                    selectedTab = i;
                    Game1.activeClickableMenu = new ChecklistMenu(this.config); // Yes, needs to be a new object
                    return;
                }
            }

            // Send click to a menu item
            this.options.FirstOrDefault(o => o.bounds.Contains(x, y))?.receiveLeftClick(x, y);
        }

        public override void leftClickHeld(int x, int y)
        {
            base.leftClickHeld(x, y);
            foreach (OptionsElement o in this.options)
            {
                o.leftClickHeld(x, y);
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            base.releaseLeftClick(x, y);
            foreach (OptionsElement o in this.options)
            {
                o.leftClickReleased(x, y);
            }
        }

        public override void receiveRightClick(int x, int y, bool playSound = false)
        {
            // ignore
        }

        // TODO: Controller support
        public override void setUpForGamePadMode()
        {
            base.setUpForGamePadMode();
        }

        public override void snapCursorToCurrentSnappedComponent()
        {
            base.snapCursorToCurrentSnappedComponent();
        }

        public override void receiveGamePadButton(Buttons b)
        {
            base.receiveGamePadButton(b);
        }

        protected override void customSnapBehavior(int direction, int oldRegion, int oldID)
        {
            base.customSnapBehavior(direction, oldRegion, oldID);
        }

        private static Rectangle CreateCenteredRectangle(xTile.Dimensions.Rectangle v, int width, int height)
        {
            var x = v.Width / 2 - width / 2;
            var y = v.Height / 2 - height / 2;
            return new Rectangle(x, y, width, height);
        }

        private void DrawTabs()
        {
            const int labelWidth = 150;
            int labelX = this.xPositionOnScreen - labelWidth;
            int labelY = this.yPositionOnScreen + 20;
            const int labelSeparation = 80;
            const int labelHeight = 60;
            int i = 0;
            foreach (string s in Enum.GetNames(typeof(TabName)))
            {
                this.tabs.Add(new ClickableComponent(new Rectangle(labelX, labelY + labelSeparation * i++, labelWidth, labelHeight), s));
            }
        }

        private void DrawContents()
        {
            var config = this.config;
            int lineHeight;
            const int margin = 50;
            int marginX = this.menuRect.X + margin;
            int marginY = this.menuRect.Y + margin;
            switch (selectedTab)
            {
                case 0:
                    lineHeight = 50;
                    int j = 0;
                    foreach (ObjectList ol in ObjectLists)
                    {
                        if (ol.ShowInMenu)
                        {
                            var checkbox = new DynamicSelectableCheckbox(ol, marginX, marginY + lineHeight * j);
                            this.options.Add(checkbox);
                            j++;
                        }
                    }

                    break;
                case 1:
                    lineHeight = 65;
                    this.options.Add(new DCOptionsCheckbox("Show All Tasks", 3, config, marginX, marginY + lineHeight * 0));
                    this.options.Add(new DCOptionsCheckbox("Allow Multiple Overlays", 4, config, marginX, marginY + lineHeight * 1));
                    this.options.Add(new DCOptionsCheckbox("Show Arrow to Nearest Task", 5, config, marginX, marginY + lineHeight * 2));
                    this.options.Add(new DCOptionsCheckbox("Show Task Overlay", 6, config, marginX, marginY + lineHeight * 3));
                    this.options.Add(new DCOptionsDropDown("Button Position", 1, config, marginX, marginY + lineHeight * 4));
                    this.options.Add(new DCOptionsInputListener("Open Menu Key", 2, this.menuRect.Width - margin, config, marginX, marginY + lineHeight * 5));
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}