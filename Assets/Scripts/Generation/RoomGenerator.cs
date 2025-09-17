using System.Collections.Generic;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    [Header("Rooms Placement")]
    [SerializeField, Min(1)] public int roomsCount = 7;
    [SerializeField] public Vector2Int roomSizeMin = new Vector2Int(6, 6);
    [SerializeField] public Vector2Int roomSizeMax = new Vector2Int(14, 14);
    [SerializeField] public int placementAttempts = 40;
    [SerializeField] public int corridorRadius = 2;
    [SerializeField] public int mergeThreshold = 3;
    [SerializeField] public float roomHeight = 5f;
    [SerializeField] public Material placeholderMaterial;

    readonly List<RectInt> placedRooms = new();
    readonly List<Vector2Int> roomCenters = new();
    Transform roomsRoot;

    public void GenerateRooms()
    {
        var mapGen = GetComponent<MapGenerator>();
        if (mapGen == null) return;

        EnsureRoot();
        ClearPlaceholders();

        var rng = new System.Random(mapGen.seed.GetHashCode());
        int tries = 0;
        while (placedRooms.Count < roomsCount && tries < placementAttempts)
        {
            tries++;
            int w = rng.Next(roomSizeMin.x, roomSizeMax.x + 1);
            int h = rng.Next(roomSizeMin.y, roomSizeMax.y + 1);
            int x = rng.Next(2, mapGen.width - w - 2);
            int y = rng.Next(2, mapGen.height - h - 2);
            var rect = new RectInt(x, y, w, h);
            if (IntersectsAny(rect, 2)) continue;

            placedRooms.Add(rect);
            roomCenters.Add(new Vector2Int(x + w / 2, y + h / 2));
            mapGen.CarveRectangle(x, y, w, h);
            CreatePlaceholder(rect, mapGen.caveSquareSize, roomHeight);
        }

        // Connect rooms with corridors using MST-like greedy
        if (roomCenters.Count > 1)
        {
            // Prim's algorithm
            var inTree = new HashSet<int> { 0 };
            while (inTree.Count < roomCenters.Count)
            {
                float best = float.MaxValue;
                int bestA = -1, bestB = -1;
                foreach (int a in inTree)
                {
                    for (int b = 0; b < roomCenters.Count; b++)
                    {
                        if (inTree.Contains(b)) continue;
                        float d = (roomCenters[a] - roomCenters[b]).sqrMagnitude;
                        if (d < best)
                        {
                            best = d; bestA = a; bestB = b;
                        }
                    }
                }
                if (bestA == -1 || bestB == -1) break;
                mapGen.CarveCorridor(roomCenters[bestA], roomCenters[bestB], corridorRadius, mergeThreshold);
                inTree.Add(bestB);
            }
        }

        mapGen.RebuildMesh();
    }

    void EnsureRoot()
    {
        if (roomsRoot == null)
        {
            var go = GameObject.Find("RoomsRoot") ?? new GameObject("RoomsRoot");
            roomsRoot = go.transform;
            roomsRoot.SetParent(transform, false);
        }
    }

    void ClearPlaceholders()
    {
        if (roomsRoot == null) return;
        for (int i = roomsRoot.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(roomsRoot.GetChild(i).gameObject);
        }
        placedRooms.Clear();
        roomCenters.Clear();
    }

    void CreatePlaceholder(RectInt rect, float squareSize, float height)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = $"Room_{rect.x}_{rect.y}";
        go.transform.SetParent(roomsRoot, false);
        float w = rect.width * squareSize;
        float h = rect.height * squareSize;
        go.transform.localScale = new Vector3(w, height, h);
        // Position: convert grid center to local world used by MapGenerator/MeshGenerator (-map/2 offset)
        var mapGen = GetComponent<MapGenerator>();
        float mapW = mapGen.width * squareSize;
        float mapH = mapGen.height * squareSize;
        float cx = -mapW/2f + (rect.x + rect.width/2f) * squareSize;
        float cz = -mapH/2f + (rect.y + rect.height/2f) * squareSize;
        go.transform.localPosition = new Vector3(cx, height/2f, cz);
        var mr = go.GetComponent<MeshRenderer>();
        if (placeholderMaterial != null) mr.sharedMaterial = placeholderMaterial;
    }

    bool IntersectsAny(RectInt rect, int padding)
    {
        var expanded = new RectInt(rect.x - padding, rect.y - padding, rect.width + padding*2, rect.height + padding*2);
        foreach (var r in placedRooms)
        {
            if (RectOverlap(expanded, r)) return true;
        }
        return false;
    }

    static bool RectOverlap(RectInt a, RectInt b)
    {
        return a.xMin < b.xMax && a.xMax > b.xMin && a.yMin < b.yMax && a.yMax > b.yMin;
    }

    // Optional manual trigger
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateRooms();
        }
    }
}
