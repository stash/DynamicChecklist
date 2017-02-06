using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using StardewValley.Objects;
using StardewValley;
using Microsoft.Xna.Framework;

namespace DynamicChecklist
{
    public class ObjectCollection
    {
        public List<StardewValley.TerrainFeatures.HoeDirt> hoeDirts;
        public CropList cropList;

        public ObjectCollection()
        {
            cropList = new CropList();
        }

    }
    public class CropList
    {
        public List<Crop> crops;
        public List<Vector2> cropLocations;
        public List<Vector2> unwateredCropLocations;
        public List<int> cropTypes = new List<int>();
        public int[] numberOfCrops = new int[1000];

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
                        }
                        var isWatered = hoeDirt.state == StardewValley.TerrainFeatures.HoeDirt.watered;
                        if (isWatered)
                        {
                            waterCount++;
                        }
                        else
                        {
                            unwateredCropLocations.Add(k);
                        }

                    }

                }

            }
        }
        public void getCropTypes()
        {
            foreach (Crop c in crops)
            {

            }

        }
    }

}

