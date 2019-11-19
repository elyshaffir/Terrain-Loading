using System;
using ComputeShading;
using LowPolyTerrain.Chunk;
using LowPolyTerrain.MeshGeneration;
using UnityEngine;
using static ComputeShading.ComputeShaderProperty;

namespace LowPolyTerrain.ShaderObjects
{
    class MarchingCubesShader : ComputeShaderObject
    {
        public static ComputeShader marchingCubesGeneratorShader;

        readonly TerrainChunkMeshGenerator generator;

        ComputeBuffer cubesToMarchBuffer;

        uint cubesToMarchCount;

        ///
        ComputeBuffer currentCubeBuffer;
        ComputeBuffer meshVerticesBuffer;
        ComputeBuffer meshVerticesCountBuffer;
        ComputeBuffer meshTrianglesBuffer;
        ComputeBuffer meshTrianglesCountBuffer;
        ///

        public MarchingCubesShader(TerrainChunkMeshGenerator generator) :
            base(marchingCubesGeneratorShader,
                marchingCubesGeneratorShader.FindKernel("MarchCubes"))
        {
            this.generator = generator;
        }

        public void SetCubesToMarch(ComputeBuffer cubesToMarchBuffer, uint cubesToMarchCount)
        {
            this.cubesToMarchBuffer = cubesToMarchBuffer;
            this.cubesToMarchCount = cubesToMarchCount;
        }

        protected override ComputeShaderProperty[] GetProperties()
        {
            return new ComputeShaderProperty[] {
                new ComputeShaderIntProperty("numPointsX", generator.constraint.scale.x * TerrainChunk.ChunkSizeInPoints.x),
                new ComputeShaderIntProperty("numPointsY", generator.constraint.scale.y * TerrainChunk.ChunkSizeInPoints.y),
                new ComputeShaderFloatProperty("isoLevel", SurfaceLevelShader.isoLevel)
            };
        }

        public override void SetBuffers()
        {
            ///
            currentCubeBuffer = new ComputeBuffer(2, sizeof(uint));
            uint[] filler = new uint[2] { 0, 0 };
            currentCubeBuffer.SetData(filler);
            SetBuffer("currentCube", currentCubeBuffer);

            meshVerticesBuffer = new ComputeBuffer((int)cubesToMarchCount * 5 * 3, sizeof(float) * 3, ComputeBufferType.Append);
            meshVerticesBuffer.SetCounterValue(0);
            meshVerticesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
            SetBuffer("meshVertices", meshVerticesBuffer);
            AddBuffer(meshVerticesCountBuffer);

            meshTrianglesBuffer = new ComputeBuffer((int)cubesToMarchCount * 5 * 3, sizeof(int), ComputeBufferType.Append);
            meshTrianglesBuffer.SetCounterValue(0);
            meshTrianglesCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
            SetBuffer("meshTriangles", meshTrianglesBuffer);
            AddBuffer(meshTrianglesCountBuffer);
            ///

            SetBuffer("points", generator.surfaceLevelShader.pointsBuffer, false);
            SetBuffer("cubesToMarch", cubesToMarchBuffer);
        }

        public override void Dispatch()
        {
            Dispatch(
                Mathf.CeilToInt((cubesToMarchCount) / 5f),
                1,
                1,
                GetProperties());
        }

        public override void GetData()
        {
            generator.mesh.Clear();

            ComputeBuffer.CopyCount(meshVerticesBuffer, meshVerticesCountBuffer, 0);
            int[] verticesCount = new int[1] { 0 };
            meshVerticesCountBuffer.GetData(verticesCount);
            generator.mesh.vertices = new Vector3[verticesCount[0]];
            meshVerticesBuffer.GetData(generator.mesh.vertices);

            ComputeBuffer.CopyCount(meshTrianglesBuffer, meshTrianglesCountBuffer, 0);
            int[] trianglesCount = new int[1] { 0 };
            meshTrianglesCountBuffer.GetData(trianglesCount);
            generator.mesh.triangles = new int[trianglesCount[0]];
            meshTrianglesBuffer.GetData(generator.mesh.triangles);

            try
            {
                // Debug.Log(generator.mesh.triangles[101]);
                // Debug.Log(generator.mesh.vertices[0]);
            }
            catch (IndexOutOfRangeException) { }

            ///
            uint[] currentCube = new uint[2];
            currentCubeBuffer.GetData(currentCube);
            // Debug.Log(currentCube[0]);
            //

            generator.mesh.RecalculateNormals();
        }
    }
}