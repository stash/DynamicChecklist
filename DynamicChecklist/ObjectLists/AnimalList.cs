using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewValley.Menus;
using StardewValley.Buildings;
using StardewValley;
using System.Linq;
using Microsoft.Xna.Framework.Input;

namespace DynamicChecklist.ObjectLists
{
    class AnimalList : ObjectList
    {
        private List<FarmAnimal> animals;
        public enum Action { Pet, Milk, Shear };
        private Action action;

        public override string OptionMenuLabel { get; protected set; }
        public override string TaskDoneMessage { get; protected set; }

        protected override Texture2D ImageTexture { get; set; }

        public AnimalList(Action action)
        {
            this.action = action;
            switch (action){
                case Action.Pet:
                    ImageTexture = OverlayTextures.Heart;
                    OptionMenuLabel = "Petted Animals";
                    TaskDoneMessage = "All animals have been petted";
                    break;
                case Action.Milk:
                    ImageTexture = OverlayTextures.MilkPail;
                    OptionMenuLabel = "Milked Cows/Goats";
                    TaskDoneMessage = "All Cows and Goats have been milked";
                    break;
                case Action.Shear:
                    ImageTexture = OverlayTextures.Shears;
                    OptionMenuLabel = "Sheared Sheep";
                    TaskDoneMessage = "All sheep have been sheared";
                    break;
                default:
                    throw (new NotImplementedException());
            }
            
            ObjectInfoList = new List<StardewObjectInfo>();
        }
        public override void OnMenuOpen()
        {

        }
        
        private StardewObjectInfo CreateSOI(FarmAnimal animal, GameLocation loc, Action action)
        {
            var soi = new StardewObjectInfo();
            soi.Coordinate = animal.getStandingPosition();
            soi.Location = Game1.getFarm();
            switch (action)
            {
                case Action.Pet:
                    soi.NeedAction = !animal.wasPet;
                    break;
                case Action.Milk:
                    soi.NeedAction = animal.currentProduce>0;
                    break;
                case Action.Shear:
                    soi.NeedAction = animal.currentProduce > 0;
                    break;
                default:
                    throw (new NotImplementedException());
            }
            
            return soi;
        }

        public override void BeforeDraw()
        {           
            if (!TaskDone && Game1.currentLocation.IsFarm)
            {
                UpdateObjectInfoList();
                //if (TaskDone)
                //{
                //    OnTaskFinished(new EventArgs());
                //}
                
            }           
        }
        protected override void UpdateObjectInfoList()
        {
            this.ObjectInfoList.Clear();
            //Outside animals
            var outsideAnimals = Game1.getFarm().animals.Values.ToList<FarmAnimal>();
            foreach (FarmAnimal animal in outsideAnimals)
            {
                StardewObjectInfo soi = CreateSOI(animal, Game1.getFarm(), action);
                ObjectInfoList.Add(soi);
            }
            // Inside animals
            var farmBuildings = Game1.getFarm().buildings;

            foreach (Building building in farmBuildings)
            {
                if (building.indoors != null && building.indoors.GetType() == typeof(AnimalHouse))
                {
                    var animalHouse = (AnimalHouse)building.indoors;
                    foreach (FarmAnimal animal in animalHouse.animals.Values.ToList<FarmAnimal>())
                    {
                        StardewObjectInfo soi = CreateSOI(animal, animalHouse, action);
                        ObjectInfoList.Add(soi);
                    }

                }
            }
            var taskDone = true;
            foreach (StardewObjectInfo soi in ObjectInfoList)
            {
                if (soi.NeedAction)
                {
                    taskDone = false;
                    break;
                }
            }
            TaskDone = taskDone;
        }
    }
}
