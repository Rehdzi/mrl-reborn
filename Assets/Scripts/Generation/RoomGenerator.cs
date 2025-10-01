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
	[SerializeField, 
     Tooltip("Если зазор между комнатами (в тайлах) меньше или равен, коридор не создается")] 
    public int minCorridorGapTiles = 5;
    [SerializeField] public float roomHeight = 5f;
    [SerializeField] public Material placeholderMaterial;

	readonly List<RectangleRoom> rooms = new();
	readonly List<Vector2Int> roomCenters = new();
	readonly Dictionary<Vector2Int, RectInt> centerToRect = new();
    Transform roomsRoot;
    Transform corridorsRoot;

    public void GenerateRooms()
    {
        var mapGen = MapGenerator.instance;
        if (mapGen == null) return;

        EnsureRoot();
        ClearPlaceholders();

        // Получаем адаптированные параметры генерации
        var generationParams = GetAdaptedGenerationParameters(mapGen);

        var rng = new System.Random(mapGen.seed.GetHashCode());
        int tries = 0;
		while (rooms.Count < generationParams.roomsCount && tries < generationParams.placementAttempts)
        {
            tries++;
            int w = rng.Next(generationParams.roomSizeMin.x, generationParams.roomSizeMax.x + 1);
            int h = rng.Next(generationParams.roomSizeMin.y, generationParams.roomSizeMax.y + 1);
            int x = rng.Next(2, mapGen.width - w - 2);
            int y = rng.Next(2, mapGen.height - h - 2);
            var rect = new RectInt(x, y, w, h);
            if (IntersectsAny(rect, 2)) continue;

			var room = new RectangleRoom(rect);
			rooms.Add(room);
			room.Carve(mapGen);
			room.CreatePlaceholder(roomsRoot, generationParams.caveSquareSize, generationParams.roomHeight, placeholderMaterial, mapGen.width, mapGen.height);
        }

		// Rebuild centers and lookup map from rooms for corridor generation/trim
		roomCenters.Clear();
		centerToRect.Clear();
		for (int i = 0; i < rooms.Count; i++)
		{
			var c = rooms[i].Center;
			roomCenters.Add(c);
			centerToRect[c] = rooms[i].rect;
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
				// Skip corridor if rooms are too close to each other
				var rectA = centerToRect[roomCenters[bestA]];
				var rectB = centerToRect[roomCenters[bestB]];
				int gapTiles = ComputeRectGapTiles(rectA, rectB);
				if (gapTiles > minCorridorGapTiles)
				{
					mapGen.CarveCorridor(roomCenters[bestA], roomCenters[bestB], generationParams.corridorRadius, generationParams.mergeThreshold);
					CreateCorridorPlaceholder(roomCenters[bestA], roomCenters[bestB], generationParams.caveSquareSize, generationParams.roomHeight, generationParams.corridorRadius);
				}
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
        if (corridorsRoot == null)
        {
            var go = GameObject.Find("CorridorsRoot") ?? new GameObject("CorridorsRoot");
            corridorsRoot = go.transform;
            corridorsRoot.SetParent(transform, false);
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
		rooms.Clear();
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
		foreach (var r in rooms)
        {
			if (RectOverlap(expanded, r.rect)) return true;
        }
        return false;
    }

    static bool RectOverlap(RectInt a, RectInt b)
    {
        return a.xMin < b.xMax && a.xMax > b.xMin && a.yMin < b.yMax && a.yMax > b.yMin;
    }

		// Минимальный зазор между двумя прямоугольниками комнат в тайлах (0, если касаются/перекрываются)
		static int ComputeRectGapTiles(RectInt a, RectInt b)
		{
			int dx = 0;
			if (a.xMax < b.xMin) dx = b.xMin - a.xMax; // a слева от b
			else if (b.xMax < a.xMin) dx = a.xMin - b.xMax; // b слева от a

			int dy = 0;
			if (a.yMax < b.yMin) dy = b.yMin - a.yMax; // a ниже b (в сетке y растет вверх)
			else if (b.yMax < a.yMin) dy = a.yMin - b.yMax; // b ниже a

			// Минимальное евклидово расстояние между прямоугольниками в тайлах.
			// Для порога разумно использовать max(dx, dy) как «зазор по клеткам».
			return Mathf.Max(dx, dy);
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

	class RectangleRoom
	{
		public bool IsStartRoom;
		public bool IsEndRoom;
		public RectInt rect;
		public Vector2Int Center => new Vector2Int(rect.x + rect.width / 2, rect.y + rect.height / 2);

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
    
    // Optional manual trigger
    void Update()
    {
        
    }
}
}

