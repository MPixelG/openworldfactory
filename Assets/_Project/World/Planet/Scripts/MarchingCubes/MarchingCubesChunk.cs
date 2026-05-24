using Unity.Mathematics;
using UnityEngine;
using TMPro;

namespace _Project.World.Planet.Scripts.MarchingCubes
{
    [ExecuteAlways]
    public class MarchingCubesChunk : MonoBehaviour
    {
        [Header("Chunk settings")] [SerializeField]
        private int size = 16;

        [SerializeField] private GameObject tmpPrefab;
        [SerializeField] private Color textColor = Color.white;
        [SerializeField] private float textScale = 0.2f;

        private MarchingCubesGrid _grid;

        private void Awake()
        {
            Debug.Log("Awake");
            _grid = new MarchingCubesGrid(size);
        }

        private void Start()
        {
            Debug.Log("Start");
            Initialize();
            Debug.Log("Initialized");
        }

        private void Initialize()
        {
            _grid.ForEach((pos, density) =>
            {
                int cubeIndex = MarchingCubesGenerator.GetCubeIndexAt(pos, _grid, 0.5f);
                
                
                if (cubeIndex is > 0 and < 255)
                {
                    DrawDebugText(pos, cubeIndex);
                    //todo
                }
            });
        }

        private void DrawDebugText(int3 gridPos, int cubeIndex)
        {
            Debug.Log("Checking");
            if (tmpPrefab == null) return;
            Debug.Log("Drawing");

            Vector3 worldPos = transform.TransformPoint(
                new Vector3(gridPos.x, gridPos.y, gridPos.z)
            );

            GameObject go = Instantiate(tmpPrefab, worldPos, Quaternion.identity, transform);
            go.name = $"CubeIndex_{gridPos.x}_{gridPos.y}_{gridPos.z}";

            var tmp = go.GetComponent<TMP_Text>();
            tmp.text = cubeIndex.ToString();
            tmp.fontSize = 3f;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = textColor;

            go.transform.localScale = Vector3.one * textScale;
            
            Debug.Log("CubeIndex: " + cubeIndex);
        }
    }
}