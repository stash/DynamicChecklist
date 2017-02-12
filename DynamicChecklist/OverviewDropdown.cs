using StardewValley.Menus;
using System.Collections.Generic;

namespace DynamicChecklist
{
    public class OverviewDropdown : OptionsCheckbox
    {
        public List<string> dropDownOptions = new List<string>();
        public OverviewDropdown(string label, int whichOption, int x = -1, int y = -1) : base(label, whichOption, x, y)
        {
            
        }
        public override void receiveLeftClick(int x, int y)
        {
            base.receiveLeftClick(x, y);
            var a = 1;
        }
            
    }
}