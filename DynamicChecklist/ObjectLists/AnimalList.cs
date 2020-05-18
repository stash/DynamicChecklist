namespace DynamicChecklist.ObjectLists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using StardewValley;

    public class AnimalList : ObjectList
    {
        public AnimalList(TaskName name)
            : base(name)
        {
            switch (name)
            {
                case TaskName.Pet:
                    this.ImageTexture = GameTexture.HeartSmol;
                    this.OptionMenuLabel = "Pet Animals";
                    this.TaskDoneMessage = "All animals have been petted";
                    break;
                case TaskName.Milk:
                    this.ImageTexture = GameTexture.MilkPail;
                    this.OptionMenuLabel = "Milk Cows/Goats";
                    this.TaskDoneMessage = "All Cows and Goats have been milked";
                    break;
                case TaskName.Shear:
                    this.ImageTexture = GameTexture.Shears;
                    this.OptionMenuLabel = "Shear Sheep";
                    this.TaskDoneMessage = "All sheep have been sheared";
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        protected override void InitializeObjectInfoList()
        {
            this.UpdateObjectInfoList(0);
        }

        protected override void UpdateObjectInfoList(uint ticks)
        {
            if (this.TaskDone && this.TaskName != TaskName.Pet)
            {
                // Animals can't become un-petted, un-sheared or un-milked, so once it's done for the day, it's done
                // HOWEVER, what about just purchased animals? They can still be petted
                return;
            }

            // TODO: just update position using SetCharacterPosition ??
            // TODO: Scan for new animals once per second (ticks % 60 == 0)
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
            // Bug: occasionally null. Unsure why this happens, just that it does.
            if (animal.currentLocation == null)
            {
                return false;
            }

            bool needAction;
            switch (this.TaskName)
            {
                case TaskName.Pet:
                    needAction = !animal.wasPet.Value;
                    break;
                case TaskName.Milk:
                    needAction = animal.currentProduce.Value > 0 && animal.toolUsedForHarvest.Value == "Milk Pail";
                    break;
                case TaskName.Shear:
                    needAction = animal.currentProduce.Value > 0 && animal.toolUsedForHarvest.Value == "Shears";
                    break;
                default:
                    throw new NotImplementedException();
            }

            return needAction;
        }
    }
}
