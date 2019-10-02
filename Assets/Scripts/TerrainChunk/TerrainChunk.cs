using System.Collections.Generic;
using Assets.Scripts.ComputeShaderObject;
using UnityEngine;

/*
The problem that the mesh just doesn't appear unless being created repeatedly each frame is caused by
the program setting the mesh data to unfinished results from the GPU (this is possible since it is multi-threaded)
- This was determined due to the length of the mesh vertices array being 0 at first creation in Start() method.
----------
To Make sure a compute shader is done, create a flag "bool done = false;"
in the shader and make it true when the shader process is done.
 */
public class TerrainChunk
{
    private struct Point
    {
#pragma warning disable 649
        internal Vector3 position;
        internal float surfaceLevel;
    }

    private struct Triangle
    {
#pragma warning disable 649
        internal Vector3 vertexC;
        internal Vector3 vertexB;
        internal Vector3 vertexA;
    }

    public const int ChunkSize = 10;
    public static GameObject prefab;
    public static ComputeShader surfaceLevelGeneratorShader;
    public static ComputeShader marchingCubesGeneratorShader;

    public TerrainChunkIndex index;

    private TerrainChunkConstraint constraint;
    private ComputeShaderObject surfaceLevelShader;
    private ComputeShaderObject marchingCubesShader;
    private Mesh mesh;
    private GameObject terrainObject;
    private Point[] points;
    private Triangle[] triangles;

    public TerrainChunk(TerrainChunkIndex index)
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
        // The position of terrainObject is (0, 0, 0) becasue the shaders put the vertices where
        // they need anyway, so repositioning is wrong.
        this.terrainObject = MonoBehaviour.Instantiate(prefab, Vector3.zero, Quaternion.identity);
        terrainObject.GetComponent<MeshFilter>().mesh = mesh;

        points = new Point[] { };
        triangles = new Triangle[] { };
    }

    public void Update(Vector3Int scaleFactor)
    {
        constraint.Scale(scaleFactor);
    }

    public void DebugFunciton()
    {
        Debug.Log(triangles.Length);
    }

    public void GenerateMesh()
    {
        ComputeShaderProperty[] surfaceLevelShaderProperties = new ComputeShaderProperty[] {
            new ComputeShaderIntProperty("numPointsX", constraint.scale.x * TerrainChunk.ChunkSize),
            new ComputeShaderIntProperty("numPointsY", constraint.scale.y * TerrainChunk.ChunkSize),
            new ComputeShaderIntProperty("numPointsZ", constraint.scale.z *TerrainChunk.ChunkSize),
            new ComputeShaderFloatProperty("noiseScale", .94f),
            new ComputeShaderIntProperty("octaves", 6),
            new ComputeShaderVector3Property("offset", constraint.position), // Notice this used to be index.ToPosition()
            new ComputeShaderFloatProperty("weightMultiplier", 1.9f),
            new ComputeShaderFloatProperty("persistence", .5f),
            new ComputeShaderFloatProperty("lacunarity", 2f),
            new ComputeShaderFloatProperty("floorOffset", 1f),
            new ComputeShaderFloatProperty("noiseWeight", 9.19f),
            new ComputeShaderVector4Property("params", new Vector4(1, 1, 1, 1)),
            new ComputeShaderFloatProperty("hardFloor", 1f),
            new ComputeShaderFloatProperty("hardFloorWeight", 37f)
        };
        ComputeShaderProperty[] marchingCubesShaderProperties = new ComputeShaderProperty[] {
                new ComputeShaderIntProperty("numPointsX", constraint.scale.x * TerrainChunk.ChunkSize),
                new ComputeShaderIntProperty("numPointsY", constraint.scale.y * TerrainChunk.ChunkSize),
                new ComputeShaderIntProperty("numPointsZ", constraint.scale.z *TerrainChunk.ChunkSize),
                new ComputeShaderFloatProperty("isoLevel", -3.5f)
            };
        GeneratePoints(surfaceLevelShaderProperties);
        GenerateTriangles(marchingCubesShaderProperties);
        CreateMesh();
    }

    private void GeneratePoints(ComputeShaderProperty[] surfaceLevelShaderProperties)
    {
        Vector3[] offsets = new Vector3[6];
        for (int i = 0; i < offsets.Length; i++)
        {
            offsets[i] = Vector3.one;
        }
        points = new Point[constraint.GetVolume()];
        ComputeBuffer outputPoints = new ComputeBuffer(points.Length, 16);
        surfaceLevelShader.SetBuffer("points", outputPoints);

        ComputeBuffer offsetsBuffer = new ComputeBuffer(offsets.Length, 12);
        offsetsBuffer.SetData(offsets);
        surfaceLevelShader.SetBuffer("offsets", offsetsBuffer);

        surfaceLevelShader.Dispatch(
            constraint.scale.x * ChunkSize / 5,
            constraint.scale.y * ChunkSize / 5,
            constraint.scale.z * ChunkSize / 5,
            surfaceLevelShaderProperties);

        outputPoints.GetData(points);
        outputPoints.Release();
        offsetsBuffer.Release();
    }

    private void GenerateTriangles(ComputeShaderProperty[] marchingCubesShaderProperties)
    {
        ComputeBuffer triangleBuffer = new ComputeBuffer(
            constraint.GetVolume(), 36, ComputeBufferType.Append); // Notice the count, it is max size of array
        triangleBuffer.SetCounterValue(0);
        ComputeBuffer triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        ComputeBuffer inputPoints = new ComputeBuffer(points.Length, 16);
        inputPoints.SetData(points);

        marchingCubesShader.SetBuffer("triangles", triangleBuffer);
        marchingCubesShader.SetBuffer("points", inputPoints);

        marchingCubesShader.Dispatch(
            (constraint.scale.x * ChunkSize - 1),
            (constraint.scale.y * ChunkSize - 1),
            (constraint.scale.z * ChunkSize - 1),
            marchingCubesShaderProperties); // Perhaps optimize

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

    public Vector3 GetScale()
    {
        return constraint.scale * ChunkSize;
    }
}