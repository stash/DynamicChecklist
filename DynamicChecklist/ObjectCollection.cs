using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewValley.Objects;
using StardewValley;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace DynamicChecklist
{
    public class ObjectCollection
    {
        public List<StardewValley.TerrainFeatures.HoeDirt> hoeDirts;
        public CropList cropList;
        public CrabTrapList crabTrapList;
        public CoopList coopList;

        public Texture2D cropSpriteSheet;
        
        public ObjectCollection(Texture2D cropSpriteSheet)
        {
            this.cropSpriteSheet = cropSpriteSheet;
            cropList = new CropList();
            crabTrapList = new CrabTrapList();
            coopList = new CoopList();           
        }
        public void update()
        {
            var locs = Game1.locations;
            foreach (GameLocation loc in locs)
            {
                var o = loc.Objects;
                if (loc is Farm)
                {
                    this.cropList.update((Farm)loc);
                    this.coopList.updateAll((Farm)loc);
                }
            }
            this.crabTrapList.updateAll();            
        }

    }
    public class CoopList
    {
        public List<CoopLoc> coops = new List<CoopLoc>();
        public int nUncollectedEggs;
        public int nNotPetted;
        public int nNotFed;
        public int nNotMilked;

        public CoopList()
        {

        }

        public void updateAll(Farm farm)
        {
            coops = new List<CoopLoc>();
            nUncollectedEggs = 0;
            nNotPetted = 0;
            nNotFed = 0;
            nNotMilked = 0;

            
            var allAnimals = farm.getAllFarmAnimals();
            foreach (StardewValley.Buildings.Building building in farm.buildings)
            {
                if(building is StardewValley.Buildings.Coop || building is StardewValley.Buildings.Barn)
                {
                    var cl = new CoopLoc(building);
                    coops.Add(cl);
                    foreach (FarmAnimal currentAnimal in allAnimals)
                    {
                        if (currentAnimal.home == building)
                        {
                            cl.addAnimal(currentAnimal);
                        }
                    }
                    cl.update();
                    nUncollectedEggs += cl.nUncollectedEggs;
                    nNotPetted += cl.nNotPetted;
                    nNotFed += cl.nNotFed;
                    nNotMilked += cl.nNotMilked;
                }


            }
        }
        

        public class CoopLoc
        {
            StardewValley.Buildings.Building coopBuilding;
            public AnimalHouse coop;
            List<FarmAnimal> inhabitants = new List<FarmAnimal>();
            public int nUncollectedEggs = 0;
            public int nNotPetted = 0;
            public int nNotFed = 0;
            public int nNotMilked = 0;

            public CoopLoc(StardewValley.Buildings.Building coopBuilding)
            {
                this.coopBuilding = coopBuilding;
                coop = (AnimalHouse)coopBuilding.indoors;
                //var allAnimals = StardewValley.Farm.getA
                
            }
            public void update()
            {
                nUncollectedEggs = 0;              
                nNotFed = 0;
                nNotPetted = 0;
                nNotMilked = 0;

                int nHey = 0;
                for (int i=0; i<coop.Objects.Count; i++)
                {
                    var currentObject = coop.Objects.ElementAt(i).Value;
                    if (currentObject.isSpawnedObject)
                    {
                        nUncollectedEggs++;
                    }
                    if (currentObject.Name.Equals("Hay"))
                    {
                        nHey++;
                    }                                        
                }
                this.nNotFed = inhabitants.Count - nHey;

                foreach(FarmAnimal fa in inhabitants)
                {
                    if (!fa.wasPet) nNotPetted++;
                    if (fa.currentProduce > 0 && fa.toolUsedForHarvest.Equals("Milk Pail")) nNotMilked++;
                }
                
            }
            public void addAnimal(FarmAnimal farmAnimal)
            {
                inhabitants.Add(farmAnimal);
            }
    }
    }


    public class CrabTrapList
    {
        public List<CrabTrapsLoc> crabTrapsLoc = new List<CrabTrapsLoc>();
        public int nNeedAction;
        public int nTotal;
        
        public CrabTrapList()
        {
            foreach(GameLocation loc in Game1.locations)
            {
                crabTrapsLoc.Add(new CrabTrapsLoc(loc));

            }
        }
        public void updateAll()
        {
            nTotal = 0;
            nNeedAction = 0;
            foreach (CrabTrapsLoc ctl in crabTrapsLoc)
            {
                ctl.update();
                nNeedAction += ctl.nNeedAction;
                nTotal += ctl.nTotal;

            }
        }

        public class CrabTrapsLoc
        {
            public List<CrabPot> crabTraps;
            public List<CrabPot> crabTrapsReadyForHarvest;
            public List<CrabPot> crabTrapsNotBaited;
            public GameLocation loc;

            public int nNeedAction;
            public int nTotal;

            public CrabTrapsLoc(GameLocation loc)
            {
                this.loc = loc;
            }
            public void update()
            {
                nNeedAction = 0;
                nTotal = 0;

                crabTraps = new List<CrabPot>();
                crabTrapsReadyForHarvest = new List<CrabPot>();
                crabTrapsNotBaited = new List<CrabPot>();
                

                var locObjects = loc.Objects;
                foreach (StardewValley.Object o in locObjects.Values)
                {
                    if (o is CrabPot)
                    {
                        CrabPot currentCrabPot = (CrabPot)o;
                        crabTraps.Add(currentCrabPot);
                        nTotal++;
                        if (currentCrabPot.readyForHarvest)
                        {                          
                            crabTrapsReadyForHarvest.Add(currentCrabPot);
                            nNeedAction++;
                        }
                        if (currentCrabPot.bait == null)
                        {
                            crabTrapsNotBaited.Add(currentCrabPot);
                            nNeedAction++;
                        }
                    }
                }

            }
        }
    }
    public class CropList
    {
        public List<Crop> crops;
        public List<bool> watered;
        public List<Vector2> cropLocations;
        public List<Vector2> unwateredCropLocations;
        public List<int> cropTypes;
        private List<Crop> cropsUnique;
        public List<CropStruct> cropStructs;
        public int[] numberOfCrops;

        public CropList()
        {
           
        }

        public void update(Farm f)
        {
            var allTerrainFeatures = f.terrainFeatures;
            int cropCounter = 0;
            int waterCount = 0;

            crops = new List<Crop>();
            cropLocations = new List<Vector2>();
            unwateredCropLocations = new List<Vector2>();
            watered = new List<bool>();
            cropTypes = new List<int>();
            cropsUnique = new List<Crop>();
            numberOfCrops = new int[1000];

            Array.Clear(numberOfCrops, 0, numberOfCrops.Length);

            foreach (KeyValuePair<Vector2, StardewValley.TerrainFeatures.TerrainFeature> entry in allTerrainFeatures)
            {
                var v = entry.Value;
                var k = entry.Key;

                if (v is StardewValley.TerrainFeatures.HoeDirt)
                {
                    var hoeDirt = (StardewValley.TerrainFeatures.HoeDirt)v;
                    var crop = hoeDirt.crop;
                    if (crop != null)
                    {
                        crops.Add(crop);
                        cropCounter++;
                        cropLocations.Add(k);
                        numberOfCrops[crop.rowInSpriteSheet]++;
                        if (!cropTypes.Contains(crop.rowInSpriteSheet))
                        {
                            cropTypes.Add(crop.rowInSpriteSheet);
                            cropsUnique.Add(crop);
                        }
                        var isWatered = hoeDirt.state == StardewValley.TerrainFeatures.HoeDirt.watered;
                        if (isWatered)
                        {
                            watered.Add(true);
                            waterCount++;
                        }
                        else
                        {
                            watered.Add(false);
                            unwateredCropLocations.Add(k);
                        }

                    }

                }

            }
            updateCropStruct();
        }

        private void updateCropStruct()
        {
            cropStructs = new List<CropStruct>();
            //cropsUnique = new List<Crop>();
            foreach (Crop cropU in cropsUnique)
            {
                CropStruct cs = new CropStruct();
                cs.crops = new List<Crop>();
                cs.watered = new List<bool>();
                cs.locations = new List<Vector2>();
                cs.locationUnwatered = new List<Vector2>();

                cs.uniqueCrop = cropU;
                var i = 0;
                foreach (Crop crop in crops)
                {
                    if (crop.rowInSpriteSheet == cropU.rowInSpriteSheet)
                    {
                        cs.crops.Add(crops[i]);
                        cs.locations.Add(cropLocations[i]);
                        cs.count++;
                        if (watered[i])
                        {
                            cs.watered.Add(true);
                        }
                        else
                        {
                            cs.countUnwatered++;
                            cs.watered.Add(false);
                            cs.locationUnwatered.Add(cropLocations[i]);
                        }
                        i++;
                    }
                }
                cropStructs.Add(cs);

            }
        }
     
    }
    public struct CropStruct
    {
        public Crop uniqueCrop;
        public List<Crop> crops;
        public List<bool> watered;
        public int count;
        public int countUnwatered;
        public List<Vector2> locations;
        public List<Vector2> locationUnwatered;

    }

}

