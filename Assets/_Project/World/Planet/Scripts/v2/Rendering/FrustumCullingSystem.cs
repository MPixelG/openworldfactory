using System.Collections.Generic;
using _Project.World.Planet.Scripts.Chunking.OctreeChunkSystem.Core;
using Unity.Mathematics;
using UnityEngine;

namespace _Project.World.Planet.Scripts.v2.Rendering
{
    public class FrustumCullingSystem
    {
        public readonly List<ulong> VisibleChunks = new();
        
        private Vector3 _lastPosition;
        private Quaternion _lastRotation;

        private const float PositionThreshold = 2f;

        private const float RotationThreshold = 2f; // degrees

        public void Update(
            Octree octree,
            Camera camera)
        {
            if (!NeedsUpdate(camera))
                return;

            UpdateVisibleChunks(octree, camera);

            _lastPosition = camera.transform.position;
            _lastRotation = camera.transform.rotation;
        }

        private void UpdateVisibleChunks(
            Octree octree,
            Camera camera)
        {
            VisibleChunks.Clear();

            Plane[] planes =
                GeometryUtility.CalculateFrustumPlanes(camera);

            Traverse(
                octree,
                new int3(0,0,0).EncodeToMorton(0),
                planes
            );
            
            Debug.Log("Updated, new amount of visible chunks: " + VisibleChunks.Count);
        }
        
        private void Traverse(
            Octree octree,
            ulong mortonCode,
            Plane[] planes)
        {
            OctreeNode? nodeNullable =
                octree.GetNodeAtPosition(mortonCode);

            if (!nodeNullable.HasValue)
                return;

            OctreeNode node = nodeNullable.Value;

            Bounds bounds =
                octree.GetBounds(mortonCode);

            if (!IsVisible(bounds, planes))
                return;

            bool isLeaf = node.ChildMask == 0;

            if (isLeaf)
            {
                VisibleChunks.Add(mortonCode);
                return;
            }

            for (byte i = 0; i < 8; i++)
            {
                if ((node.ChildMask & (1 << i)) == 0)
                    continue;

                Traverse(
                    octree,
                    mortonCode.AppendChild(i),
                    planes
                );
            }
        }

        private static bool IsVisible(
            Bounds bounds,
            Plane[] planes)
        {
            foreach (Plane plane in planes)
            {
                Vector3 normal = plane.normal;

                Vector3 p = bounds.min;

                if (normal.x >= 0) p.x = bounds.max.x;
                if (normal.y >= 0) p.y = bounds.max.y;
                if (normal.z >= 0) p.z = bounds.max.z;

                if (plane.GetDistanceToPoint(p) < 0)
                    return false;
            }

            return true;
        }
        
        
        private bool NeedsUpdate(Camera camera)
        {
            float positionDelta =
                Vector3.Distance(
                    camera.transform.position,
                    _lastPosition);

            float rotationDelta =
                Quaternion.Angle(
                    camera.transform.rotation,
                    _lastRotation);

            return positionDelta > PositionThreshold ||
                   rotationDelta > RotationThreshold;
        }
        
    }
}