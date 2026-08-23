using UnityEngine;

namespace _Project.World.Atmosphere.Shader
{
    public class AtmosphereController : MonoBehaviour
    {
        public static AtmosphereController Instance { get; private set; }

        public float planetRadius = 144f;

        public Vector3 PlanetCentre => transform.position;

        private void OnEnable()
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