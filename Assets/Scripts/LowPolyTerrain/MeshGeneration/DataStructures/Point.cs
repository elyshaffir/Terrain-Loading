using UnityEngine;

namespace LowPolyTerrain.MeshGeneration.DataStructures
{
    struct Point
    {
#pragma warning disable 649
        public Vector3 position;
        public float surfaceLevel;
        public Vector3 onEdges;

        public const int StructSize = sizeof(float) * 3 + sizeof(float) + sizeof(float) * 3;
    }
}