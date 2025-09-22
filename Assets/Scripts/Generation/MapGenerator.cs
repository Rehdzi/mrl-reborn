using System;
using System.Collections.Generic;
using Generation;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    [SerializeField, Min(1)] public int width = 100;
    [SerializeField, Min(1)] public int height = 100;

    [SerializeField] public string seed = "";
    [SerializeField] public bool useRandomSeed = true;
    
    [SerializeField, Range(0, 100)] public int randomFillPercent = 45;

    [SerializeField, Min(0)] public int smoothIterations = 5;
    [SerializeField, Min(0)] public int borderSize = 1;
    [SerializeField, Min(0)] public int passageRadius = 1;
    [SerializeField, Min(0.01f)] public float caveSquareSize = 1f;
    [SerializeField, Range(0f, 1f)] public float interpolationThreshold = 0.5f;

    public BiomeData biome;
    
    private int[,] map;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    // void Start()
    // {
    //     GenerateMap();
    // }
    
    public void GenerateMap()
    {
        map =  new int[width, height];
        RandomFillMap();

        for (int i = 0; i < smoothIterations; i++)
        {
            SmoothMap();
        }
        
        ProcessMap();

        var borderedMap = new int[width + borderSize * 2,  height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x -  borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                }
            }
        }
        
        RebuildMesh();
        var roomGen = GetComponent<RoomGenerator>();
        if (roomGen != null)
        {
            roomGen.GenerateRooms();
        }
    }

    public void RebuildMesh()
    {
        if (map == null) return;
        var borderedMap = new int[width + borderSize * 2,  height + borderSize * 2];
        for (int x = 0; x < borderedMap.GetLength(0); x++)
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedMap[x, y] = map[x -  borderSize, y - borderSize];
                }
                else
                {
                    borderedMap[x, y] = 1;
                }
            }
        }
        var meshGen = GetComponent<MeshGenerator>();
        float[,] values = BuildValuesField(map);
        float[,] borderedValues = new float[borderedMap.GetLength(0), borderedMap.GetLength(1)];
        for (int x = 0; x < borderedValues.GetLength(0); x++)
        {
            for (int y = 0; y < borderedValues.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize)
                {
                    borderedValues[x, y] = values[x - borderSize, y - borderSize];
                }
                else
                {
                    borderedValues[x, y] = 1f;
                }
            }
        }
        meshGen.GenerateMesh(borderedMap, borderedValues, caveSquareSize);
    }

    float[,] BuildValuesField(int[,] src)
    {
        int w = src.GetLength(0);
        int h = src.GetLength(1);
        var values = new float[w, h];
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                values[x, y] = src[x, y] == 1 ? 1f : 0f;
            }
        }
        var blurred = new float[w, h];
        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                float sum = 0f;
                int count = 0;
                for (int nx = x - 1; nx <= x + 1; nx++)
                {
                    for (int ny = y - 1; ny <= y + 1; ny++)
                    {
                        if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                        {
                            sum += values[nx, ny];
                            count++;
                        }
                    }
                }
                blurred[x, y] = sum / Mathf.Max(1, count);
            }
        }
        return blurred;
    }

    public bool CarveRectangle(int x, int y, int w, int h)
    {
        bool changed = false;
        int x0 = Mathf.Clamp(x, 0, width - 1);
        int y0 = Mathf.Clamp(y, 0, height - 1);
        int x1 = Mathf.Clamp(x + w - 1, 0, width - 1);
        int y1 = Mathf.Clamp(y + h - 1, 0, height - 1);
        for (int ix = x0; ix <= x1; ix++)
        {
            for (int iy = y0; iy <= y1; iy++)
            {
                if (map[ix, iy] != 0)
                {
                    map[ix, iy] = 0;
                    changed = true;
                }
            }
        }
        return changed;
    }

    public void CarveCorridor(Vector2Int from, Vector2Int to, int radius, int mergeThreshold = 2)
    {
        var line = GetLine(new Coord(from.x, from.y), new Coord(to.x, to.y));
        foreach (var c in line)
        {
            DrawCircle(c, radius);
            if (TryFindNearestOpen(c, mergeThreshold, out var near))
            {
                var bridge = GetLine(c, near);
                foreach (var bc in bridge)
                {
                    DrawCircle(bc, Mathf.Max(1, radius - 1));
                }
            }
        }
    }

    bool TryFindNearestOpen(Coord c, int maxDist, out Coord nearest)
    {
        for (int r = 1; r <= maxDist; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                int dy1 = r - Mathf.Abs(dx);
                int dy2 = -dy1;
                int x1 = c.tileX + dx;
                int y1 = c.tileY + dy1;
                int x2 = c.tileX + dx;
                int y2 = c.tileY + dy2;
                if (IsInMapRange(x1, y1) && map[x1, y1] == 0)
                {
                    nearest = new Coord(x1, y1);
                    return true;
                }
                if (IsInMapRange(x2, y2) && map[x2, y2] == 0)
                {
                    nearest = new Coord(x2, y2);
                    return true;
                }
            }
        }
        nearest = c;
        return false;
    }

    void ProcessMap()
    {
        const int wallTresholdSize = 50;
        const int roomTresholdSize = 50;
        
        var wallRegions = GetRegions(1);
        var roomRegions = GetRegions(0);
        
        foreach (var wallRegion in wallRegions)
        {
            if (wallRegion.Count < wallTresholdSize)
            {
                foreach (var tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }
        
        var survivingRooms = new List<Room>(roomRegions.Count);
        
        
        foreach (var roomRegion in roomRegions)
        {
            if (roomRegion.Count < roomTresholdSize)
            {
                foreach (var tile in roomRegion)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                survivingRooms.Add(new Room(roomRegion, map));
            }
        }
        
        if (survivingRooms.Count > 0)
        {
            survivingRooms.Sort();
            survivingRooms[0].IsMainRoom =  true;
            survivingRooms[0].IsAccessibleFromMainRoom = true;
        }
        // foreach (Room r in survivingRooms)
        // {
        //     print(r.roomSize);               //Display room sizes
        // }
        ConnectClosestRooms(survivingRooms);
    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccess = false)
    {
        List<Room> roomListA;
        List<Room> roomListB;

        if (forceAccess)
        {
            roomListA = new List<Room>(allRooms.Count);
            roomListB = new List<Room>(allRooms.Count);
            foreach (var r in allRooms)
            {
                if (r.IsAccessibleFromMainRoom)
                {
                    roomListB.Add(r);
                }
                else
                {
                    roomListA.Add(r);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }
        
        int bestDistance = 0;
        var bestTileA = new Coord();
        var bestTileB = new Coord();
        
        var bestRoomA = new Room();
        var bestRoomB = new Room();
        bool possibleConnectionFound = false;
        
        foreach (var roomA in roomListA)
        {
            if (!forceAccess)
            {
                possibleConnectionFound = false;
                if (roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }
            
            foreach (var roomB in roomListB)
            {
                if (roomA == roomB || roomA.IsConnected(roomB))
                {
                    continue;
                }
                
                for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++)
                {
                    for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++)
                    {
                        var tileA = roomA.edgeTiles[tileIndexA];
                        var tileB = roomB.edgeTiles[tileIndexB];
                        
                        int dx = tileA.tileX - tileB.tileX;
                        int dy = tileA.tileY - tileB.tileY;
                        int distanceBetweenRooms = dx * dx + dy * dy;

                        if (distanceBetweenRooms < bestDistance || !possibleConnectionFound)
                        {
                            bestDistance = distanceBetweenRooms;
                            possibleConnectionFound = true;
                            bestTileA = tileA;
                            bestTileB = tileB;
                            bestRoomA = roomA;
                            bestRoomB = roomB;
                        }
                    }
                }
            }

            if (possibleConnectionFound && !forceAccess)
            {
                CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            }
        }

        if (possibleConnectionFound && forceAccess)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }
        
        if (!forceAccess)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        Room.ConnectRooms(roomA, roomB);
        
        var line = GetLine(tileA, tileB);
        foreach (var c in line)
        {
            DrawCircle(c, passageRadius);
        }
    }

    void DrawCircle(Coord c, int r)
    {
        for (int x = -r; x <= r; x++)
        {
            for (int y = -r; y <= r; y++)
            {
                if (x * x + y * y <= r * r)
                {
                    int drawX = c.tileX + x;
                    int drawY = c.tileY + y;
                    if (IsInMapRange(drawX, drawY))
                    {
                        map[drawX, drawY] = 0;
                    }
                }
            }
        }
    }
    
    List<Coord> GetLine(Coord from, Coord to)
    {
        var line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;
        
        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);
            
            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
        }

        int gradientAccumulation = longest / 2;
        line.Capacity = longest + 1;
        for (int i = 0; i <= longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                    
                }
                else
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }
        
        return line;
    }
    
    Vector3 CoordToWorldPoint(Coord tile)
    {
        return new Vector3(-width / 2 + .5f + tile.tileX, 2 , -height / 2 + .5f + tile.tileY);
    }
    
    List<List<Coord>> GetRegions(int tileType)
    {
        var regions = new List<List<Coord>>();
        var mapFlags = new int[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    var newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    foreach (var tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }
        
        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        var regionTiles = new List<Coord>();
        
        var mapFlags = new int[width, height];
        int tileType = map[startX, startY];
        
        var queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;

        while (queue.Count > 0)
        {
            var tile = queue.Dequeue();
            regionTiles.Add(tile);

            for (int x = tile.tileX -1; x <= tile.tileX + 1; x++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }
        
        return regionTiles;
    }

    bool IsInMapRange(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
    
    void RandomFillMap()
    {
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }

        var prng = new System.Random(seed.GetHashCode());

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    map[x, y] = 1;
                }
                else
                {
                    map[x, y] = prng.Next(0, 100) < randomFillPercent ? 1 : 0;
                }
            }
        }
    }

    void SmoothMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neigbourWallTiles = GetSurroundingWallCount(x, y);

                if (neigbourWallTiles > 4)
                {
                    map[x, y] = 1;
                } else if (neigbourWallTiles < 4)
                {
                    map[x, y] = 0;
                }
            }
        }
    }

    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY))
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                        wallCount += map[neighbourX, neighbourY];
                    }
                }
                else
                {
                    wallCount++;
                }

                
            }
        }
        return wallCount;
    }

    public struct Coord
    {
        public int tileX;
        public int tileY;

        public Coord(int x, int y)
        {
            tileX = x;
            tileY = y;
        }
    }
    
    
    class Room : IComparable<Room>
    {
        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;

        public bool IsAccessibleFromMainRoom;
        public bool IsMainRoom;

        public int roomSize;

        public Room(){}
        
        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();
            
            
            edgeTiles = new List<Coord>();

            foreach (Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (map[x, y] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public void SetAccessibleFromMainRoom()
        {
            if (!IsAccessibleFromMainRoom)
            {
                IsAccessibleFromMainRoom = true;
                foreach (Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            if (roomA.IsAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            } else if (roomB.IsAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }
            
            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            return  connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            return otherRoom.roomSize.CompareTo(roomSize);
        }
    }
    
    
    // void OnDrawGizmos()
    // {
    //     if (map != null)
    //     {
    //         for (int x = 0; x < width; x++)
    //         {
    //             for (int y = 0; y < height; y++)
    //             {
    //                 Gizmos.color = map[x, y] == 1? Color.green : Color.red;
    //                 Vector3 pos = new Vector3(-width/2 + x + .5f, 0, -height/2 + y + .5f);
    //                 Gizmos.DrawCube(pos, Vector3.one);
    //             }
    //         }
    //     }
    // }
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GenerateMap();
        }
    }
}
