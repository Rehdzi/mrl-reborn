using UnityEngine;

namespace Generation.RoomGen
{
    public class RectangleRoom
    {
        public bool IsStartRoom;
        public bool IsEndRoom;
        public RectInt rect;
        public Vector2Int Center => new Vector2Int(rect.x + rect.width / 2, rect.y + rect.height / 2);

        public Wall[] walls;
		
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

        public void CreatePlaceholder(Transform roomsRoot, float squareSize, float height, Material placeholderMaterial, int mapTilesWidth, int mapTilesHeight)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"Room_{rect.x}_{rect.y}";
            go.transform.SetParent(roomsRoot, false);
            float w = rect.width * squareSize;
            float h = rect.height * squareSize;
            go.transform.localScale = new Vector3(w, height, h);
            float mapW = mapTilesWidth * squareSize;
            float mapH = mapTilesHeight * squareSize;
            float cx = -mapW/2f + (rect.x + rect.width/2f) * squareSize;
            float cz = -mapH/2f + (rect.y + rect.height/2f) * squareSize;
            go.transform.localPosition = new Vector3(cx, height/2f, cz);
            var mr = go.GetComponent<MeshRenderer>();
            if (placeholderMaterial != null) mr.sharedMaterial = placeholderMaterial;
        }
    }
}