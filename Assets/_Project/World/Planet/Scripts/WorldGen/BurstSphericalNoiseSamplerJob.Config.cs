using Unity.Burst;
using Unity.Jobs;

namespace _Project.World.Planet.Scripts.WorldGen
{
    public partial struct BurstSphericalNoiseSamplerJob
    {
        public float Radius;
        

        public float TerrainHeight;

        public float ReferenceRadius;
        

        public float ContinentFrequency;
        public int ContinentOctaves;
        public float ContinentPersistence;

        public float OceanThreshold;
        

        public float MountainMaskFrequency;

        public float MountainThreshold;

        public float MountainBlend;
        
        
        public float MountainFrequency;
        public int MountainOctaves;
        public float MountainPersistence;

        public float MountainSharpness;
        

        public float PlainsStrength;
        public float PlainsFrequency;
        

        public float DetailFrequency;
        public float DetailStrength;
        

        public float WarpFrequency;
        public float WarpStrength;
    }
}