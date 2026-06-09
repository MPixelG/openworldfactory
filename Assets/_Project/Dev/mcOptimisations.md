
here im planning all architecture, performance and optimisations for the marching cubes and chunking system.
it will act as a blackboard for me to write down all the ideas and plans for the system.

# Architecture
## Abstraction and modularity
- [x] split MarchingCubesChunk into 
  - ChunkData: holds all the data for the chunk, such as the density values, vertices, normals, etc.
  - ChunkRenderer: responsible for rendering the chunk, it will take the data from ChunkData and create the mesh. this will be a MonoBehaviour
  - MeshData: a struct that holds vertices, normals, indices, uvs, etc.
- [x] Convert the MarchingCubesGrid class into 
  - DensityField: only responsible for storing the density values and providing methods to access them
  - IDensitySampler: an interface that defines a method for sampling the density field. (i currently already have the abstract class TerrainGenerator but an interface would be more fitting imo)
  - DensityFieldBuilder: a class that takes an IDensitySampler and builds the DensityField by sampling the density values.
- [x] Split the MarchingCubesGenerator class into
  - MarchingCubesMesher: responsible for taking the density field and generating the mesh data (vertices, normals, indices, etc.)
  - UnityMeshBuilder: takes the MeshData and converts it into a Unity Mesh
- [x] Split the GridChunkSystem into
  - ChunkManager: responsible for managing the chunks, it will keep track of which chunks are loaded, which ones need to be created or destroyed, and handle the communication between the ChunkData and ChunkRenderer.
  - ChunkStreamer: responsible for streaming the chunks in and out based on the player's position and view distance. it will request the ChunkManager to create or destroy chunks as needed.
  - ChunkGenerator: responsible for creating the ChunkData for a given chunk coordinate. 
  - ChunkGridRenderer: responsible for rendering the chunks in the grid
- [x] Use Modifiers for density generation so that you can stack multiple modifiers on top of each other to create more complex terrain. for example, you could have a base noise modifier, then a cave modifier that subtracts from the density field, and a river modifier that creates a river by subtracting from the density field in a specific area.
- [x] Convert GridChunkSystem to 
  - ChunkStreamer: responsible for checking what chunks should be loaded based on the player's position and view distance
  - ChunkManager: responsible for managing everything. it will be the only class that contains a Dictionary of the chunk coordinates and their corresponding ChunkData. it will handle the creation and destruction of chunks, as well as the communication to the renderer.
  - GridChunkSystemRenderer: responsible for rendering the chunks in the grid. it will take the data via dependency injection from the ChunkManager and render the chunks accordingly. it is only responsible for rendering and doesnt contain any logic.


## Data management
- [x] use a 1D array for storing density values instead of a 3D array because unity doesnt handle 3D arrays well
- [ ] better approach for vertex deduplication, currently im using a dictionary to store the vertices and their indices
