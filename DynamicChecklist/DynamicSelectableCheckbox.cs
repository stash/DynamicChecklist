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
        public bool isSelected = true;
        private ObjectCollection objectCollection;
        private ObjectList objectList;

        [Obsolete("Use other constructor")]
        public DynamicSelectableCheckbox(string label, int whichOption, ObjectCollection objectCollection,int x = -1, int y = -1)
            : base(label, whichOption, x, y)
        {
            this.objectCollection = objectCollection;

            switch (whichOption)
            {
                // TODO Handling cases seems unneccesery. Generalize?
                case 1:
                    this.isChecked = (objectCollection.crabTrapList.nNeedAction == 0);
                    break;
                case 2:
                    this.isChecked = objectCollection.cropList.watered.All(xx => xx == true);
                    break;
                case 3:
                    this.isChecked = (objectCollection.coopList.nUncollectedEggs == 0);
                    break;
                case 4:
                    this.isChecked = (objectCollection.coopList.nNotMilked == 0);
                    break;
                case 5:
                    this.isChecked = (objectCollection.coopList.nNotPetted == 0);
                    break;
                case 6:
                    this.isChecked = (objectCollection.coopList.nNotFed == 0);
                    break;
            }

        }
        public DynamicSelectableCheckbox(ObjectList objectList, int x = -1, int y = -1)
            : base(objectList.OptionMenuLabel, 1, x, y)
        {
            this.objectList = objectList;
            this.isChecked = objectList.TaskDone;
            this.isSelected = objectList.OverlayActive;
        }

        public override void draw(SpriteBatch b, int slotX, int slotY)
        {            
            // TODO: Strikethrough when option done, checkbox for overlay
            var whitePixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            whitePixel.SetData(new Color[] { Color.White });
            var destRect = new Rectangle((slotX + this.bounds.X+ this.bounds.Width), (slotY + this.bounds.Y-5), 500, this.bounds.Height);
            Color c = new Color();
            if (this.isSelected)
            {
                c =  new Color(0, 255, 0, 100);

            }
            else
            {
                c = new Color(255, 0, 0, 100);
            }
            b.Draw(whitePixel, destRect, c);
            base.draw(b, slotX, slotY);
        }
        public override void receiveLeftClick(int x, int y)
        {
            isSelected = !isSelected;
            if (objectList != null)this.objectList.OverlayActive = isSelected;
            Game1.playSound("drumkit6");
            //base.receiveLeftClick(x, y);
        }
    }
}