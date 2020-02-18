namespace DynamicChecklist
{
    using DynamicChecklist.ObjectLists;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using StardewValley;
    using StardewValley.Menus;

    internal class DynamicSelectableCheckbox : OptionsCheckbox
    {
        private static readonly Color StrikethroughColor = new Color(1f, 0, 0, 0.5f);

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
            var pixel = GameTexture.White;
            var destRect = new Rectangle(slotX + this.bounds.X + this.bounds.Width + Game1.pixelZoom * 1, slotY + this.bounds.Y + (int)this.labelSize.Y / 3 + 2 * Game1.pixelZoom, (int)this.labelSize.X + Game1.pixelZoom, Game1.pixelZoom * 5 / 4);
            b.Draw(pixel.Tex, destRect, StrikethroughColor);
        }
    }
}