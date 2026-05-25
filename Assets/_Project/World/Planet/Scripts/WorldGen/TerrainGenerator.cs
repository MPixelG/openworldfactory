using System;
using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen
{
    /// <summary>
    /// The base for all terrain generators. every terrain generator must contain a function that returns the density at any given point in space. if a subclass of this wants to have any configuration it can add parameters to its constructor.
    /// </summary>
    public abstract class TerrainGenerator : ScriptableObject
    {
        public abstract float DensityAt(Vector3 worldPosition, Vector3 center);
        
        
        public event Action OnSettingsChanged;

        protected virtual void OnValidate()
        {
            OnSettingsChanged?.Invoke();
        }
        
    }
}