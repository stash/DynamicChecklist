﻿namespace DynamicChecklist.ObjectLists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using StardewValley;

    public class AnimalList : ObjectList
    {
        private Action action;

        public AnimalList(ModConfig config, TaskName name, Action action)
            : base(config, name)
        {
            this.action = action;
            switch (action)
            {
                case Action.Pet:
                    this.ImageTexture = GameTexture.HeartSmol;
                    this.OptionMenuLabel = "Pet Animals";
                    this.TaskDoneMessage = "All animals have been petted";
                    break;
                case Action.Milk:
                    this.ImageTexture = GameTexture.MilkPail;
                    this.OptionMenuLabel = "Milk Cows/Goats";
                    this.TaskDoneMessage = "All Cows and Goats have been milked";
                    break;
                case Action.Shear:
                    this.ImageTexture = GameTexture.Shears;
                    this.OptionMenuLabel = "Shear Sheep";
                    this.TaskDoneMessage = "All sheep have been sheared";
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public enum Action
        {
            Pet, Milk, Shear
        }

        protected override void InitializeObjectInfoList()
        {
            this.UpdateObjectInfoList(0);
        }

        protected override void UpdateObjectInfoList(uint ticks)
        {
            if (this.TaskDone && this.action != Action.Pet)
            {
                // Animals can't become un-petted, un-sheared or un-milked, so once it's done for the day, it's done
                // TODO: consider just purchased animals
                return;
            }

            // "Have to" wipe it out every time since animals move around.
            // TODO: just update position using SetCharacterPosition ??
            this.ObjectInfoList.Clear();

            // Outside animals
            Farm farm = Game1.getFarm();
            this.AddAnimalsFromLocation(farm.animals.Values);

            // Inside animals
            foreach (var animalHouse in ListHelper.GetFarmAnimalHouses())
            {
                this.AddAnimalsFromLocation(animalHouse.animals.Values);
            }
        }

        private void AddAnimalsFromLocation(IEnumerable<FarmAnimal> animals)
        {
            this.ObjectInfoList.AddRange(from animal in animals
                                         where this.AnimalFilter(animal)
                                         select new StardewObjectInfo(animal) { DrawOffset = StardewObjectInfo.CharacterOffset });
        }

        private bool AnimalFilter(FarmAnimal animal)
        {
            bool needAction;
            switch (this.action)
            {
                case Action.Pet:
                    needAction = !animal.wasPet.Value;
                    break;
                case Action.Milk:
                    needAction = animal.currentProduce.Value > 0 && animal.toolUsedForHarvest.Value == "Milk Pail";
                    break;
                case Action.Shear:
                    needAction = animal.currentProduce.Value > 0 && animal.toolUsedForHarvest.Value == "Shears";
                    break;
                default:
                    throw new NotImplementedException();
            }

            return needAction;
        }
    }
}
