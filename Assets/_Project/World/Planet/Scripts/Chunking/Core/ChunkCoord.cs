namespace _Project.World.Planet.Scripts.Chunking.Core
{
    public struct ChunkCoord
    {
        public int X;
        public int Y;
        public int Z;

        public ChunkCoord(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}