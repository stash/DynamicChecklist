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
        public Texture2D cropSpriteSheet;
        
        public ObjectCollection(Texture2D cropSpriteSheet)
        {
            this.cropSpriteSheet = cropSpriteSheet;
            cropList = new CropList();
            crabTrapList = new CrabTrapList();
        }

    }
    public class CrabTrapList
    {
        public List<CrabPot> crabTraps;
        public List<CrabTrapsLoc> crabTrapsLoc = new List<CrabTrapsLoc>();
        
        public CrabTrapList()
        {
            foreach(GameLocation loc in Game1.locations)
            {
                crabTrapsLoc.Add(new CrabTrapsLoc(loc));
            }
        }
        public void update(GameLocation l)
        {
            int nCrabPots = 0;
            int nCrabPotsReadyForHarvest = 0;
            int nCrabPotsNotBaited = 0;
            foreach(StardewValley.Object o in l.Objects.Values)
            {
                if (o is CrabPot)
                {
                    nCrabPots++;
                    CrabPot currentCrabPot = (CrabPot)o;
                    if (currentCrabPot.readyForHarvest)
                    {
                        nCrabPotsReadyForHarvest++;
                    }
                    if (currentCrabPot.bait==null){
                        nCrabPotsNotBaited++;
                    }
                }
            }

        }

        public class CrabTrapsLoc
        {
            public List<CrabPot> crabTraps;
            public GameLocation loc;

            public CrabTrapsLoc(GameLocation loc)
            {
                this.loc = loc;
            }
            public void update()
            {
                var locObjects = loc.Objects;

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

