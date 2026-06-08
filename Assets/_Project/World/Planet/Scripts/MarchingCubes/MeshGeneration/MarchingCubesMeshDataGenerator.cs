using _Project.World.Planet.Scripts.MarchingCubes.Core;
using _Project.World.Planet.Scripts.MarchingCubes.DensitySampling;
using Unity.Mathematics;

namespace _Project.World.Planet.Scripts.MarchingCubes.MeshGeneration
{
    public static class MarchingCubesMeshDataGenerator
    {
        /// <summary>
        /// generates the mesh data for given density field.
        /// </summary>
        /// <param name="densityField">the density field used for generating the mesh</param>
        /// <returns></returns>
        public static MeshData GenerateMeshDataAt(DensityField densityField)
        {
            MeshDataBuilder meshDataBuilder = new MeshDataBuilder(); // the mesh data is used to store the vertices, normals and indices of the mesh. we use a builder to easily add new vertices and indices and then build the final mesh data at the end.
            
            int size = densityField.Size;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    for (int z = 0; z < size; z++)
                    {
                        MarchingCubesMesher.GenerateAt((new int3(x, y, z)), densityField, meshDataBuilder); // generate the mesh for every grid cell and use the mesh builder to add it all to one large mesh
                    }
                }
            }
            
            meshDataBuilder.NormalizeNormals(); // this generates smooth normals so lighting looks good and smooth on the object.
            
            return meshDataBuilder.Build(); // return the mesh data
        }
        
    }
}