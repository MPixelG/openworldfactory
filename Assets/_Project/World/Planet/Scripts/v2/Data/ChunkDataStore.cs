using Unity.Collections;

namespace _Project.World.Planet.Scripts.v2.Data
{
    public class ChunkDataStore
    {
        private NativeHashMap<ulong, ChunkData> _chunks;
        private NativeHashMap<ulong, ChunkPayload> _chunkPayloads;

        public ChunkDataStore(NativeHashMap<ulong, ChunkData> chunks, NativeHashMap<ulong, ChunkPayload> chunkPayloads)
        {
            _chunks = chunks;
            _chunkPayloads = chunkPayloads;
        }
        public ChunkDataStore(int initialCapacity=1024)
        {
            _chunks = new NativeHashMap<ulong, ChunkData>(initialCapacity, Allocator.Persistent);
            _chunkPayloads = new NativeHashMap<ulong, ChunkPayload>(initialCapacity, Allocator.Persistent);
        }
        

        public void SetChunkPayloadAt(ulong mortonCode, ChunkPayload payload)
        {
            _chunkPayloads[mortonCode] = payload;
        }
        
        public ChunkPayload? GetChunkPayloadAt(ulong mortonCode)
        {
            bool containsValue = _chunkPayloads.TryGetValue(mortonCode, out ChunkPayload payload);
            return containsValue ? payload : null;
        }

        public void SetLODAt(ulong mortonCode, byte lod)
        {
            ChunkData chunkData = _chunks.ContainsKey(mortonCode) ? _chunks[mortonCode] : new ChunkData(){MortonCode = mortonCode, LOD = lod};
            chunkData.LOD = lod;
            
            _chunks[mortonCode] = chunkData;
        }

        public byte? GetLODAt(ulong mortonCode)
        {
            bool containsValue = _chunks.TryGetValue(mortonCode, out ChunkData data);
            return containsValue ? data.LOD : null;
        }
        
        public void Clear()
        {
            _chunks.Clear();
            _chunkPayloads.Clear();
        }
        
        public void Dispose()
        {
            _chunks.Dispose();
            _chunkPayloads.Dispose();
        }
    }
}