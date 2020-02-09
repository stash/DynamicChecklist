namespace DynamicChecklist
{
    using System;
    using System.Collections.Generic;
    using ObjectLists;

    public class ModConfig
    {
        public ModConfig()
        {
            this.IncludeTask = new Dictionary<TaskName, bool>();
            this.AddMissingTasks();
        }

        public enum ButtonLocation
        {
            BelowJournal, LeftOfJournal
        }

        public string OpenMenuKey { get; set; } = "NumPad1";

        public bool ShowAllTasks { get; set; } = false;

        public bool AllowMultipleOverlays { get; set; } = true;

        public bool ShowArrow { get; set; } = true;

        public bool ShowOverlay { get; set; } = true;

        public ButtonLocation OpenChecklistButtonLocation { get; set; } = ButtonLocation.BelowJournal;

        public Dictionary<TaskName, bool> IncludeTask { get; set; }

        public void Check()
        {
            // Add any new tasks to the list, but set them to be disabled
            this.AddMissingTasks();
        }

        private void AddMissingTasks()
        {
            var listNames = (TaskName[])Enum.GetValues(typeof(TaskName));
            foreach (var listName in listNames)
            {
                if (!this.IncludeTask.ContainsKey(listName))
                {
                    this.IncludeTask.Add(listName, true);
                }
            }
        }
    }
}
