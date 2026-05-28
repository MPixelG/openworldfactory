using UnityEngine;

namespace _Project.World.Planet.Scripts.WorldGen.Unity
{
    public abstract class DensitySamplerSettings : ScriptableObject
    {
        public abstract IDensitySampler CreateSampler();
    }
}

