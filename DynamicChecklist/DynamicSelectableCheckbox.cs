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

namespace DynamicChecklist
{
    internal class DynamicSelectableCheckbox : OptionsCheckbox
    {
        private bool isDone = true;
        private ObjectList objectList;
        private Vector2 labelSize;

        public DynamicSelectableCheckbox(ObjectList objectList, int x = -1, int y = -1)
            : base(objectList.OptionMenuLabel, 1, x, y)
        {
            this.objectList = objectList;
            this.isDone = objectList.TaskDone;            

            labelSize = Game1.dialogueFont.MeasureString(label);
        }

        public override void draw(SpriteBatch b, int slotX, int slotY)
        {
            this.isChecked = objectList.OverlayActive;
            base.draw(b, slotX, slotY);
            var whitePixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            whitePixel.SetData(new Color[] { Color.White });
            var destRect = new Rectangle((slotX + this.bounds.X+ this.bounds.Width + Game1.pixelZoom * 2), (slotY + this.bounds.Y+(int)labelSize.Y/3), (int)labelSize.X, Game1.pixelZoom);
            if (this.isDone)
            {
                b.Draw(whitePixel, destRect, Color.Red);
            }                                    
        }
        public override void receiveLeftClick(int x, int y)
        {
            base.receiveLeftClick(x, y);
            //isSelected = !isSelected;
            if (objectList != null) this.objectList.OverlayActive = this.isChecked;
            //Game1.playSound("drumkit6");
            
        }
    }
}