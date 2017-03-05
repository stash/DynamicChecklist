using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley;
using System.Linq;

namespace DynamicChecklist
{
    internal class AddToChecklistElement : OptionsElement
    {
        string currentString;
        public AddToChecklistElement(int slotWidth, int x = -1, int y = -1)
            : base("", x, y, slotWidth - x, 11)
        {
            currentString = "abc";
            // TODO use StardewValley.Menus.TextBox
            // To use it, just detect when the player clicks on it and then call textbox.SelectMe() to enable it
        }

        public override void draw(SpriteBatch b, int slotX, int slotY)
        {
            //b.Draw(Game1.staminaRect, new Rectangle(1, 1, 100, 100), Color.White);
            b.DrawString(Game1.dialogueFont, currentString, new Vector2(500,500), Color.Black);
            //base.draw(b, slotX, slotY);
        }
        public override void receiveKeyPress(Keys key)
        {
            if (key == Keys.Back)
            {
                currentString = currentString.Remove(currentString.Length - 1);
            }
            else if (AddToChecklistElement.IsKeyAChar(key))
            {
                if (Keyboard.GetState().IsKeyDown(Keys.LeftShift) || Keyboard.GetState().IsKeyDown(Keys.RightShift))
                {
                    currentString += key.ToString();
                }
                else
                {
                    currentString += key.ToString().ToLower();
                }                                   
               
            }
            else if (key == Keys.Space)
            {
                currentString += " ";
            }
            else if (key == Keys.Enter)
            {
                addCheckbox(false, true, true, currentString);
                currentString = "";
            }
            base.receiveKeyPress(key);
        }
        public void addCheckbox(bool def, bool clickable, bool deletable, string name)
        {

        }
        private static bool IsKeyAChar(Keys key)
        {
            return (key >= Keys.A && key <= Keys.Z) || (key >= Keys.D0 && key <= Keys.D9) || (key >= Keys.NumPad0 && key <= Keys.NumPad9);
        }
    }
}