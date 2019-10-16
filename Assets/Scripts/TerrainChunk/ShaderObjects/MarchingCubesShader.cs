using Assets.Scripts.ComputeShaderObject;
using UnityEngine;
using static TerrainChunkMeshGenerator;

public class MarchingCubesShader : ComputeShaderObject
{
    public static ComputeShader marchingCubesGeneratorShader;

    private readonly TerrainChunkMeshGenerator generator;

    private ComputeBuffer triangleBuffer;
    private ComputeBuffer triangleCountBuffer;
    private ComputeBuffer inputPoints;

    public MarchingCubesShader(TerrainChunkMeshGenerator generator) :
        base(marchingCubesGeneratorShader,
            marchingCubesGeneratorShader.FindKernel("MarchCubes"))
    {
        this.generator = generator;
    }

    protected override ComputeShaderProperty[] GetProperties()
    {
        return new ComputeShaderProperty[] {
                new ComputeShaderIntProperty("numPointsX", generator.constraint.scale.x * TerrainChunk.ChunkSize.x),
                new ComputeShaderIntProperty("numPointsY", generator.constraint.scale.y * TerrainChunk.ChunkSize.y),
                new ComputeShaderIntProperty("numPointsZ", generator.constraint.scale.z * TerrainChunk.ChunkSize.z),
                new ComputeShaderFloatProperty("isoLevel", -3.5f) // Note that it is hard coded
            };
    }

    public override void SetBuffers()
    {
        triangleBuffer = new ComputeBuffer(
            generator.constraint.GetVolume(), Triangle.StructSize, ComputeBufferType.Append);
        triangleBuffer.SetCounterValue(0);
        triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        inputPoints = new ComputeBuffer(generator.points.Length, Point.StructSize);
        inputPoints.SetData(generator.points);

        SetBuffer("triangles", triangleBuffer);
        SetBuffer("points", inputPoints);
        AddBuffer(triangleCountBuffer);
    }

    public override void Dispatch()
    {
        Dispatch(
            generator.constraint.scale.x,
            generator.constraint.scale.y,
            generator.constraint.scale.z,
            GetProperties());
    }

    public override void GetData()
    {
        ComputeBuffer.CopyCount(triangleBuffer, triangleCountBuffer, 0);
        int[] triangleCount = new int[1] { 0 };
        triangleCountBuffer.GetData(triangleCount);
        generator.triangles = new Triangle[triangleCount[0]];
        triangleBuffer.GetData(generator.triangles);
    }
}