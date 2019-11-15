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
        public static float isoLevel;

        public ComputeBuffer pointsBuffer;

        readonly TerrainChunkMeshGenerator generator;

        ComputeBuffer relevantCubeCornersBuffer;
        PrepareRelevantCubesShader prepareRelevantCubesShader;

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
                new ComputeShaderFloatProperty("isoLevel", isoLevel)
            };
        }

        public override void SetBuffers()
        {
            pointsBuffer = new ComputeBuffer(generator.constraint.GetVolume(), Point.StructSize);
            // relevantCubeCorners = new uint[generator.constraint.GetVolume()];
            // Array.Clear(relevantCubeCorners, 0, relevantCubeCorners.Length);
            relevantCubeCornersBuffer = new ComputeBuffer(generator.constraint.GetVolume(), sizeof(uint));
            // if the initial value is not set to 0 it might pose a problem
            // but even if the value in the array is random, worst case scenario
            // the random value is 1 and an empty cube gets marched
            // relevantCubeCornersBuffer.SetData(relevantCubeCorners);

            SetBuffer("points", pointsBuffer);
            SetBuffer("relevantCubeCorners", relevantCubeCornersBuffer);
            SetBuffer("alterations", generator.getPointsToAlterShader.alterationsBuffer, false);
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
            prepareRelevantCubesShader = new PrepareRelevantCubesShader(generator, relevantCubeCornersBuffer);
            prepareRelevantCubesShader.Execute();
        }

        public override void Release()
        {
            base.Release();
            if (prepareRelevantCubesShader != null)
            {
                prepareRelevantCubesShader.Release();
            }
        }

        public void SetRelevant(bool relevant)
        {
            this.relevant = relevant;
        }

        public bool IsRelevant()
        {
            return relevant;
        }
    }
}