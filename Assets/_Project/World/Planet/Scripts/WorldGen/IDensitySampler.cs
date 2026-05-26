using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.WorldGen
{
    /// <summary>
    /// The base for all terrain generators. every DensitySampler must contain a function that returns the density at any given point in space. 
    /// </summary>
    public interface IDensitySampler
    {
        float DensityAt(float3 position);
    }
}