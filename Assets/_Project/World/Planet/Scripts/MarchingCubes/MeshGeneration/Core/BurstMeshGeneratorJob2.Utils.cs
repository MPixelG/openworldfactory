using _Project.World.Planet.Scripts.MarchingCubes.Materials;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration.Core
{
    public unsafe partial struct BurstMeshGeneratorJob2
    {
        private static VoxelMaterial GetDominantMaterial(
            VoxelMaterial a,
            VoxelMaterial b,
            VoxelMaterial c,
            VoxelMaterial d,
            VoxelMaterial e,
            VoxelMaterial f,
            int materialCount, ref int* counts)
        {
            for (int i = 0; i < materialCount-1; i++)
            {
                counts[i] = 0;
            }
            counts[(int)a]++;
            counts[(int)b]++;
            counts[(int)c]++;
            counts[(int)d]++;
            counts[(int)e]++;
            counts[(int)f]++;

            int max = 0;
            int maxIndex = 0;

            
            for (int i = 0; i < materialCount-1; i++) //exclude air (its the last field)
            {
                if (counts[i] > max)
                {
                    max = counts[i];
                    maxIndex = i;
                }
            }

            return (VoxelMaterial) maxIndex;
        }

        // lerps (linear interpolates) between 2 given points based on their density values and the iso level
        private float3 VertexInterp(float isoLevel, int3 p1, int3 p2, float valP1, float valP2)
        {
            float3 p;

            if (math.abs(isoLevel - valP1) < 0.00001)
                return new float3(p1.x, p1.y, p1.z) * CellSize;
            if (math.abs(isoLevel - valP2) < 0.00001)
                return new float3(p2.x, p2.y, p2.z) * CellSize;
            if (math.abs(valP1 - valP2) < 0.00001)
                return new float3(p1.x, p1.y, p1.z) * CellSize;

            float mu = (isoLevel - valP1) / (valP2 - valP1);
            p.x = p1.x + mu * (p2.x - p1.x);
            p.y = p1.y + mu * (p2.y - p1.y);
            p.z = p1.z + mu * (p2.z - p1.z);

            return p * CellSize;
        }
    }
}