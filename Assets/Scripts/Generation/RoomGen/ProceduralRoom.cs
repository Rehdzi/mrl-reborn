using UnityEngine;

namespace Generation.RoomGen
{
    public class WFCRoom : MonoBehaviour
    {

        public GameObject floorPrefab;
        public GameObject wallPrefab;
    
        public Vector2Int roomSize;

        private int numberOfFloors = 1;
        private float cellUnitSize;

        private Floor[] floors;
        
        void GenerateRoom()
        {
            if (MapGenerator.instance != null)
            {
                cellUnitSize = MapGenerator.instance.caveSquareSize;
            }
            else
            {
                cellUnitSize = 1;
            }
            
            floors = new Floor[numberOfFloors];

            int floorCount = 0;

            foreach (Floor floor in floors)
            {
                RectangleRoom[,] rooms = new RectangleRoom[roomSize.x, roomSize.y];

                for (int x = 0; x < roomSize.x; x++)
                {
                    for (int y = 0; y < roomSize.y; y++)
                    {
                        rooms[x, y] = new RectangleRoom(new Vector2(x * cellUnitSize, y * cellUnitSize));
                    }
                }

                floors[floorCount] = new Floor(floorCount++, rooms);
            }

        }

        void RenderRoom()
        {
            foreach (Floor floor in floors)
            {
                for (int x = 0; x < roomSize.x; x++)
                {
                    for (int y = 0; y < roomSize.y; y++)
                    {
                        RectangleRoom room = floor.rooms[x, y];

                        var floorMesh = Instantiate(
                            floorPrefab,
                            new Vector3(room.RoomPosition.x, floor.FloorNumber, room.RoomPosition.y),
                            Quaternion.Euler(Vector3.zero));
                        floorMesh.transform.parent = transform;
                        
                        var wall1 = Instantiate(
                            wallPrefab, 
                            new Vector3(room.RoomPosition.x, floor.FloorNumber + 0.5f, room.RoomPosition.y + 0.5f), 
                            Quaternion.Euler(0,0,0));
                        wall1.transform.parent = transform;
                        var wall2 = Instantiate(
                            wallPrefab, 
                            new Vector3(room.RoomPosition.x + 0.5f, floor.FloorNumber + 0.5f, room.RoomPosition.y), 
                            Quaternion.Euler(0,90,0));
                        wall2.transform.parent = transform;
                        var wall3 = Instantiate(
                            wallPrefab, 
                            new Vector3(room.RoomPosition.x, floor.FloorNumber + 0.5f, room.RoomPosition.y - 0.5f), 
                            Quaternion.Euler(0,180,0));
                        wall3.transform.parent = transform;
                        var wall4 = Instantiate(
                            wallPrefab, 
                            new Vector3(room.RoomPosition.x - 0.5f, floor.FloorNumber + 0.5f, room.RoomPosition.y), 
                            Quaternion.Euler(0,-90,0));
                        wall4.transform.parent = transform;
                        
                    }
                }
            }
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            GenerateRoom();
            RenderRoom();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}


