using UnityEngine;

namespace Generation.RoomGen
{
    public class WFCRoom : MonoBehaviour
    {
        public GameObject floorPrefab;
        public GameObject wallPrefab;

        private int numberOfFloors = 1;
        private float cellUnitSize;
        private Floor[] floors;
        
        public void GenerateRoom(Vector2Int roomSize)
        {
            cellUnitSize = MapGenerator.instance != null ? MapGenerator.instance.caveSquareSize : 1f;
            
            floors = new Floor[numberOfFloors];

            for (int floorIndex = 0; floorIndex < numberOfFloors; floorIndex++)
            {
                int sizeX = roomSize.x;
                int sizeY = roomSize.y;
                var rooms = new RectangleRoom[sizeX, sizeY];

                for (int x = 0; x < sizeX; x++)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        rooms[x, y] = new RectangleRoom(new Vector2(x * cellUnitSize, y * cellUnitSize));
                    }
                }

                floors[floorIndex] = new Floor(floorIndex, rooms);
            }
        }

        public void RenderRoom(Vector2Int roomSize)
        {
            foreach (Floor floor in floors)
            {
                int sizeX = roomSize.x;
                int sizeY = roomSize.y;
                float half = 0.5f;
                for (int x = 0; x < sizeX; x++)
                {
                    for (int y = 0; y < sizeY; y++)
                    {
                        RectangleRoom room = floor.rooms[x, y];

						// Создаем пол для каждого тайла (локальные координаты относительно комнаты)
						var floorMesh = Instantiate(floorPrefab, transform);
						floorMesh.transform.localPosition = new Vector3(room.RoomPosition.x, floor.FloorNumber, room.RoomPosition.y);
						floorMesh.transform.localRotation = Quaternion.identity;
                        
                        // TODO: Revert rotations on new models
                        // Создаем стены только по границам комнаты
                        if (y == sizeY - 1) // Верхняя стена (Z+)
                        {
							var wall1 = Instantiate(wallPrefab, transform);
							wall1.transform.localPosition = new Vector3(room.RoomPosition.x, floor.FloorNumber, room.RoomPosition.y + half);
							wall1.transform.localRotation = Quaternion.Euler(0, -90, 0);
                        }
                        
                        if (x == sizeX - 1) // Правая стена (X+)
                        {
							var wall2 = Instantiate(wallPrefab, transform);
							wall2.transform.localPosition = new Vector3(room.RoomPosition.x + half, floor.FloorNumber, room.RoomPosition.y);
							wall2.transform.localRotation = Quaternion.Euler(0, 0, 0);
                        }
                        
                        if (y == 0) // Нижняя стена (Z-)
                        {
							var wall3 = Instantiate(wallPrefab, transform);
							wall3.transform.localPosition = new Vector3(room.RoomPosition.x, floor.FloorNumber, room.RoomPosition.y - half);
							wall3.transform.localRotation = Quaternion.Euler(0, 90, 0);
                        }
                        
                        if (x == 0) // Левая стена (X-)
                        {
							var wall4 = Instantiate(wallPrefab, transform);
							wall4.transform.localPosition = new Vector3(room.RoomPosition.x - half, floor.FloorNumber, room.RoomPosition.y);
							wall4.transform.localRotation = Quaternion.Euler(0, -180, 0);
                        }
                    }
                }
            }
        }
    }
}