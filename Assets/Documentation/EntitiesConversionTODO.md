## Steps
1. Move all edit-time logic out of monobehaviours and into corresponding editor classes.
1. Move all runtime logic into component systems.
1. Add Conversion Systems to migrate data from monobehaviours to components.

## Conversion Notes
* HexFeatureManager (MonoBehaviour) -> FeatureGenerator
* HexGridChunk (MonoBehaviour) -> ChunkBuilder
* HexMesh (MonoBehaviour) -> ChunkMeshBuilder
* HexGrid (MonoBehaviour) -> ??? (TODO)

* Mesh generation is driven by Editor classes; MonoBehaviours just hold data.

### Kill HexMetrics class
* Replace with interfaces based around various concepts, like mesh deformation, wall information, etc.