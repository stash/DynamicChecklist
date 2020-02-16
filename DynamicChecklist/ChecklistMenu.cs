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
    using StardewValley;
    using StardewValley.BellsAndWhistles;
    using StardewValley.Menus;

    public class ChecklistMenu : IClickableMenu
    {
        public const int MenuWidth = 13 * Game1.tileSize;
        public const int MenuHeight = 9 * Game1.tileSize;
        public const int Margin = Game1.tileSize / 2;

        private static int selectedTab = 0; // persist between invocations
        private List<OptionsElement> options = new List<OptionsElement>();
        private List<ClickableComponent> optionSlots = new List<ClickableComponent>();
        private List<ClickableComponent> tabs = new List<ClickableComponent>();
        private ModConfig config;
        private ClickableTextureComponent upArrow;
        private ClickableTextureComponent downArrow;
        private ClickableTextureComponent scrollbar;
        private Rectangle scrollbarRunner;
        private int scrollOffset;
        private int optionsSlotHeld = -1;
        private bool isScrolling;
        private string hoverText = string.Empty;

        public ChecklistMenu(ModConfig config)
        {
            this.config = config;

            Game1.playSound("bigSelect");
            var menuRect = CreateCenteredRectangle(Game1.viewport, MenuWidth, MenuHeight);
            this.initialize(menuRect.X, menuRect.Y, menuRect.Width, menuRect.Height, true);
            this.SetupTabs();
            this.SetupContents();
            this.SetupScrollbar();
        }

        private enum TabName
        {
            Checklist, Settings
        }

        public static List<ObjectList> ObjectLists { get; set; }

        public static int LineHeight => selectedTab == 0 ? 50 : 65;

        public static int ItemsPerPage => (MenuHeight - 2 * Margin) / LineHeight;

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
            if (!Game1.options.showMenuBackground)
            {
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);
            }

            SpriteText.drawStringWithScrollCenteredAt(b, "Checklist", this.xPositionOnScreen + this.width / 2, this.yPositionOnScreen - Game1.tileSize, string.Empty, 1f, -1, 0, 0.88f, false);
            drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, Color.White);

            if (selectedTab == 1)
            { // Draw batch front-to-back so that pop-ups show over top of other options
                b.End();
                b.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            }

            this.DrawOptions(b);

            if (selectedTab == 1)
            { // Restore
                b.End();
                b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null);
            }

            if (!GameMenu.forcePreventClose)
            {
                this.DrawTabs(b);
                if (this.options.Count > ItemsPerPage)
                {
                    this.DrawScrollbar(b);
                }
            }

            if (this.hoverText != string.Empty)
            {
                drawHoverText(b, this.hoverText, Game1.smallFont);
            }

            this.drawMouse(b);
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            if (GameMenu.forcePreventClose)
            {
                return;
            }

            if (this.downArrow.containsPoint(x, y) && this.scrollOffset < Math.Max(0, this.options.Count - ItemsPerPage))
            {
                this.DownArrowPressed();
                Game1.soundBank.PlayCue("shwip");
            }
            else if (this.upArrow.containsPoint(x, y) && this.scrollOffset > 0)
            {
                this.UpArrowPressed();
                Game1.soundBank.PlayCue("shwip");
            }
            else if (this.scrollbar.containsPoint(x, y))
            {
                this.isScrolling = true;
            }
            else if (!this.downArrow.containsPoint(x, y) && x > this.xPositionOnScreen + this.width && (x < this.xPositionOnScreen + this.width + Game1.tileSize * 2 && y > this.yPositionOnScreen) && y < this.yPositionOnScreen + this.height)
            {
                this.isScrolling = true;
                this.leftClickHeld(x, y);
                this.releaseLeftClick(x, y);
            }

            this.scrollOffset = Math.Max(0, Math.Min(this.options.Count - ItemsPerPage, this.scrollOffset));
            var slotIndex = this.optionSlots.FindIndex((slot) => slot.bounds.Contains(x, y));
            int realIndex = this.scrollOffset + slotIndex;
            if (slotIndex != -1 && realIndex < this.options.Count)
            {
                var option = this.options[realIndex];
                this.MapOptionCoords(x, y, out int mappedX, out int mappedY, slotIndex);
                if (option.bounds.Contains(mappedX, mappedY))
                {
                    option.receiveLeftClick(mappedX, mappedY);
                    this.optionsSlotHeld = slotIndex;
                    return;
                }
            }

            for (var i = 0; i < this.tabs.Count; i++)
            {
                if (this.tabs[i].bounds.Contains(x, y))
                {
                    selectedTab = i;
                    Game1.activeClickableMenu = new ChecklistMenu(this.config); // must be a new instance
                    return;
                }
            }
        }

        public override void leftClickHeld(int x, int y)
        {
            if (GameMenu.forcePreventClose)
            {
                return;
            }

            base.leftClickHeld(x, y);
            if (this.isScrolling)
            {
                int num = this.scrollbar.bounds.Y;
                this.scrollbar.bounds.Y = Math.Min(this.yPositionOnScreen + this.height - Game1.tileSize - Game1.pixelZoom * 3 - this.scrollbar.bounds.Height, Math.Max(y, this.yPositionOnScreen + this.upArrow.bounds.Height + Game1.pixelZoom * 5));
                this.scrollOffset = Math.Min(this.options.Count - ItemsPerPage, Math.Max(0, (int)(this.options.Count * (double)((y - this.scrollbarRunner.Y) / (float)this.scrollbarRunner.Height))));
                this.SetScrollBarToCurrentIndex();
                if (num == this.scrollbar.bounds.Y)
                {
                    return;
                }

                Game1.soundBank.PlayCue("shiny4");
            }
            else
            {
                this.LeftClickHeldOrReleasedForOption(x, y, held: true);
            }
        }

        public override void releaseLeftClick(int x, int y)
        {
            if (GameMenu.forcePreventClose)
            {
                return;
            }

            base.releaseLeftClick(x, y);
            this.LeftClickHeldOrReleasedForOption(x, y, held: false);

            this.optionsSlotHeld = -1;
            this.isScrolling = false;
        }

        public override void receiveScrollWheelAction(int direction)
        {
            if (GameMenu.forcePreventClose)
            {
                return;
            }

            base.receiveScrollWheelAction(direction);
            if (direction > 0 && this.scrollOffset > 0)
            {
                this.UpArrowPressed();
            }
            else
            {
                if (direction >= 0 || this.scrollOffset >= Math.Max(0, this.options.Count - ItemsPerPage))
                {
                    return;
                }

                this.DownArrowPressed();
            }
        }

        private static Rectangle CreateCenteredRectangle(xTile.Dimensions.Rectangle v, int width, int height)
        {
            var x = v.Width / 2 - width / 2;
            var y = v.Height / 2 - height / 2;
            return new Rectangle(x, y, width, height);
        }

        private void MapOptionCoords(int x, int y, out int mappedX, out int mappedY, int slotIndex = -1)
        {
            if (slotIndex == -1)
            {
                slotIndex = this.optionsSlotHeld;
            }

            int realIndex = slotIndex + this.scrollOffset;
            var slot = this.optionSlots[slotIndex];
            var option = this.options[realIndex];
            mappedX = x + option.bounds.X - slot.bounds.X;
            mappedY = y + option.bounds.Y - slot.bounds.Y;
        }

        private void LeftClickHeldOrReleasedForOption(int x, int y, bool held)
        {
            var slotIndex = this.optionsSlotHeld;
            int realIndex = slotIndex + this.scrollOffset;
            if (slotIndex == -1 || realIndex >= this.options.Count)
            {
                return;
            }

            var option = this.options[realIndex];
            this.MapOptionCoords(x, y, out int mappedX, out int mappedY, slotIndex);
            if (held)
            {
                option.leftClickHeld(mappedX, mappedY);
            }
            else
            {
                option.leftClickReleased(mappedX, mappedY);
            }
        }

        private void SetupTabs()
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

        private void SetupContents()
        {
            var config = this.config;
            int h = LineHeight;
            int x = this.xPositionOnScreen + Margin;
            int y = this.yPositionOnScreen + Margin;
            int w = this.width - Margin;

            for (var i = 0; i < ItemsPerPage; i++)
            {
                Rectangle bounds = new Rectangle(x, y + h * i, w, h);
                var slot = new ClickableComponent(bounds, i.ToString());
                this.optionSlots.Add(slot);
            }

            var j = 0;
            switch (selectedTab)
            {
                case 0:
                    this.options.AddRange(from ObjectList ol in ObjectLists
                                          where ol.ShowInMenu
                                          select new DynamicSelectableCheckbox(ol, x, y + h * j++));
                    break;

                case 1:
                    this.options.Add(new DCOptionsCheckbox("Show All Tasks", 3, config, x, y + h * j++));
                    this.options.Add(new DCOptionsCheckbox("Allow Multiple Overlays", 4, config, x, y + h * j++));
                    this.options.Add(new DCOptionsCheckbox("Show Arrow to Nearest Task", 5, config, x, y + h * j++));
                    this.options.Add(new DCOptionsCheckbox("Show Task Overlay", 6, config, x, y + h * j++));
                    this.options.Add(new DCOptionsDropDown("Button Position", 1, config, x, y + h * j++));
                    this.options.Add(new DCOptionsInputListener("Open Menu Key", 2, w, config, x, y + h * j++));
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        private void SetupScrollbar()
        {
            this.upArrow = GameTextures.UpArrow.AsClickableTextureComponent("up-arrow", this.xPositionOnScreen + this.width + Game1.tileSize / 4, this.yPositionOnScreen + Game1.tileSize);
            this.downArrow = GameTextures.DownArrow.AsClickableTextureComponent("down-arrow", this.xPositionOnScreen + this.width + Game1.tileSize / 4, this.yPositionOnScreen + this.height - Game1.tileSize);
            this.scrollbar = GameTextures.Scrollbar.AsClickableTextureComponent("scrollbar", this.upArrow.bounds.X + Game1.pixelZoom * 3, this.upArrow.bounds.Y + this.upArrow.bounds.Height + Game1.pixelZoom);
            this.scrollbarRunner = new Rectangle(this.scrollbar.bounds.X, this.upArrow.bounds.Y + this.upArrow.bounds.Height + Game1.pixelZoom, this.scrollbar.bounds.Width, this.height - Game1.tileSize * 2 - this.upArrow.bounds.Height - Game1.pixelZoom * 2);
        }

        private void SetScrollBarToCurrentIndex()
        {
            if (this.options.Count == 0)
            {
                return;
            }

            this.scrollbar.bounds.Y = (this.scrollOffset == this.options.Count - ItemsPerPage) ?
                this.scrollbarRunner.Height / Math.Max(1, this.options.Count - ItemsPerPage + 1) * this.scrollOffset + this.upArrow.bounds.Bottom + Game1.pixelZoom :
                this.downArrow.bounds.Y - this.scrollbar.bounds.Height - Game1.pixelZoom;
        }

        private void DownArrowPressed()
        {
            this.downArrow.scale = this.downArrow.baseScale;
            ++this.scrollOffset;
            this.SetScrollBarToCurrentIndex();
        }

        private void UpArrowPressed()
        {
            this.upArrow.scale = this.upArrow.baseScale;
            --this.scrollOffset;
            this.SetScrollBarToCurrentIndex();
        }

        private void DrawScrollbar(SpriteBatch b)
        {
            this.upArrow.draw(b);
            var dst = this.scrollbarRunner;
            drawTextureBox(b, GameTextures.ScrollbarRunner.Tex, GameTextures.ScrollbarRunner.Src, dst.X, dst.Y, dst.Width, dst.Height, Color.White, Game1.pixelZoom, false);
            this.scrollbar.draw(b);
            this.downArrow.draw(b);
        }

        private void DrawTabs(SpriteBatch b)
        {
            for (var j = 0; j < this.tabs.Count; j++)
            {
                var tab = this.tabs[j];
                var highlight = Color.White * (selectedTab == j ? 1f : 0.7f);
                drawTextureBox(b, tab.bounds.X, tab.bounds.Y, tab.bounds.Width, tab.bounds.Height, highlight);
                b.DrawString(Game1.smallFont, tab.name, new Vector2(tab.bounds.X + 15, tab.bounds.Y + 15), Color.Black);
            }
        }

        private void DrawOptions(SpriteBatch b)
        {
            int curIndex = this.scrollOffset;
            if (curIndex < 0)
            {
                return;
            }

            for (int slotIndex = 0; slotIndex < this.optionSlots.Count; slotIndex++)
            {
                var realIndex = curIndex + slotIndex;
                if (realIndex >= this.options.Count)
                {
                    break;
                }

                var option = this.options[realIndex];
                var slot = this.optionSlots[slotIndex];
                option.draw(b, slot.bounds.X - option.bounds.X, slot.bounds.Y - option.bounds.Y);
            }
        }
    }
}