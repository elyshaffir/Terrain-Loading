using System.Collections.Generic;
using UnityEngine;

namespace Comparers
{
    class Vector3IntComparer : IEqualityComparer<Vector3Int>
    {
        public bool Equals(Vector3Int v1, Vector3Int v2)
        {
            return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
        }

        public int GetHashCode(Vector3Int obj)
        {
            return obj.x.GetHashCode() ^ obj.y.GetHashCode() << 2 ^ obj.z.GetHashCode() >> 2;
        }
    }
}