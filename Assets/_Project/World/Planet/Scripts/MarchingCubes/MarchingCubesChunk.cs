using System;
using System.Collections.Generic;
using _Project.World.Planet.Scripts.WorldGen;
using Unity.Mathematics;
using UnityEngine;
using TMPro;

namespace _Project.World.Planet.Scripts.MarchingCubes
{
    [ExecuteAlways]
    public class MarchingCubesChunk : MonoBehaviour
    {
        private static readonly int Cull = Shader.PropertyToID("_Cull");

        [Header("Chunk settings"), Range(1, 64)] [SerializeField]
        private int size = 4;

        [Header("Noise settings")] 
        [SerializeReference] private TerrainGenerator terrainGenerator;
        [SerializeField, Range(0f, 1f)] private float isoLevel = 0.5f;

        [Header("Debug Text Settings")]
        [SerializeField] private GameObject tmpPrefab;
        [SerializeField] private Color textColor = Color.crimson;
        [SerializeField] private float textScale = 0.2f;

        private MarchingCubesGrid _grid;

        private MeshFilter _meshFilter; // the mesh filter is used to pass the vertices to unity.
        private MeshRenderer _meshRenderer; // and the mesh renderer actually renders these. 

        // the last values so that it knows if something changed
        private float _lastIsoLevel;

        private void Awake()
        {
            EnsureComponents(); // ensure the game object contains a mesh filter and renderer
            Rebuild(); // rebuild the mesh
        }

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            if (!HasSettingsChanged()) return;
            Rebuild();
        }

        // whether the user changed the settings and the applied mesh is outdated
        private bool HasSettingsChanged()
        {
            return Math.Abs(_lastIsoLevel - isoLevel) > 0.0001f;
        }

        //caches the settings in the last versions
        private void CacheSettings()
        {
            _lastIsoLevel = isoLevel;
        }

        //ensures the components (currently the mesh filter and renderer) are applied to the game object
        private void EnsureComponents()
        {
            if (_meshFilter == null)
                _meshFilter = gameObject.GetComponent<MeshFilter>() ?? gameObject.AddComponent<MeshFilter>();
            if (_meshRenderer == null)
                _meshRenderer = gameObject.GetComponent<MeshRenderer>() ?? gameObject.AddComponent<MeshRenderer>();

            if (_meshRenderer.sharedMaterial == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Simple Lit") ?? Shader.Find("Standard");
                _meshRenderer.sharedMaterial = new Material(shader);
            }
        }

        //rebuilds the mesh
        private void Rebuild()
        {
            if (_rebuilding) return; // if we are already rebuilding we dont want to start another rebuild task
            _rebuilding = true;
            EnsureComponents(); //ensure there is a mesh filter and renderer we can apply the mesh to
            _grid = new MarchingCubesGrid(size, terrainGenerator); //generate a grid of density values

            List<Triangle> allTriangles = new List<Triangle>(); //will contain the triangles

            _grid.ForEach((pos, density) => // is called for every density in the 3d array
            {
                var tris = MarchingCubesGenerator.GenerateAt(pos, _grid, isoLevel); // generate the triangles using the marching cubes algorithm
                allTriangles.AddRange(tris); //add all 
            });

            Mesh mesh = BuildMesh(allTriangles.ToArray()); // build the mesh from the triangles
            _meshFilter.sharedMesh = mesh; //and apply it to the mesh filter
            _meshRenderer.sharedMaterial.SetInt(Cull, (int)UnityEngine.Rendering.CullMode.Back);

            CacheSettings();
            _rebuilding = false;
        }

        // this function actually builds the mesh using the given triangle. this also contains calculating the indices, normals and bounds of the mesh.
        private static Mesh BuildMesh(Triangle[] tris)
        {
            Mesh mesh = new Mesh
            {
                indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 // allow more than 65535 vertices (which is the default limit for meshes) by using 32 bit indices
            }; // we start with a plain empty mesh


            List<Vector3> vertices = new List<Vector3>(); // we will put the vertices into this list
            List<int> indices = new List<int>(); // and the indices into this one, as the name suggests
            
            List<Vector3> normals = new List<Vector3>();
            
            Dictionary<Vector3, int> indexMap = new Dictionary<Vector3, int>(); // this is used for index deduplication.
                                                                                // since every triangle has 3 vertices and many triangles share vertices with each other, we want to avoid adding the same vertex multiple times to the vertices list.
                                                                                // that would be a waste of memory and also cause visual artifacts. so we use this dictionary to check if we already added a vertex and if so we just reuse its index instead of adding it again.

            foreach (var t in tris)
            {
                int i0 = GetOrAddVertex(t.P1, vertices, normals, indexMap);
                int i1 = GetOrAddVertex(t.P2, vertices, normals, indexMap);
                int i2 = GetOrAddVertex(t.P3, vertices, normals, indexMap);

                indices.Add(i0);
                indices.Add(i1);
                indices.Add(i2);

                Vector3 normal = Vector3.Cross(t.P2 - t.P1, t.P3 - t.P1).normalized; // this calculates a good normal value based on the triangle vertices, which is important for the lighting to look smooth.
                normals[i0] += normal;
                normals[i1] += normal;
                normals[i2] += normal;
            }

            

            for (int i = 0; i < normals.Count; i++) // we need to normalize the normals because we added them up for every triangle that shares a vertex to get smooth shading, but now they are not normalized anymore
            {
                normals[i] = normals[i].normalized;
            }
            
            
            
            mesh.SetVertices(vertices); // set the vertices
            mesh.SetNormals(normals);
            mesh.SetTriangles(indices, 0); // and set the indices

            mesh.RecalculateBounds();//recalculate bounds

            return mesh;
        }
        
        /// <summary>
        /// checks if the given vertex already exists in the vertices list and if so returns its index,
        /// otherwise it adds it to the list and returns the new index
        /// </summary>
        private static int GetOrAddVertex(
            Vector3 v,
            List<Vector3> vertices,
            List<Vector3> normals,
            Dictionary<Vector3, int> indexMap)
        {
            if (indexMap.TryGetValue(v, out var index))
                return index;

            index = vertices.Count;
            vertices.Add(v);
            normals.Add(Vector3.zero);
            indexMap.Add(v, index);
            return index;
        }


        private bool _rebuilding = false;
        
        // if the user changed any setting the mesh should be updated
        private void Update()
        {
            if (!Application.isPlaying) return; // if the application isnt even running we can return
            if (!HasSettingsChanged()) return; // and if the user didnt change anything we can also return
            Rebuild();
        }
        
        private void OnEnable()
        {
            Subscribe();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (terrainGenerator != null)
                terrainGenerator.OnSettingsChanged += Rebuild; // this way we add a listener to the onSettingsChanged action that gets called every time the user changes a value 
        }

        private void Unsubscribe()
        {
            if (terrainGenerator != null)
                terrainGenerator.OnSettingsChanged -= Rebuild; // remove the listener
        }

        // draws a cube index at a given pos, deprecated, will remove that soon
        private void DrawDebugText(int3 gridPos, int cubeIndex)
        {
            if (tmpPrefab == null) return;

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