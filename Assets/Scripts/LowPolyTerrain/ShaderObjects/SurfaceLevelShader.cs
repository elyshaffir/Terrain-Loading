using System;
using ComputeShading;
using LowPolyTerrain.Chunk;
using LowPolyTerrain.MeshGeneration;
using LowPolyTerrain.MeshGeneration.DataStructures;
using UnityEngine;
using static ComputeShading.ComputeShaderProperty;

namespace LowPolyTerrain.ShaderObjects
{
    class SurfaceLevelShader : ComputeShaderObject
    {
        public static ComputeShader surfaceLevelGeneratorShader;
        public static float seed;

        readonly TerrainChunkMeshGenerator generator;

        ComputeBuffer outputPoints;
        ComputeBuffer relevantBuffer;
        ComputeBuffer irrelevantBuffer;
        ComputeBuffer relevantCountBuffer;
        ComputeBuffer irrelevantCountBuffer;

        ///
        ComputeBuffer relevantCubeCornersBuffer;
        uint[] relevantCubeCorners;
        ComputeBuffer debugBuffer;
        ///

        bool relevant;

        public SurfaceLevelShader(TerrainChunkMeshGenerator generator) :
            base(surfaceLevelGeneratorShader,
                surfaceLevelGeneratorShader.FindKernel("GenerateSurfaceLevel"))
        {
            this.generator = generator;
        }

        protected override ComputeShaderProperty[] GetProperties()
        {
            return new ComputeShaderProperty[] {
                new ComputeShaderIntProperty("numPointsX", generator.constraint.scale.x * TerrainChunk.ChunkSizeInPoints.x),
                new ComputeShaderIntProperty("numPointsY", generator.constraint.scale.y * TerrainChunk.ChunkSizeInPoints.y),
                new ComputeShaderIntProperty("numPointsZ", generator.constraint.scale.z * TerrainChunk.ChunkSizeInPoints.z),
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
                new ComputeShaderFloatProperty("isoLevel", TerrainChunkMeshGenerator.IsoLevel)
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

            ///
            relevantCubeCorners = new uint[generator.points.Length];
            Array.Clear(relevantCubeCorners, 0, relevantCubeCorners.Length);
            relevantCubeCornersBuffer = new ComputeBuffer(generator.points.Length, sizeof(uint));
            relevantCubeCornersBuffer.SetData(relevantCubeCorners);
            SetBuffer("relevantCubeCorners", relevantCubeCornersBuffer);

            float[] debug = new float[10];
            Array.Clear(debug, 0, debug.Length);
            debugBuffer = new ComputeBuffer(debug.Length, sizeof(float));
            debugBuffer.SetData(debug);
            SetBuffer("debug", debugBuffer);
            ///
        }

        public override void Dispatch()
        {
            Dispatch(
                generator.constraint.scale.x * TerrainChunk.ChunkSizeInPoints.x / 5,
                generator.constraint.scale.y * TerrainChunk.ChunkSizeInPoints.y / 5,
                generator.constraint.scale.z * TerrainChunk.ChunkSizeInPoints.z / 5,
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

            ///
            relevantCubeCornersBuffer.GetData(relevantCubeCorners);
            PrepareRelevantCubesShader prepareRelevantCubesShader = new PrepareRelevantCubesShader(generator);
            prepareRelevantCubesShader.Execute(relevantCubeCorners);
            float[] debug = new float[10];
            debugBuffer.GetData(debug);
            if (generator.constraint.position.Equals(new Vector3(-28, 0, -28)))
            {
                Debug.Log(debug[0]);
            }
            ///
        }

        public bool IsRelevant()
        {
            return relevant;
        }
    }
}