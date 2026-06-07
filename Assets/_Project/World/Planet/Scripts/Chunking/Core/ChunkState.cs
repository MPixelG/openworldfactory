namespace _Project.World.Planet.Scripts.Chunking.Core
{
    /// <summary>
    /// 
    /// </summary>
    public enum ChunkState
    {
        Unloaded,
        Queued,
        GeneratingDensity,
        Meshing,
        WaitingForMeshUpload, // if the mesh is done the algorithm needs to wait until all the logic is done processing until the unity part is being done. this includes the uploading of the mesh to unity. in the gap between this instance of the logic being done and the graphics being processed the chunk is in this state.
        UploadingMesh,
        Ready,
    }
}