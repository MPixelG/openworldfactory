using UnityEngine;

public class TerrainFace
{
    // Determines whether we generate a cube-based sphere face
    // or subdivide a triangle face.
    bool _cubeMesh;
    Mesh _mesh;
    int _resolution;
    // Used for triangle-based generation
    private Vector3[] _vertices;
    // Face orientation vectors
    Vector3 _localUp;
    Vector3 _axisA;
    Vector3 _axisB;
    
    // Constructor for cube-sphere generation
    public TerrainFace(Mesh mesh, int resolution, Vector3 localUp)
    {
        this._cubeMesh = true;
        this._localUp =  localUp;
        this._mesh = mesh;
        this._resolution = resolution;
        // Create two perpendicular axes for the face plane
        _axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        _axisB = Vector3.Cross(localUp, _axisA);
    }
    // Constructor for triangle subdivision generation
    public TerrainFace(Mesh mesh, int resolution, Vector3[] vertices)
    {
        this._cubeMesh = false;
        this._mesh = mesh;
        this._resolution = resolution;
        this._vertices = vertices;
    }
    // Splits a triangle into many smaller triangles
    // based on the given resolution.
    public (Vector3[], int[]) TriangleFragmentation(Vector3[] triangle, int res)
    {
        // Total vertex count for a triangular grid
        Vector3[] vertices = new Vector3[(res + 1)*(res + 2) / 2];
        // Each subdivision creates small triangles
        int[] triangles = new int[res * res*3];
        // Set corner vertices
        vertices[0] = triangle[2];
        vertices[((res+1)*(res+2)/2)-1] = triangle[1];
        vertices[res*(res+1)/2] = triangle[0];
        Vector3 u = triangle[2];
        Vector3 r = triangle[1];
        Vector3 l = triangle[0];
        int n = 0;
        int m = 0;
        // Generate rows of interpolated vertices
        for (int i = 1; i < res; i++)
        {
            n += i + 1;
            m += i;
            // Edge interpolation
            vertices[n] = Vector3.Lerp(u,r, (float)i/res);
            vertices[(res*(res+1)/2)+i] = Vector3.Lerp(l,r, (float)i/res);
            vertices[m] = Vector3.Lerp(u,l, (float)i/res);
            // Fill inner vertices between edges
            if (i != 1)
            {
                for (int j = 1; j < i; j++)
                {
                    vertices[m+j] =  Vector3.Lerp(vertices[m],vertices[n], (float)j/i);
                }
            }
        }
        // Build triangle indices
        int n2 = 1;
        int triCount = 0;
        for (int i = 0; i < ((res) * (res + 1) / 2); i++)
        {
            // First triangle
            triangles[triCount] = i;
            triangles[triCount + 1] = i + n2+1;
            triangles[triCount + 2] = i + n2;
            triCount+=3;
            // Second triangle (except at row edges)
            if (i != (n2 * (n2 + 1) / 2) - 1)
            {
                triangles[triCount] = i;
                triangles[triCount + 1] = i + 1;
                triangles[triCount + 2] = i + n2+1;
                triCount += 3;
            }
            else
            {
                n2++;
            }
        }
        return (vertices, triangles);
    }
    // Creates one face of a cube sphere.
    // Vertices are projected from cube space onto a sphere.
    public (Vector3[],int[]) ConstructCubeMesh()
    {
        Vector3[] vertices = new Vector3[_resolution * _resolution];
        // Two triangles per quad
        int[] triangles = new int[(_resolution-1) * (_resolution-1) * 6];
        int triIndex = 0;
        for (int y = 0; y < _resolution; y++)
        {
            for (int x = 0; x < _resolution; x++)
            { 
                int i = x + y * _resolution;
                // Convert grid position into 0-1 range
                Vector2 percent = new Vector2(x,y)/(_resolution-1);
                // Position on cube face
                Vector3 pointOnUnitCube = _localUp+(percent.x - 0.5f)*2*_axisA+(percent.y - 0.5f)*2*_axisB;
                // Normalize to project onto sphere
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                vertices[i] = pointOnUnitSphere;
                // Generate quad triangles
                if (x != _resolution - 1 && y != _resolution - 1)
                {
                    triangles[triIndex] = i;
                    triangles[triIndex+1] = i+_resolution+1;
                    triangles[triIndex+2] = i+_resolution;
                    triangles[triIndex+3] = i;
                    triangles[triIndex+4] = i+1;
                    triangles[triIndex+5] = i+_resolution+1;
                    triIndex += 6;
                }
            }
        } 
        return (vertices, triangles);
    }
    // Builds the final mesh and applies it to Unity's Mesh object
    public void ConstructMesh()
    {
        Vector3[] pointOnUnitSphere;
        int[] triangles;
        if (!_cubeMesh)
        {
            // Generate subdivided triangle mesh
            (Vector3[], int[]) fragmentedTriangles = TriangleFragmentation(_vertices, _resolution);
            Vector3[] pointOnOctahedron = fragmentedTriangles.Item1;
            triangles = fragmentedTriangles.Item2;
            pointOnUnitSphere = pointOnOctahedron;
            // Project all vertices onto sphere
            for (int i = 0; i < pointOnUnitSphere.Length; i++)
            {
                pointOnUnitSphere[i] = pointOnUnitSphere[i].normalized;
            }
        }
        else
        {
            // Generate cube sphere face
            pointOnUnitSphere = ConstructCubeMesh().Item1;
            triangles = ConstructCubeMesh().Item2;
        }
        // Apply generated geometry to mesh
        _mesh.Clear();
        _mesh.vertices = pointOnUnitSphere;
        _mesh.triangles = triangles;
        // Recalculate lighting normals
        _mesh.RecalculateNormals();
    }
}
