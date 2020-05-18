namespace DynamicChecklist.ObjectLists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using StardewValley;
    using StardewValley.Characters;

    public class NPCList : ObjectList
    {
        private const string BirthdayMenuGeneric = "Give Birthday Gift";
        private const string BirthdayMenuPerson = "Give {person} a Birthday Gift";
        private const string BirthdayDoneGeneric = "You gave a birthday gift!";
        private const string BirthdayDonePerson = "You gave {person} a birthday gift!";
        private const string SpouseMenuGeneric = "Kiss Spouse";
        private const string SpouseMenuPerson = "Kiss {person}";
        private const string ChildMenuGeneric = "Hug Children";
        private const string ChildMenuPerson = "Hug {person}";
        private const string ChildMenuPlural = "Hug Your Children";
        private const string PetMenuGeneric = "Pet Your Pet and Fill Bowl";
        private const string PetMenuPlural = "Pet Your Pets and Fill Bowl";
        private const string PetMenuSpecific = "Pet {name} and Fill Bowl";
        private const string PetDoneGeneric = "Pet cared for!";
        private const string PetDonePlural = "Pets cared for!";

        private List<NPC> trackedNPCs;
        private Func<NPC, bool> taskCheck;
        private System.Action taskSetup;
        private Vector2 drawOffset;

        public NPCList(TaskName name)
            : base(name)
        {
            this.trackedNPCs = new List<NPC>();
            switch (name)
            {
                case TaskName.Birthday:
                    this.ImageTexture = GameTexture.Present;
                    this.OptionMenuLabel = BirthdayMenuGeneric;
                    this.TaskDoneMessage = BirthdayDoneGeneric;
                    this.taskCheck = this.CheckBirthday;
                    this.taskSetup = this.SetupBirthday;
                    this.drawOffset = StardewObjectInfo.TallCharacterOffset;
                    break;
                case TaskName.Spouse:
                    this.ImageTexture = GameTexture.HeartSmol;
                    this.OptionMenuLabel = SpouseMenuGeneric;
                    this.TaskDoneMessage = "Happy spouse, happy house!";
                    this.taskCheck = this.CheckSpouse;
                    this.taskSetup = this.SetupSpouse;
                    this.drawOffset = StardewObjectInfo.TallCharacterOffset;
                    break;
                case TaskName.Child:
                    this.ImageTexture = GameTexture.HeartSmol;
                    this.OptionMenuLabel = ChildMenuGeneric;
                    this.TaskDoneMessage = "All children hugged!";
                    this.taskCheck = this.CheckChild;
                    this.taskSetup = this.SetupChildren;
                    this.drawOffset = StardewObjectInfo.CharacterOffset;
                    break;
                case TaskName.CareForPet:
                    this.ImageTexture = GameTexture.HeartSmol;
                    this.OptionMenuLabel = PetMenuGeneric;
                    this.TaskDoneMessage = PetDoneGeneric;
                    this.taskCheck = this.CheckPet;
                    this.taskSetup = this.SetupPet;
                    this.drawOffset = StardewObjectInfo.CharacterOffset;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected override void InitializeObjectInfoList()
        {
            this.trackedNPCs.Clear();
            this.taskSetup();
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

            if (this.TaskName == TaskName.CareForPet)
            {
                var bowlSoi = this.ObjectInfoList[this.ObjectInfoList.Count - 1];
                bowlSoi.NeedAction = !Game1.getFarm().petBowlWatered.Value;
            }
        }

        private void TrackNPC(NPC npc)
        {
            this.trackedNPCs.Add(npc);
            var soi = new StardewObjectInfo(npc);
            soi.DrawOffset = this.drawOffset;
            this.ObjectInfoList.Add(soi);
        }

        private void SetupBirthday()
        {
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
        }

        private bool CheckBirthday(NPC npc)
        {
            return Game1.player.friendshipData.TryGetValue(npc.Name, out var friendship) && friendship.GiftsToday < 1;
        }

        private void SetupSpouse()
        {
            // I know there's mods for polyamory, but currently it's a singular field on the Player:
            if (Game1.player.isMarried())
            {
                var spouse = Game1.getCharacterFromName(Game1.player.spouse, true);
                this.OptionMenuLabel = SpouseMenuPerson.Replace("{person}", spouse.displayName);
                this.TrackNPC(spouse);
            }
        }

        private bool CheckSpouse(NPC npc)
        {
            return !npc.hasBeenKissedToday.Value;
        }

        private void SetupChildren()
        {
            int kids = 0;
            foreach (var kid in Game1.player.getChildren())
            {
                kids++;
                if (kids == 1)
                {
                    this.OptionMenuLabel = ChildMenuPerson.Replace("{person}", kid.displayName);
                }

                this.TrackNPC(kid);
            }

            if (kids > 1)
            {
                this.OptionMenuLabel = ChildMenuPlural;
            }
        }

        private bool CheckChild(NPC npc)
        {
            return Game1.player.friendshipData.TryGetValue(npc.Name, out var friendship) && !friendship.TalkedToToday;
        }

        private void SetupPet()
        {
            int pets = 0;
            var farm = Game1.getFarm();
            foreach (var pet in farm.characters.OfType<Pet>())
            {
                pets++;
                if (pets == 1)
                {
                    this.OptionMenuLabel = PetMenuSpecific.Replace("{name}", pet.displayName);
                }

                this.TrackNPC(pet);
            }

            // More than one pet via some Mod
            if (pets > 1)
            {
                this.OptionMenuLabel = PetMenuPlural;
                this.TaskDoneMessage = PetDonePlural;
            }

            if (pets > 0)
            {
                var point = farm.petBowlPosition.Value;
                var bowlSoi = new StardewObjectInfo();
                bowlSoi.SetTilePosition(new Vector2(point.X, point.Y), LocationReference.For(farm));
                this.ObjectInfoList.Add(bowlSoi);
            }
        }

        private bool CheckPet(NPC npc)
        {
            return !((Pet)npc).grantedFriendshipForPet.Value;
        }
    }
}