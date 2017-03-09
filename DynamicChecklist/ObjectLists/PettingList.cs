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
    class PettingList : ObjectList
    {
        private List<FarmAnimal> animals;
        
        public override string OptionMenuLabel {get
            {
                return "Petted Animals";
            }
        }

        public PettingList()
        {
            ImageTexture = OverlayTextures.getTexture(0);
            OverlayActive = true;
            ObjectInfoList = new List<StardewObjectInfo>();
        }

        public override void beforeMenuOpenUpdate()
        {
            updateAnimalList();
        }

        public override void updateObjectInfo()
        {
            if (this.OverlayActive)
            {
                this.ObjectInfoList.Clear();
                //Outside animals
                var outsideAnimals = Game1.getFarm().animals.Values.ToList<FarmAnimal>();
                foreach (FarmAnimal animal in outsideAnimals)
                {
                    var soi = new StardewObjectInfo();
                    soi.Coordinate = animal.getStandingPosition();
                    soi.Location = Game1.getFarm();                   
                    soi.NeedAction = !animal.wasPet;
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
                            var soi = new StardewObjectInfo();
                            soi.Coordinate = animal.getStandingPosition();
                            soi.Location = Game1.getFarm();
                            soi.NeedAction = !animal.wasPet;
                            ObjectInfoList.Add(soi);
                        }

                    }
                }
            }         
        }

        private void updateAnimalList()
        {
            animals = Game1.getFarm().getAllFarmAnimals();
        }

    }
}
