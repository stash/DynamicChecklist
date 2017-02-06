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
    class ObjectCollection
    {
        public List<StardewValley.TerrainFeatures.HoeDirt> hoeDirts;
        public List<Crop> crops;
        public List<Vector2> cropLocations;
        public ObjectCollection()
        {
            var c = hoeDirts.ElementAt(1).crop;
        }

        public void updateCropList(Farm f)
        {
            {
                var allTerrainFeatures = f.terrainFeatures;
                int cropCounter = 0;
                int waterCount = 0;
                var unwateredCropLocations = new LinkedList<Vector2>();
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
                            var cf = f.isCropAtTile((int)k.X, (int)k.Y);
                            cropCounter++;
                            var isWatered = hoeDirt.state == StardewValley.TerrainFeatures.HoeDirt.watered;
                            if (isWatered)
                            {
                                waterCount++;
                            }
                            else
                            {

                                unwateredCropLocations.AddLast(k);
                            }

                        }

                    }

                }
                
            }
    }
}
