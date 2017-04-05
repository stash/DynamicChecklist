namespace DynamicChecklist.Options
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using StardewValley.Menus;

    public class DCOptionsCheckbox : OptionsCheckbox
    {
        private ModConfig config;

        public DCOptionsCheckbox(string label, int whichOption, ModConfig config, int x = -1, int y = -1)
            : base(label, 1, x, y)
        {
            this.config = config;
            switch (whichOption)
            {
                case 3:
                    this.isChecked = config.ShowAllTasks;
                    break;
                case 4:
                    this.isChecked = config.AllowMultipleOverlays;
                    break;
                case 5:
                    this.isChecked = config.AllowMultipleOverlays;
                    break;
                case 6:
                    this.isChecked = config.AllowMultipleOverlays;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override void receiveLeftClick(int x, int y)
        {
            base.receiveLeftClick(x, y);
            this.isChecked = !this.isChecked;
            switch (this.whichOption)
            {
                case 3:
                    this.config.ShowAllTasks = this.isChecked;
                    break;
                case 4:
                    this.config.AllowMultipleOverlays = this.isChecked;
                    break;
                case 5:
                    this.config.AllowMultipleOverlays = this.isChecked;
                    break;
                case 6:
                    this.config.AllowMultipleOverlays = this.isChecked;
                    break;
            }
        }
    }
}
