namespace DynamicChecklist
{
    using DynamicChecklist.ObjectLists;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewValley;
    using StardewValley.Menus;

    internal class DynamicSelectableCheckbox : OptionsCheckbox
    {
        private static Texture2D whitePixel;
        private bool isDone = true;
        private ObjectList objectList;
        private Vector2 labelSize;

        public DynamicSelectableCheckbox(ObjectList objectList, int x = -1, int y = -1)
            : base(objectList.OptionMenuLabel, 1, x, y)
        {
            this.objectList = objectList;
            this.isDone = objectList.TaskDone;

            this.labelSize = Game1.dialogueFont.MeasureString(this.label);
        }

        public override void draw(SpriteBatch b, int slotX, int slotY)
        {
            this.isChecked = this.objectList.OverlayActive;
            base.draw(b, slotX, slotY);
            if (this.isDone)
            {
                this.StrikeThrough(b, slotX, slotY);
            }
        }

        public override void receiveLeftClick(int x, int y)
        {
            base.receiveLeftClick(x, y);
            if (this.objectList != null)
            {
                this.objectList.OverlayActive = this.isChecked;
            }
        }

        private void StrikeThrough(SpriteBatch b, int slotX, int slotY)
        {
            if (whitePixel == null || whitePixel.GraphicsDevice != Game1.graphics.GraphicsDevice)
            {
                whitePixel = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
                whitePixel.SetData(new Color[] { Color.White });
            }

            var destRect = new Rectangle(slotX + this.bounds.X + this.bounds.Width + Game1.pixelZoom * 2, slotY + this.bounds.Y + (int)this.labelSize.Y / 3, (int)this.labelSize.X, Game1.pixelZoom);
            b.Draw(whitePixel, destRect, Color.Red);
        }
    }
}