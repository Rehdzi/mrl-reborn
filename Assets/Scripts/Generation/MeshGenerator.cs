using UnityEngine;
using System.Collections.Generic;

public class MeshGenerator : MonoBehaviour
{
    [SerializeField] public MeshFilter walls;
    [SerializeField] public MeshFilter cave;
    [SerializeField] public bool is2D;
    [SerializeField] public float wallHeight = 5f;
    [SerializeField] public int tilingSize = 10;

    public SquareGrid squareGrid;
    
    List<Vector3> vertices;
    List<int> triangles;
    
    Dictionary<int, List<Triangle>> triangleMap =  new();
    List<List<int>> outlines = new();
    HashSet<int> checkedVertices = new();
    
    public void GenerateMesh(int[,] map, float squareSize)
    {
        GenerateMesh(map, null, squareSize);
    }

    public void GenerateMesh(int[,] map, float[,] values, float squareSize)
    {
        triangleMap.Clear();
        outlines.Clear();
        checkedVertices.Clear();
        
        squareGrid = values == null ? new SquareGrid(map, squareSize) : new SquareGrid(map, values, squareSize);
        
        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y]);
            }
        }
        
        var mesh = new Mesh { indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 };
        cave.mesh = mesh;
        
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0, true);
        
        var uvs = new List<Vector2>(vertices.Count);
        for (int i = 0; i < vertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0)*squareSize, 
                map.GetLength(0)*squareSize, 
                vertices[i].x) * tilingSize;
            float percentY = Mathf.InverseLerp(-map.GetLength(0)*squareSize, 
                map.GetLength(0)*squareSize, 
                vertices[i].z)  * tilingSize;
            
            uvs.Add(new Vector2(percentX, percentY));
        }
        
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        
        if (!is2D)
        {
            CreateWallMesh();
        }
    }

    private void CreateWallMesh()
    {
        CalculateMeshOutlines();
        
        var wallVertices = new List<Vector3>();
        var wallTriangles = new List<int>();
        var wallUVs = new List<Vector2>();
        var wallMesh =  new Mesh();

        foreach (List<int> outline in outlines)
        {
            // Create a complete wall strip for each outline
            var outlineVertices = new List<Vector3>();
            var outlineTriangles = new List<int>();
            var outlineUVs = new List<Vector2>();
            
            float uAccumulated = 0f;
            for (int x = 0; x < outline.Count - 1; x++)
            {
                Vector3 topLeftPos = vertices[outline[x]];
                Vector3 topRightPos = vertices[outline[x+1]];
                Vector3 bottomLeftPos = topLeftPos - Vector3.up * wallHeight;
                Vector3 bottomRightPos = topRightPos - Vector3.up * wallHeight;

                float segmentLength = Vector3.Distance(topLeftPos, topRightPos);
                float uNext = uAccumulated + segmentLength * tilingSize;
                float vTop = 0f;
                float vBottom = wallHeight * tilingSize;

                // Add vertices for this quad
                int baseIndex = outlineVertices.Count;
                outlineVertices.Add(topLeftPos);
                outlineVertices.Add(topRightPos);
                outlineVertices.Add(bottomLeftPos);
                outlineVertices.Add(bottomRightPos);

                // Add triangles (clockwise winding for outward-facing normals)
                outlineTriangles.Add(baseIndex + 0); // topLeft
                outlineTriangles.Add(baseIndex + 2); // bottomLeft
                outlineTriangles.Add(baseIndex + 1); // topRight

                outlineTriangles.Add(baseIndex + 1); // topRight
                outlineTriangles.Add(baseIndex + 2); // bottomLeft
                outlineTriangles.Add(baseIndex + 3); // bottomRight

                // Add UVs
                outlineUVs.Add(new Vector2(uAccumulated, vTop));
                outlineUVs.Add(new Vector2(uNext, vTop));
                outlineUVs.Add(new Vector2(uAccumulated, vBottom));
                outlineUVs.Add(new Vector2(uNext, vBottom));

                uAccumulated = uNext;
            }
            
            // Add this outline's data to the main wall mesh
            int vertexOffset = wallVertices.Count;
            wallVertices.AddRange(outlineVertices);
            wallUVs.AddRange(outlineUVs);
            
            // Adjust triangle indices for the global vertex array
            for (int i = 0; i < outlineTriangles.Count; i++)
            {
                wallTriangles.Add(outlineTriangles[i] + vertexOffset);
            }
        }
        
        wallMesh.SetVertices(wallVertices);
        wallMesh.SetTriangles(wallTriangles, 0, true);
        wallMesh.SetUVs(0, wallUVs);
        wallMesh.RecalculateNormals();
        walls.sharedMesh = wallMesh;
        
        var wallCollider = walls.GetComponent<MeshCollider>();
        if (wallCollider == null)
        {
            wallCollider = walls.gameObject.AddComponent<MeshCollider>();
        }
        wallCollider.sharedMesh = wallMesh;
    }

    void TriangulateSquare(Square square)
    {
        switch (square.configuration)
        {
            case 0:
                break;
            
            // 1 point
            case 1:
                MeshFromPoints(square.centerLeft, 
                    square.centerBottom, 
                    square.bottomLeft);
                break;
            case 2:
                MeshFromPoints(square.bottomRight, 
                    square.centerBottom, 
                    square.centerRight);
                break;
            case 4:
                MeshFromPoints(square.topRight, 
                    square.centerRight, 
                    square.centerTop);
                break;
            case 8:
                MeshFromPoints(square.topLeft, 
                    square.centerTop, 
                    square.centerLeft);
                break;
            
            //2 points
            case 3:
                MeshFromPoints(square.centerRight, 
                    square.bottomRight, 
                    square.bottomLeft, 
                    square.centerLeft);
                break;
            case 6:
                MeshFromPoints(square.centerTop, 
                    square.topRight, 
                    square.bottomRight, 
                    square.centerBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, 
                    square.centerTop, 
                    square.centerBottom, 
                    square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, 
                    square.topRight, 
                    square.centerRight, 
                    square.centerLeft);
                break;
            case 5:
                MeshFromPoints(square.centerTop, 
                    square.topRight, 
                    square.centerRight, 
                    square.centerBottom, 
                    square.bottomLeft, 
                    square.centerLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, 
                    square.centerTop, 
                    square.centerRight,
                    square.bottomRight,
                    square.centerBottom,
                    square.centerLeft
                    );
                break;
            
            // 3 points
            case 7:
                MeshFromPoints(square.centerTop,
                    square.topRight,
                    square.bottomRight,
                    square.bottomLeft,
                    square.centerLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft,
                    square.centerTop,
                    square.centerRight,
                    square.bottomRight,
                    square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft,
                    square.topRight,
                    square.centerRight,
                    square.centerBottom,
                    square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft,
                    square.topRight,
                    square.bottomRight,
                    square.centerBottom,
                    square.centerLeft);
                break;
            
            //4 points
            case 15:
                MeshFromPoints(square.topLeft,
                    square.topRight,
                    square.bottomRight,
                    square.bottomLeft);
                checkedVertices.Add(square.topLeft.vertexIndex);
                checkedVertices.Add(square.topRight.vertexIndex);
                checkedVertices.Add(square.bottomRight.vertexIndex);
                checkedVertices.Add(square.bottomLeft.vertexIndex);
                break;
        }
    }

    void MeshFromPoints(params Node[] points)
    {
        AssignVertices(points);

        if (points.Length >= 3)
        {
            CreateTriangles(points[0], points[1], points[2]);
        }

        if (points.Length >= 4)
        {
            CreateTriangles(points[0], points[2], points[3]);
        }

        if (points.Length >= 5)
        {
            CreateTriangles(points[0], points[3], points[4]);
        }

        if (points.Length >= 6)
        {
            CreateTriangles(points[0], points[4], points[5]);
        }
    }

    void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangles(Node a, Node b, Node c)
    {
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);
        
        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        if (triangleMap.ContainsKey(vertexIndexKey))
        {
            triangleMap[vertexIndexKey].Add(triangle);
            
        }
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleMap.Add(vertexIndexKey, triangleList);
        }
    }

    void CalculateMeshOutlines()
    {
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex++)
        {
            if (!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);
                    
                    List<int> newOutline =  new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    private void FollowOutline(int vertexIndex, int outlineIndex)
    {
        outlines [outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);
        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        List<Triangle> trianglesContainingVertex = triangleMap[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j++)
            {
                int vertexB = triangle[j];

                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if (IsOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
                
            }
        }
        
        return -1;
    }
    
    bool IsOutlineEdge(int vertexA, int vertexB)
    {
        List<Triangle> trianglesContainingVertexA = triangleMap[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < trianglesContainingVertexA.Count; i++)
        {
            if (trianglesContainingVertexA[i].Contains(vertexB))
            {
                sharedTriangleCount++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }
        return sharedTriangleCount == 1;
    }

    readonly struct Triangle
    {
        public readonly int vertexIndexA;
        public readonly int vertexIndexB;
        public readonly int vertexIndexC;
        
        private readonly int[] vertices;

        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;
            
            vertices = new[] { a, b, c };
        }

        public int this[int i]
        {
            get
            {
                return vertices[i];
            }
        }

        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }
    
    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;
            
            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(
                        -mapWidth/2 + x * squareSize + squareSize/2,
                        0,
                        -mapHeight/2 + y * squareSize + squareSize/2);
                    float v = map[x,y] == 1 ? 1f : 0f;
                    controlNodes[x,y] = new ControlNode(pos, v, map[x,y] == 1, squareSize);
                }
            }
            
            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(
                        controlNodes[x,y+1],
                        controlNodes[x+1,y+1],
                        controlNodes[x+1,y],
                        controlNodes[x,y],
                        squareSize);
                }
            }
        }

        public SquareGrid(int[,] map, float[,] values, float squareSize)
        {
            int nodeCountX = map.GetLength(0);
            int nodeCountY = map.GetLength(1);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;
            
            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(
                        -mapWidth/2 + x * squareSize + squareSize/2,
                        0,
                        -mapHeight/2 + y * squareSize + squareSize/2);
                    float v = Mathf.Clamp01(values[x,y]);
                    controlNodes[x,y] = new ControlNode(pos, v, map[x,y] == 1, squareSize);
                }
            }
            
            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(
                        controlNodes[x,y+1],
                        controlNodes[x+1,y+1],
                        controlNodes[x+1,y],
                        controlNodes[x,y],
                        squareSize);
                }
            }
        }
    }
    
    public class Square
    {
        public ControlNode topLeft;
        public ControlNode topRight;
        public ControlNode bottomLeft;
        public ControlNode bottomRight;

        public Node centerTop;
        public Node centerBottom;
        public Node centerLeft;
        public Node centerRight;

        public int configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft, float squareSize)
        {
            
            topLeft = _topLeft;
            topRight = _topRight;
            bottomLeft = _bottomLeft;
            bottomRight = _bottomRight;

            float threshold = 0.5f;
            float tTop = InverseLerpSafe(topLeft.value, topRight.value, threshold);
            float tBottom = InverseLerpSafe(bottomLeft.value, bottomRight.value, threshold);
            float tLeft = InverseLerpSafe(bottomLeft.value, topLeft.value, threshold);
            float tRight = InverseLerpSafe(bottomRight.value, topRight.value, threshold);

            centerTop = new Node(Vector3.Lerp(topLeft.position, topRight.position, tTop));
            centerBottom = new Node(Vector3.Lerp(bottomLeft.position, bottomRight.position, tBottom));
            centerLeft = new Node(Vector3.Lerp(bottomLeft.position, topLeft.position, tLeft));
            centerRight = new Node(Vector3.Lerp(bottomRight.position, topRight.position, tRight));

            if (topLeft.active)
            {
                configuration += 8;
            }

            if (topRight.active)
            {
                configuration += 4;
            }

            if (bottomLeft.active)
            {
                configuration += 1;
            }

            if (bottomRight.active)
            {
                configuration += 2;
            }
        }
    }
    
    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;

        public Node(Vector3 _position)
        {
            position = _position;
        }
    }

    public class ControlNode : Node
    {
        public bool active;
        public float value;
        public Node above, right;

        public ControlNode(Vector3 _position, float _value, bool _active, float squareSize) : base(_position)
        {
            active = _active;
            value = _value;
            // Keep midpoints for reference; actual edge nodes are interpolated per Square
            above = new Node(position + Vector3.forward * squareSize/2f);
            right = new Node(position + Vector3.right * squareSize/2f);
        }
    }

    static float InverseLerpSafe(float a, float b, float value)
    {
        float denom = (b - a);
        if (Mathf.Abs(denom) < 1e-6f)
        {
            return 0.5f;
        }
        return Mathf.Clamp01((value - a) / denom);
    }
}

