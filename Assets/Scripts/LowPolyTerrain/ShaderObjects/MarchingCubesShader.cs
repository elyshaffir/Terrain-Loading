using ComputeShading;
using LowPolyTerrain.Chunk;
using LowPolyTerrain.MeshGeneration;
using LowPolyTerrain.MeshGeneration.DataStructures;
using Unity.Mathematics;
using UnityEngine;
using static ComputeShading.ComputeShaderProperty;

namespace LowPolyTerrain.ShaderObjects
{
    class MarchingCubesShader : ComputeShaderObject
    {
        public static ComputeShader marchingCubesGeneratorShader;

        readonly TerrainChunkMeshGenerator generator;

        ComputeBuffer triangleBuffer;
        ComputeBuffer triangleCountBuffer;

        ///
        uint3[] cubesToMarch; // remember to change back to ve3int when finished
        ComputeBuffer debugBuffer;
        ///

        public MarchingCubesShader(TerrainChunkMeshGenerator generator) :
            base(marchingCubesGeneratorShader,
                marchingCubesGeneratorShader.FindKernel("MarchCubes"))
        {
            this.generator = generator;
        }

        public void SetCubesToMarch(uint3[] cubesToMarch)
        { // Perhaps later try to receive that in the constructor
            this.cubesToMarch = cubesToMarch;
        }

        protected override ComputeShaderProperty[] GetProperties()
        {
            return new ComputeShaderProperty[] {
                new ComputeShaderIntProperty("numPointsX", generator.constraint.scale.x * TerrainChunk.ChunkSizeInPoints.x),
                new ComputeShaderIntProperty("numPointsY", generator.constraint.scale.y * TerrainChunk.ChunkSizeInPoints.y),
                new ComputeShaderIntProperty("numPointsZ", generator.constraint.scale.z * TerrainChunk.ChunkSizeInPoints.z),
                new ComputeShaderFloatProperty("isoLevel", TerrainChunkMeshGenerator.IsoLevel)
            };
        }

        public override void SetBuffers()
        {
            triangleBuffer = new ComputeBuffer(
                generator.constraint.GetVolume(), Triangle.StructSize, ComputeBufferType.Append);
            triangleBuffer.SetCounterValue(0);
            triangleCountBuffer = new ComputeBuffer(1, sizeof(int), ComputeBufferType.IndirectArguments);
            ComputeBuffer inputPoints = new ComputeBuffer(generator.points.Length, Point.StructSize);
            inputPoints.SetData(generator.points);

            SetBuffer("triangles", triangleBuffer);
            SetBuffer("points", inputPoints);
            AddBuffer(triangleCountBuffer);
            ///
            ComputeBuffer cubesToMarchBuffer = new ComputeBuffer(TerrainChunk.ChunkSizeInPoints.x * TerrainChunk.ChunkSizeInPoints.y * TerrainChunk.ChunkSizeInPoints.z, sizeof(uint) * 3);
            cubesToMarchBuffer.SetData(cubesToMarch);
            SetBuffer("cubesToMarch", cubesToMarchBuffer);
            debugBuffer = new ComputeBuffer(100, sizeof(float));
            debugBuffer.SetData(new float[100]);
            SetBuffer("debug", debugBuffer);
            ///
        }

        public override void Dispatch()
        {
            // [1] : Cracks at the positive ends of chunks
            // [2] : Results are deterministic
            // [3] : The cracks don't appear in every chunk
            // [4] : They don't always appear in the same place
            // [5] : Adding threads doesn't help
            // [6] : Since some cracks are small, we know that the necessary cubes are being marched but not outputting the proper results            

            Dispatch(
                cubesToMarch.Length,
                1,
                1, // Later optimize thread efficiency
                GetProperties());

            // Dispatch(
            //     generator.constraint.scale.x * TerrainChunk.ChunkSizeInCubes.x / 7,
            //     generator.constraint.scale.y * TerrainChunk.ChunkSizeInCubes.y / 7,
            //     generator.constraint.scale.z * TerrainChunk.ChunkSizeInCubes.z / 7,
            //     GetProperties());
        }

        public override void GetData()
        {
            ComputeBuffer.CopyCount(triangleBuffer, triangleCountBuffer, 0);
            int[] triangleCount = new int[1] { 0 };
            triangleCountBuffer.GetData(triangleCount);
            generator.triangles = new Triangle[triangleCount[0]];
            triangleBuffer.GetData(generator.triangles);

            ///
            float[] debug = new float[100];
            debugBuffer.GetData(debug);
            // if (generator.points.Length - debug[0] <= 0)
            // {
            //     Debug.Log(generator.points.Length - debug[0]); // Remember to add to cubes to march the cubes altered if needed
            // }
            if (generator.constraint.position.Equals(new Vector3(-28, 0, -28)))
            {
                /*
                number of triangles outputted is the same?
                 */
                Debug.Log(debug[0]);
                Vector3 negativeCorner = new Vector3(debug[1], debug[2], debug[3]);

                // TerrainLoadingObject.current.blue.Add(negativeCorner + generator.constraint.position);

                // int currentDebug = 1;
                // float avg = 0;
                // for (int i = 0; i < 8; i++)
                // {
                //     TerrainLoadingObject.current.red.Add(new Vector3(
                //         debug[currentDebug],
                //         debug[currentDebug + 1],
                //         debug[currentDebug + 2]
                //     ) + generator.constraint.position);
                //     avg += debug[currentDebug + 3];
                //     currentDebug += 4;
                // }
                // Debug.Log(avg / 7);
                Debug.Log(debug[99]);
            }
        }
    }
}