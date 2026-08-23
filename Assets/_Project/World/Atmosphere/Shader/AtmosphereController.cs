using UnityEngine;

namespace _Project.World.Atmosphere.Shader
{
    public class AtmosphereController : MonoBehaviour
    {
        public static AtmosphereController Instance { get; private set; }

        public AtmosphereSettings settings;
        public float planetRadius = 1000f;

        public Vector3 PlanetCentre =>
            transform.position;

        private void Awake()
        {
            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
    }
}