namespace _Project.World.Planet.Scripts.Chunking.Core
{
    public enum ChunkState
    {
        Unloaded,
        Queued,
        GeneratingDensity,
        Meshing,
        WaitingForMeshUpload,
        UploadingMesh,
        Ready,
    }
}