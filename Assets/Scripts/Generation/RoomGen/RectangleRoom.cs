using UnityEngine;

namespace Generation.RoomGen
{
    public class RectangleRoom
    {
        public bool IsStartRoom;
        public bool IsEndRoom;
        public RectInt rect;
        public Vector2Int Center => new Vector2Int(rect.x + rect.width / 2, rect.y + rect.height / 2);

		
        private Vector2 position;

        public RectangleRoom(Vector2 position)
        {
            this.position = position;
        }

        public Vector2 RoomPosition
        {
            get
            {
                return this.position;
            }
        }
        
        public RectangleRoom(RectInt rect)
        {
            this.rect = rect;
        }

        public void Carve(MapGenerator mapGen)
        {
            mapGen.CarveRectangle(rect.x, rect.y, rect.width, rect.height);
        }

        public void CreateWFCRoom(Transform roomsRoot, float squareSize, int mapTilesWidth, int mapTilesHeight, GameObject floorPrefab, GameObject wallPrefab)
        {
            // Parent container
            var go = new GameObject($"WFCRoom_{rect.x}_{rect.y}");
            if (roomsRoot != null)
            {
                go.transform.SetParent(roomsRoot, false);
            }

            // Position the room's local origin at the top-left tile center of this rectangle in world space
            float mapW = mapTilesWidth * squareSize;
            float mapH = mapTilesHeight * squareSize;
            float originX = -mapW/2f + rect.x * squareSize;
            float originZ = -mapH/2f + rect.y * squareSize;
            go.transform.localPosition = new Vector3(originX, 0f, originZ);

            // Add WFC room component and assign prefabs
            var wfcRoom = go.AddComponent<WFCRoom>();
            wfcRoom.floorPrefab = floorPrefab;
            wfcRoom.wallPrefab = wallPrefab;

            // Generate and render with the rectangle's size in tiles
            var roomSize = new Vector2Int(rect.width, rect.height);
            wfcRoom.GenerateRoom(roomSize);
            wfcRoom.RenderRoom(roomSize);
        }
    }
}