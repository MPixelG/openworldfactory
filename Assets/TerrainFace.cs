using UnityEngine;

public class TerrainFace
{
    bool _cubeMesh;
    Mesh _mesh;
    int _resolution;
    private Vector3[] _vertices;
    Vector3 _localUp;
    Vector3 _axisA;
    Vector3 _axisB;

    public TerrainFace(Mesh mesh, int resolution, Vector3 localUp)
    {
        this._cubeMesh = true;
        this._localUp =  localUp;
        this._mesh = mesh;
        this._resolution = resolution;
        _axisA = new Vector3(localUp.y, localUp.z, localUp.x);
        _axisB = Vector3.Cross(localUp, _axisA);
    }
    public TerrainFace(Mesh mesh, int resolution, Vector3[] vertices)
    {
        this._cubeMesh = false;
        this._localUp = new Vector3(0, 0, 0);
        this._mesh = mesh;
        this._resolution = resolution;
        this._vertices = vertices;
        _axisA = new Vector3(_localUp.y, _localUp.z, _localUp.x);
        _axisB = Vector3.Cross(_localUp, _axisA);
    }
    public (Vector3[], int[]) TriangleFragmentation(Vector3[] triangle, int res)
    {
        Vector3[] vertices = new Vector3[(res + 1)*(res + 2) / 2];
        int[] triangles = new int[res * res*3];
        vertices[0] = triangle[2];
        vertices[((res+1)*(res+2)/2)-1] = triangle[1];
        vertices[res*(res+1)/2] = triangle[0];
        Vector3 u = triangle[2];
        Vector3 r = triangle[1];
        Vector3 l = triangle[0];
        int n = 0;
        int m = 0;
        for (int i = 1; i < res; i++)
        {
            n += i + 1;
            m += i;
            vertices[n] = Vector3.Lerp(u,r, (float)i/res);
            vertices[(res*(res+1)/2)+i] = Vector3.Lerp(l,r, (float)i/res);
            vertices[m] = Vector3.Lerp(u,l, (float)i/res);
            if (i != 1)
            {
                for (int j = 1; j < i; j++)
                {
                    vertices[m+j] =  Vector3.Lerp(vertices[m],vertices[n], (float)j/i);
                }
            }
        }

        int n2 = 1;
        int triCount = 0;
        for (int i = 0; i < ((res) * (res + 1) / 2); i++)
        {
            triangles[triCount] = i;
            triangles[triCount + 1] = i + n2+1;
            triangles[triCount + 2] = i + n2;
            triCount+=3;
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

    public (Vector3[],int[]) ConstructCubeMesh()
    {
        Vector3[] vertices = new Vector3[_resolution * _resolution];
        int[] triangles = new int[(_resolution-1) * (_resolution-1) * 6];
        int triIndex = 0;
        for (int y = 0; y < _resolution; y++)
        {
            for (int x = 0; x < _resolution; x++)
            { 
                int i = x + y * _resolution;
                Vector2 percent = new Vector2(x,y)/(_resolution-1);
                Vector3 pointOnUnitCube = _localUp+(percent.x - 0.5f)*2*_axisA+(percent.y - 0.5f)*2*_axisB;
                Vector3 pointOnUnitSphere = pointOnUnitCube.normalized;
                vertices[i] = pointOnUnitSphere;
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

    public void ConstructMesh()
    {
        Vector3[] pointOnUnitSphere;
        int[] triangles;
        if (!_cubeMesh)
        {
            (Vector3[], int[]) fragmentedTriangles = TriangleFragmentation(_vertices, _resolution);
            Vector3[] pointOnOctahedron = fragmentedTriangles.Item1;
            triangles = fragmentedTriangles.Item2;
            pointOnUnitSphere = pointOnOctahedron;
            for (int i = 0; i < pointOnUnitSphere.Length; i++)
            {
                pointOnUnitSphere[i] = pointOnUnitSphere[i].normalized;
            }
        }
        else
        {
            pointOnUnitSphere = ConstructCubeMesh().Item1;
            triangles = ConstructCubeMesh().Item2;
        }

    _mesh.Clear();
        _mesh.vertices = pointOnUnitSphere;
        _mesh.triangles = triangles;
        _mesh.RecalculateNormals();
    }
}
