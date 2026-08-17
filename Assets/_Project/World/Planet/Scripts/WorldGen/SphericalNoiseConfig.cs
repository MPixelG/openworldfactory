namespace _Project.World.Planet.Scripts.WorldGen
{
    public struct BurstSphericalNoiseConfig
    {
        public float Radius;
        
        public float ContinentFrequency;
        public int ContinentOctaves;
        public float ContinentPersistence;

        public float TerrainHeight;

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