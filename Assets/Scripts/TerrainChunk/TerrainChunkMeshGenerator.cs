using System.Collections.Generic;
using Assets.Scripts.ComputeShaderObject;
using UnityEngine;

class TerrainChunkMeshGenerator
{
    public struct Point
    {
#pragma warning disable 649
        public Vector3 position;
        public float surfaceLevel;
        public Vector2 onEdges;

        public const int StructSize = sizeof(float) * 3 + sizeof(float) + sizeof(float) * 2;
    }

    private struct Triangle
    {
#pragma warning disable 649
        internal Vector3 vertexC;
        internal Vector3 vertexB;
        internal Vector3 vertexA;

        internal const int StructSize = sizeof(float) * 3 * 3;
    }

    public static ComputeShader surfaceLevelGeneratorShader;
    public static ComputeShader marchingCubesGeneratorShader;

    private static float seed;

    public Mesh mesh;
    public readonly TerrainChunkConstraint constraint;

    private TerrainChunkIndex index;
    private ComputeShaderObject surfaceLevelShader;
    private ComputeShaderObject marchingCubesShader;
    private Point[] points;
    private Triangle[] triangles;

    public static void Init(ComputeShader surfaceLevelGeneratorShader, ComputeShader marchingCubesGeneratorShader)
    {
        TerrainChunkMeshGenerator.surfaceLevelGeneratorShader = surfaceLevelGeneratorShader;
        TerrainChunkMeshGenerator.marchingCubesGeneratorShader = marchingCubesGeneratorShader;
        seed = Random.Range(-1000000f, 1000000f);
    }

    public TerrainChunkMeshGenerator(TerrainChunkIndex index)
    {
        this.index = index;
        constraint = new TerrainChunkConstraint(index.ToPosition());

        surfaceLevelShader = new ComputeShaderObject(
            surfaceLevelGeneratorShader,
            surfaceLevelGeneratorShader.FindKernel("GenerateSurfaceLevel"));
        marchingCubesShader = new ComputeShaderObject(
            marchingCubesGeneratorShader,
            marchingCubesGeneratorShader.FindKernel("MarchCubes"));

        mesh = new Mesh();
        points = new Point[] { };
        triangles = new Triangle[] { };
    }

    public void Update(Vector3Int scaleFactor, float newConstraintY)
    {
        constraint.UpdateScale(scaleFactor);
        constraint.UpdateY(newConstraintY);
        GenerateMesh();
    }

    public Dictionary<Vector3, float> Alter(Vector3 spherePosition, float sphereRadius, float power, HashSet<TerrainChunkIndex> additionalIndices)
    {
        Dictionary<Vector3, float> alterations = new Dictionary<Vector3, float>(new TerrainChunkAlterationManager.Vector3Comparer());
        for (int i = 0; i < points.Length; i++)
        {
            if (Vector3.Distance(points[i].position, spherePosition) <= sphereRadius)
            {
                points[i].surfaceLevel += power;
                alterations[points[i].position] = points[i].surfaceLevel;

                index.GetAdjacentToManipulate(points[i].onEdges, additionalIndices);
            }
        }
        GenerateMeshWithPoints();
        return alterations;
    }

    public void GenerateMesh()
    {
        ComputeShaderProperty[] surfaceLevelShaderProperties = GetSurfaceLevelProperties();
        ComputeShaderProperty[] marchingCubesShaderProperties = GetMarchingCubesProperties();
        GeneratePoints(surfaceLevelShaderProperties);
        GenerateTriangles(marchingCubesShaderProperties);
        CreateMesh();
    }

    private void GenerateMeshWithPoints()
    {
        ComputeShaderProperty[] marchingCubesShaderProperties = GetMarchingCubesProperties();
        GenerateTriangles(marchingCubesShaderProperties);
        CreateMesh();
    }

    private ComputeShaderProperty[] GetMarchingCubesProperties()
    {
        return new ComputeShaderProperty[] {
                new ComputeShaderIntProperty("numPointsX", constraint.scale.x * TerrainChunk.ChunkSize.x),
                new ComputeShaderIntProperty("numPointsY", constraint.scale.y * TerrainChunk.ChunkSize.y),
                new ComputeShaderIntProperty("numPointsZ", constraint.scale.z *TerrainChunk.ChunkSize.z),
                new ComputeShaderFloatProperty("isoLevel", -3.5f)
            };
    }

    private ComputeShaderProperty[] GetSurfaceLevelProperties()
    {
        return new ComputeShaderProperty[] {
            new ComputeShaderIntProperty("numPointsX", constraint.scale.x * TerrainChunk.ChunkSize.x),
            new ComputeShaderIntProperty("numPointsY", constraint.scale.y * TerrainChunk.ChunkSize.y),
            new ComputeShaderIntProperty("numPointsZ", constraint.scale.z *TerrainChunk.ChunkSize.z),
            new ComputeShaderFloatProperty("noiseScale", .94f),
            new ComputeShaderIntProperty("octaves", 6),
            new ComputeShaderVector3Property("offset", constraint.position),
            new ComputeShaderFloatProperty("weightMultiplier", 1.9f),
            new ComputeShaderFloatProperty("persistence", .5f),
            new ComputeShaderFloatProperty("lacunarity", 2f),
            new ComputeShaderFloatProperty("floorOffset", 1f),
            new ComputeShaderFloatProperty("noiseWeight", 9.19f),
            new ComputeShaderVector4Property("params", new Vector4(1, 1, 1, 1)),
            new ComputeShaderFloatProperty("hardFloor", 1f),
            new ComputeShaderFloatProperty("hardFloorWeight", 37f),
            new ComputeShaderFloatProperty("offsetNoise", seed)
    };
    }

    private void GeneratePoints(ComputeShaderProperty[] surfaceLevelShaderProperties)
    {
        points = new Point[constraint.GetVolume()];
        ComputeBuffer outputPoints = new ComputeBuffer(points.Length, Point.StructSize);
        surfaceLevelShader.SetBuffer("points", outputPoints);

        surfaceLevelShader.Dispatch(
            constraint.scale.x * TerrainChunk.ChunkSize.x / 5,
            constraint.scale.y * TerrainChunk.ChunkSize.y / 5,
            constraint.scale.z * TerrainChunk.ChunkSize.z / 5,
            surfaceLevelShaderProperties);

        outputPoints.GetData(points);
        ApplyAlterations();

        outputPoints.Release();
    }

    private void ApplyAlterations()
    {
        Dictionary<Vector3, float> alterations = TerrainChunkAlterationManager.alterations[index];
        Dictionary<Vector3, int> pointIndices = new Dictionary<Vector3, int>();
        for (int i = 0; i < points.Length; i++)
        {
            pointIndices.Add(points[i].position, i);
        }
        foreach (KeyValuePair<Vector3, float> alteration in alterations)
        {
            // sometimes, alteration.Key is not in pointIndices, this occures when the height is being changed.
            points[pointIndices[alteration.Key]].surfaceLevel = alteration.Value;
        }
    }

    private void GenerateTriangles(ComputeShaderProperty[] marchingCubesShaderProperties)
    {
        ComputeBuffer triangleBuffer = new ComputeBuffer(
            constraint.GetTrianglesVolume(), Triangle.StructSize, ComputeBufferType.Append);
        triangleBuffer.SetCounterValue(0);
        ComputeBuffer triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        ComputeBuffer inputPoints = new ComputeBuffer(points.Length, Point.StructSize);
        inputPoints.SetData(points);

        marchingCubesShader.SetBuffer("triangles", triangleBuffer);
        marchingCubesShader.SetBuffer("points", inputPoints);

        marchingCubesShader.Dispatch(constraint.scale.x, constraint.scale.y, constraint.scale.z, marchingCubesShaderProperties);
        ComputeBuffer.CopyCount(triangleBuffer, triangleCountBuffer, 0);
        int[] triangleCount = new int[1] { 0 };
        triangleCountBuffer.GetData(triangleCount);
        triangles = new Triangle[triangleCount[0]];
        triangleBuffer.GetData(triangles);
        triangleBuffer.Release();
        triangleCountBuffer.Release();
        inputPoints.Release();
    }

    private void CreateMesh()
    {
        mesh.Clear();
        List<Vector3> meshVertices = new List<Vector3>();
        List<int> meshTriangles = new List<int>();
        for (int i = 0; i < triangles.Length; i++)
        {
            Triangle t = triangles[i];
            meshVertices.Add(t.vertexC);
            meshVertices.Add(t.vertexB);
            meshVertices.Add(t.vertexA);
            meshTriangles.Add(i * 3);
            meshTriangles.Add(i * 3 + 1);
            meshTriangles.Add(i * 3 + 2);
        }
        mesh.vertices = meshVertices.ToArray();
        mesh.triangles = meshTriangles.ToArray();
        mesh.RecalculateNormals();
    }
}
