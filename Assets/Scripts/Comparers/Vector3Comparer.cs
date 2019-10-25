using System.Collections.Generic;
using UnityEngine;

namespace Comparers
{
    class Vector3Comparer : IEqualityComparer<Vector3>
    {
        public bool Equals(Vector3 v1, Vector3 v2)
        {
            return v1.x == v2.x && v1.y == v2.y && v1.z == v2.z;
        }

        public int GetHashCode(Vector3 obj)
        {
            return obj.x.GetHashCode() ^ obj.y.GetHashCode() << 2 ^ obj.z.GetHashCode() >> 2;
        }
    }
}