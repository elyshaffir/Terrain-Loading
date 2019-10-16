using System.Collections.Generic;
using Assets.Scripts.ComputeShaderObject;
using UnityEngine;
using static TerrainChunkMeshGenerator;

public class SurfaceLevelShader : ComputeShaderObject
{
    public static ComputeShader surfaceLevelGeneratorShader;
    public static float seed;

    private readonly TerrainChunkMeshGenerator generator;

    private ComputeBuffer outputPoints;
    private ComputeBuffer relevantBuffer;
    private ComputeBuffer irrelevantBuffer;
    private ComputeBuffer relevantCountBuffer;
    private ComputeBuffer irrelevantCountBuffer;

    private bool relevant;

    public SurfaceLevelShader(TerrainChunkMeshGenerator generator) :
        base(surfaceLevelGeneratorShader,
            surfaceLevelGeneratorShader.FindKernel("GenerateSurfaceLevel"))
    {
        this.generator = generator;
    }

    protected override ComputeShaderProperty[] GetProperties()
    {
        return new ComputeShaderProperty[] {
            new ComputeShaderIntProperty("numPointsX", generator.constraint.scale.x * TerrainChunk.ChunkSize.x),
            new ComputeShaderIntProperty("numPointsY", generator.constraint.scale.y * TerrainChunk.ChunkSize.y),
            new ComputeShaderIntProperty("numPointsZ", generator.constraint.scale.z *TerrainChunk.ChunkSize.z),
            new ComputeShaderFloatProperty("noiseScale", .94f), // Scale this down and scale the world up to create an effect of more points
            new ComputeShaderIntProperty("octaves", 6),
            new ComputeShaderVector3Property("offset", generator.constraint.position),
            new ComputeShaderFloatProperty("weightMultiplier", 1.9f),
            new ComputeShaderFloatProperty("persistence", .5f),
            new ComputeShaderFloatProperty("lacunarity", 2f),
            new ComputeShaderFloatProperty("floorOffset", 1f),
            new ComputeShaderFloatProperty("noiseWeight", 9.19f),
            new ComputeShaderVector4Property("params", new Vector4(1, 1, 1, 1)),
            new ComputeShaderFloatProperty("hardFloor", 1f),
            new ComputeShaderFloatProperty("hardFloorWeight", 37f),
            new ComputeShaderFloatProperty("offsetNoise", seed),
            new ComputeShaderFloatProperty("isoLevel", -3.5f) // Note that it is hard coded            
        };
    }

    public override void SetBuffers()
    {
        generator.points = new Point[generator.constraint.GetVolume()];
        outputPoints = new ComputeBuffer(generator.points.Length, Point.StructSize);
        relevantBuffer = new ComputeBuffer(
            generator.points.Length, sizeof(int), ComputeBufferType.Append);
        irrelevantBuffer = new ComputeBuffer(
            generator.points.Length, sizeof(int), ComputeBufferType.Append);
        relevantCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
        irrelevantCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);

        relevantBuffer.SetCounterValue(0);
        irrelevantBuffer.SetCounterValue(0);

        SetBuffer("relevant", relevantBuffer);
        SetBuffer("irrelevant", irrelevantBuffer);
        SetBuffer("points", outputPoints);
        AddBuffer(relevantCountBuffer);
        AddBuffer(irrelevantCountBuffer);
    }

    public override void Dispatch()
    {
        Dispatch(
            generator.constraint.scale.x * TerrainChunk.ChunkSize.x / 5,
            generator.constraint.scale.y * TerrainChunk.ChunkSize.y / 5,
            generator.constraint.scale.z * TerrainChunk.ChunkSize.z / 5,
            GetProperties());
    }

    public override void GetData()
    {
        ComputeBuffer.CopyCount(relevantBuffer, relevantCountBuffer, 0);
        int[] relevantCount = new int[1] { 0 };
        relevantCountBuffer.GetData(relevantCount);
        ComputeBuffer.CopyCount(irrelevantBuffer, irrelevantCountBuffer, 0);
        int[] irrelevantCount = new int[1] { 0 };
        irrelevantCountBuffer.GetData(irrelevantCount);
        outputPoints.GetData(generator.points);
        relevant = relevantCount[0] != 0 && irrelevantCount[0] != generator.points.Length;
    }

    public bool IsRelevant()
    {
        return relevant;
    }
}