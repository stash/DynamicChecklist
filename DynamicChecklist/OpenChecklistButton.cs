using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using DynamicChecklist.ObjectLists;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewModdingAPI.Events;

namespace DynamicChecklist
{
    class OpenChecklistButton : IClickableMenu
    {
        private Rectangle sourceRectangle;
        private readonly Texture2D texture;
        private Action openChecklist;
        private string hoverText = "";
        private Func<int> countRemainingTasks;
        private ModConfig config;

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
            
        }
        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            openChecklist();
            base.receiveLeftClick(x, y, playSound);
        }
        public override void performHoverAction(int x, int y)
        {
            hoverText = "Checklist";
        }
        public OpenChecklistButton(Action OpenChecklist, Func<int> CountRemainingTasks, ModConfig config) 
            : base(0, 0, OverlayTextures.Sign.Width*Game1.pixelZoom, OverlayTextures.Sign.Height*Game1.pixelZoom, false)
        {
            this.config = config;
            countRemainingTasks = CountRemainingTasks;
            texture = OverlayTextures.Sign;
            openChecklist = OpenChecklist;

            MenuEvents.MenuClosed += this.OnMenuClosed;
            UpdateButtonPosition(config.OpenChecklistButtonLocation);
        }

        private void OnMenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            if (e.PriorMenu is ChecklistMenu)
            {
                UpdateButtonPosition(config.OpenChecklistButtonLocation);
            }
        }

        private void UpdateButtonPosition(ModConfig.ButtonLocation buttonLocation)
        {
            switch (buttonLocation)
            {
                case ModConfig.ButtonLocation.BelowJournal:
                    xPositionOnScreen = Game1.viewport.Width - 300 + 180 + Game1.tileSize / 2;
                    yPositionOnScreen = Game1.tileSize / 8 + 296;
                    break;
                case ModConfig.ButtonLocation.LeftOfJournal:
                    xPositionOnScreen = Game1.viewport.Width - 300 + 125 + Game1.tileSize / 2;
                    yPositionOnScreen = Game1.tileSize / 8 + 240;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override void draw(SpriteBatch b)
        {
            int tasks = countRemainingTasks();
            SpriteFont font = (tasks > 9) ? Game1.smallFont : Game1.dialogueFont;
            string s = tasks.ToString();
            Vector2 sSize = font.MeasureString(s);
            Vector2 sPos = new Vector2(xPositionOnScreen + width / 2 - sSize.X/2, yPositionOnScreen + height / 2 - sSize.Y/2 + 10);
            b.Draw(texture, new Rectangle(xPositionOnScreen, yPositionOnScreen, width, height), Color.White);
            b.DrawString(font, tasks.ToString(), sPos, Color.Black);
            if(this.isWithinBounds(Game1.getOldMouseX(), Game1.getOldMouseY()))
            {
                drawHoverText(Game1.spriteBatch, hoverText, Game1.dialogueFont);
            }            
            base.draw(b);
        }
    }
}
