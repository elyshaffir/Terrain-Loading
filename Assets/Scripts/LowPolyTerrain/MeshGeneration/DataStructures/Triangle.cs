using UnityEngine;

namespace LowPolyTerrain.MeshGeneration.DataStructures
{
    struct Triangle
    {
#pragma warning disable 649
        public Vector3 vertexC;
        public Vector3 vertexB;
        public Vector3 vertexA;

        public const int StructSize = sizeof(float) * 3 * 3;
    }
}