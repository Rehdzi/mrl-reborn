namespace Generation.RoomGen
{
    public class Wall
    {
        public enum WallType
        {
            Normal
        }
        
        public WallType TypeSelected { get; private set; } = WallType.Normal;

        public Wall(WallType type = WallType.Normal)
        {
            this.TypeSelected = type;
        }
    }
}