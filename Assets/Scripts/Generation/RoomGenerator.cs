using System.Collections.Generic;
using UnityEngine;

namespace Generation
{
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
    readonly Dictionary<Vector2Int, RectInt> centerToRect = new();
    Transform roomsRoot;
    Transform corridorsRoot;

    public void GenerateRooms()
    {
        var mapGen = GetComponent<MapGenerator>();
        if (mapGen == null) return;

        EnsureRoot();
        ClearPlaceholders();
        mapGen.ClearGeneratedRooms();

        // Получаем адаптированные параметры генерации
        var generationParams = GetAdaptedGenerationParameters(mapGen);

        var rng = new System.Random(mapGen.seed.GetHashCode());
        int tries = 0;
        while (placedRooms.Count < generationParams.roomsCount && tries < generationParams.placementAttempts)
        {
            tries++;
            int w = rng.Next(generationParams.roomSizeMin.x, generationParams.roomSizeMax.x + 1);
            int h = rng.Next(generationParams.roomSizeMin.y, generationParams.roomSizeMax.y + 1);
            int x = rng.Next(2, mapGen.width - w - 2);
            int y = rng.Next(2, mapGen.height - h - 2);
            var rect = new RectInt(x, y, w, h);
            if (IntersectsAny(rect, 2)) continue;

            placedRooms.Add(rect);
            var center = new Vector2Int(x + w / 2, y + h / 2);
            roomCenters.Add(center);
            centerToRect[center] = rect;
            mapGen.CarveRectangle(x, y, w, h);
            mapGen.CreateRoomFromRect(rect, generationParams.caveSquareSize, generationParams.roomHeight, placeholderMaterial, roomsRoot);
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
                mapGen.CarveCorridor(roomCenters[bestA], roomCenters[bestB], generationParams.corridorRadius, generationParams.mergeThreshold);
                CreateCorridorPlaceholder(roomCenters[bestA], roomCenters[bestB], generationParams.caveSquareSize, generationParams.roomHeight, generationParams.corridorRadius);
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
            roomsRoot.localPosition = Vector3.zero;
            roomsRoot.localRotation = Quaternion.identity;
            roomsRoot.localScale = Vector3.one;
        }
        if (corridorsRoot == null)
        {
            var go = GameObject.Find("CorridorsRoot") ?? new GameObject("CorridorsRoot");
            corridorsRoot = go.transform;
            corridorsRoot.SetParent(transform, false);
            corridorsRoot.localPosition = Vector3.zero;
            corridorsRoot.localRotation = Quaternion.identity;
            corridorsRoot.localScale = Vector3.one;
        }
    }

    void ClearPlaceholders()
    {
        if (roomsRoot != null)
        {
            for (int i = roomsRoot.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(roomsRoot.GetChild(i).gameObject);
            }
        }
        if (corridorsRoot != null)
        {
            for (int i = corridorsRoot.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(corridorsRoot.GetChild(i).gameObject);
            }
        }
        placedRooms.Clear();
        roomCenters.Clear();
        centerToRect.Clear();
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

    void CreateCorridorPlaceholder(Vector2Int from, Vector2Int to, float squareSize, float height, int radius)
    {
        var mapGen = GetComponent<MapGenerator>();
        float mapW = mapGen.width * squareSize;
        float mapH = mapGen.height * squareSize;

        Vector3 a = new Vector3(-mapW/2f + from.x * squareSize, 0f, -mapH/2f + from.y * squareSize);
        Vector3 b = new Vector3(-mapW/2f + to.x * squareSize, 0f, -mapH/2f + to.y * squareSize);
        Vector3 dir = (b - a);
        float length = dir.magnitude;
        if (length < 1e-4f) return;

        Vector2 dirXZ = new Vector2(dir.x, dir.z).normalized;

        // Trim corridor so it doesn't intersect room boxes
        float thickness = Mathf.Max(squareSize, radius * 2f * squareSize);
        float halfThickness = thickness * 0.5f;

        float trimA = ComputeTrimDistance(from, dirXZ, squareSize) + halfThickness;
        float trimB = ComputeTrimDistance(to, -dirXZ, squareSize) + halfThickness;

        float usable = Mathf.Max(0f, length - (trimA + trimB));
        if (usable <= 1e-3f) return;

        Vector3 newA = a + new Vector3(dirXZ.x, 0f, dirXZ.y) * trimA;
        Vector3 newB = b - new Vector3(dirXZ.x, 0f, dirXZ.y) * trimB;
        Vector3 mid = (newA + newB) * 0.5f;

        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = $"Corridor_{from.x}_{from.y}_{to.x}_{to.y}";
        go.transform.SetParent(corridorsRoot, false);

        go.transform.localScale = new Vector3(usable, height, thickness);
        go.transform.localPosition = new Vector3(mid.x, height/2f, mid.z);
        go.transform.localRotation = Quaternion.LookRotation(new Vector3(dir.x, 0f, dir.z), Vector3.up) * Quaternion.Euler(0f, 90f, 0f);

        var mr = go.GetComponent<MeshRenderer>();
        if (placeholderMaterial != null) mr.sharedMaterial = placeholderMaterial;
    }

    float ComputeTrimDistance(Vector2Int center, Vector2 dirNorm, float squareSize)
    {
        if (!centerToRect.TryGetValue(center, out var rect))
        {
            return 0f;
        }
        float ex = rect.width * 0.5f * squareSize;
        float ez = rect.height * 0.5f * squareSize;
        float dx = Mathf.Abs(dirNorm.x);
        float dz = Mathf.Abs(dirNorm.y);
        float tx = dx > 1e-4f ? ex / dx : float.PositiveInfinity;
        float tz = dz > 1e-4f ? ez / dz : float.PositiveInfinity;
        return Mathf.Min(tx, tz);
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

    /// <summary>
    /// Получает адаптированные параметры генерации на основе текущего биома
    /// </summary>
    private LevelGenerationParameters GetAdaptedGenerationParameters(MapGenerator mapGen)
    {
        var parameters = new LevelGenerationParameters
        {
            roomsCount = this.roomsCount,
            roomSizeMin = this.roomSizeMin,
            roomSizeMax = this.roomSizeMax,
            placementAttempts = this.placementAttempts,
            corridorRadius = this.corridorRadius,
            mergeThreshold = this.mergeThreshold,
            roomHeight = this.roomHeight,
            caveSquareSize = mapGen.caveSquareSize
        };
        
        // Применяем модификаторы биома если включено
        var level = Level.levelInstance;
        if (level?.environment != null)
        {
            BiomeType biomeType = level.environment.GetBiomeType();
            var modifiers = BiomeGenerationAdapter.GetModifiersForBiome(biomeType);
            parameters.ApplyModifiers(modifiers);
        }
        
        return parameters;
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
}

