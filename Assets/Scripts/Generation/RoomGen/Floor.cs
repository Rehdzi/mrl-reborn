namespace Generation.RoomGen
{
    public class Floor
    {
        public int FloorNumber { get; private set; }

        public RectangleRoom[,] rooms;

        public Floor(int floorNumber, RectangleRoom[,] rooms)
        {
            FloorNumber = floorNumber;
            this.rooms = rooms;
        }
    }
}