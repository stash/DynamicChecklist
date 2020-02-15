namespace DynamicChecklist.ObjectLists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
    using StardewModdingAPI;
    using StardewValley;
    using StardewValley.Buildings;
    using StardewValley.Menus;

    public class NPCList : ObjectList
    {
        private const string BirthdayMenuGeneric = "Give Birthday Gift";
        private const string BirthdayMenuPerson = "Give {person} a Birthday Gift";
        private const string BirthdayDoneGeneric = "You gave a birthday gift!";
        private const string BirthdayDonePerson = "You gave {person} a birthday gift!";

        private Action action;
        private List<NPC> trackedNPCs;
        private Func<NPC, bool> taskCheck;

        public NPCList(ModConfig config, Action action)
            : base(config)
        {
            this.action = action;
            this.ImageTexture = OverlayTextures.Heart;
            this.trackedNPCs = new List<NPC>();
            switch (action)
            {
                case Action.Birthday:
                    this.OptionMenuLabel = BirthdayMenuGeneric;
                    this.TaskDoneMessage = BirthdayDoneGeneric;
                    this.Name = TaskName.Birthday;
                    this.taskCheck = this.CheckBirthday;
                    break;
                case Action.Spouse:
                    this.OptionMenuLabel = "Kiss Spouse";
                    this.TaskDoneMessage = "Happy spouse, happy house!";
                    this.Name = TaskName.Spouse;
                    this.taskCheck = this.CheckSpouse;
                    break;
                case Action.Child:
                    this.OptionMenuLabel = "Hug Children";
                    this.TaskDoneMessage = "All children hugged!";
                    this.Name = TaskName.Child;
                    this.taskCheck = this.CheckChild;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public enum Action
        {
            Birthday, Spouse, Child
        }

        protected override void InitializeObjectInfoList()
        {
            this.trackedNPCs.Clear();
            switch (this.action)
            {
                case Action.Birthday:
                    var npc = ListHelper.GetNPCs().FirstOrDefault(n => n.isBirthday(Game1.currentSeason, Game1.dayOfMonth));
                    if (npc != default)
                    {
                        this.OptionMenuLabel = BirthdayMenuPerson.Replace("{person}", npc.displayName);
                        this.TaskDoneMessage = BirthdayDonePerson.Replace("{person}", npc.displayName);
                        this.TrackNPC(npc);
                    }
                    else
                    {
                        this.OptionMenuLabel = BirthdayMenuGeneric;
                        this.TaskDoneMessage = BirthdayDoneGeneric;
                    }

                    break;

                case Action.Spouse:
                    if (Game1.player.isMarried())
                    {
                        var spouse = Game1.getCharacterFromName(Game1.player.spouse, true);
                        this.TrackNPC(spouse);
                    }

                    break;

                case Action.Child:
                    Game1.player.getChildren().ForEach(this.TrackNPC);

                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        protected override void UpdateObjectInfoList(uint ticks)
        {
            if (this.TaskDone || !this.TaskExistedAtStartOfDay)
            {
                return; // no further updates
            }

            int i = 0;
            foreach (var npc in this.trackedNPCs)
            {
                var soi = this.ObjectInfoList[i++];
                soi.SetCharacterPosition(npc);
                soi.NeedAction = this.taskCheck(npc);
            }
        }

        private void TrackNPC(NPC npc)
        {
            this.trackedNPCs.Add(npc);
            this.ObjectInfoList.Add(new StardewObjectInfo(npc));
        }

        private bool CheckBirthday(NPC npc)
        {
            return Game1.player.friendshipData.TryGetValue(npc.Name, out var friendship) && friendship.GiftsToday < 1;
        }

        private bool CheckSpouse(NPC npc)
        {
            return !npc.hasBeenKissedToday.Value;
        }

        private bool CheckChild(NPC npc)
        {
            return Game1.player.friendshipData.TryGetValue(npc.Name, out var friendship) && !friendship.TalkedToToday;
        }
    }
}