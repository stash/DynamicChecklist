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

    public class AnimalList : ObjectList
    {
        private Action action;

        public AnimalList(ModConfig config, Action action)
            : base(config)
        {
            this.action = action;
            switch (action)
            {
                case Action.Pet:
                    this.ImageTexture = OverlayTextures.Heart;
                    this.OptionMenuLabel = "Pet Animals";
                    this.TaskDoneMessage = "All animals have been petted";
                    this.Name = TaskName.Pet;
                    break;
                case Action.Milk:
                    this.ImageTexture = OverlayTextures.MilkPail;
                    this.OptionMenuLabel = "Milk Cows/Goats";
                    this.TaskDoneMessage = "All Cows and Goats have been milked";
                    this.Name = TaskName.Milk;
                    break;
                case Action.Shear:
                    this.ImageTexture = OverlayTextures.Shears;
                    this.OptionMenuLabel = "Shear Sheep";
                    this.TaskDoneMessage = "All sheep have been sheared";
                    this.Name = TaskName.Shear;
                    break;
                default:
                    throw new NotImplementedException();
            }

            this.ObjectInfoList = new List<StardewObjectInfo>();
        }

        public enum Action
        {
            Pet, Milk, Shear
        }

        public override string OptionMenuLabel { get; protected set; }

        public override string TaskDoneMessage { get; protected set; }

        protected override Texture2D ImageTexture { get; set; }

        public override void OnMenuOpen()
        {
        }

        public override void BeforeDraw()
        {
            if (!this.TaskDone && Game1.currentLocation.IsFarm)
            {
                this.UpdateObjectInfoList();
            }
        }

        protected override void UpdateObjectInfoList()
        {
            this.ObjectInfoList.Clear();

            // Outside animals
            Farm farm = Game1.getFarm();
            this.AddAnimalsFromLocation(farm, farm.animals.Values);

            // Inside animals
            foreach (var animalHouse in ListHelper.GetFarmAnimalHouses())
            {
                this.AddAnimalsFromLocation(animalHouse, animalHouse.animals.Values);
            }

            this.TaskDone = !this.ObjectInfoList.Any(soi => soi.NeedAction);
        }

        private void AddAnimalsFromLocation(GameLocation loc, IEnumerable<FarmAnimal> animals)
        {
            this.ObjectInfoList.AddRange(from animal in animals
                                         select this.CreateSOI(animal, loc));
        }

        private StardewObjectInfo CreateSOI(FarmAnimal animal, GameLocation loc)
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

            var soi = new StardewObjectInfo
            {
                Coordinate = animal.getStandingPosition(),
                Location = loc,
                NeedAction = needAction
            };
            return soi;
        }
    }
}
