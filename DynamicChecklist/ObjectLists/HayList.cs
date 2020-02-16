namespace DynamicChecklist.ObjectLists
{
    using Microsoft.Xna.Framework;
    using StardewValley;

    public class HayList : ObjectList
    {
        private const int TickOffset = 40;

        public HayList(ModConfig config)
            : base(config)
        {
            this.ImageTexture = GameTexture.Hay;
            this.OptionMenuLabel = "Filled Troughs";
            this.TaskDoneMessage = "All troughs have been filled";
            this.Name = TaskName.Hay;
        }

        protected override void InitializeObjectInfoList()
        {
            foreach (var animalHouse in ListHelper.GetFarmAnimalHouses())
            {
                this.UpdateObjectInfoList(animalHouse);
            }
        }

        protected override void UpdateObjectInfoList(uint ticks)
        {
            // According to wiki, it's possible to exploit removing hey from bench by setting off a bomb!
            // So, just update every second or so for AnimalHouses without an active Farmer
            var houses = (this.TaskDone && ticks % 60 == TickOffset) ? ListHelper.GetFarmAnimalHouses() : ListHelper.GetActiveFarmAnimalHouses();

            foreach (var animalHouse in houses)
            {
                this.UpdateObjectInfoList(animalHouse);
            }
        }

        private void UpdateObjectInfoList(AnimalHouse animalHouse)
        {
            this.ObjectInfoList.RemoveAll(soi => soi.Location == animalHouse);

            var houseWidth = animalHouse.map.Layers[0].LayerWidth;
            var houseHeight = animalHouse.map.Layers[0].LayerHeight;
            for (int tileX = 0; tileX < houseWidth; tileX++)
            {
                for (int tileY = 0; tileY < houseHeight; tileY++)
                {
                    bool tileIsTrough = animalHouse.doesTileHaveProperty(tileX, tileY, "Trough", "Back") != null;
                    if (tileIsTrough)
                    {
                        var coord = new Vector2(tileX, tileY);
                        bool tileHasHay = animalHouse.Objects.ContainsKey(coord);
                        this.ObjectInfoList.Add(new StardewObjectInfo(coord, animalHouse, !tileHasHay));
                    }
                }
            }
        }
    }
}
