using Unity.Mathematics;

namespace _Project.World.Ship.Generation
{
    public struct ShipData
    {
        public int ThrusterCount;
        public int WeaponCount;
        public int Armor;
    }

    public struct ShipLayout
    {
        public int Iteration;

        public ShipTile[,,] Tiles;

    }

    public struct ShipEvaluationResult
    {
        public float Movability;
        public float Mass;
        public float Stability;
        public float Maneuverability;
        public float Armor;
        public float Aesthetics;
    }

    public struct ShipTile
    {
        public ShipTileType Type;
        public TileDirection Direction;
        public int3 Position;
    }
    
    public enum ShipTileType
    {
        Empty,
        Thruster,
        Engine,
        Dock,
        Cockpit,
    }
    
    public enum TileDirection
    {
        Up,
        Down,
        Left,
        Right,
    }
}