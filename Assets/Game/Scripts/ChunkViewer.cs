using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkViewer : MonoBehaviour {

    List<ChunkRenderer> chunkRenderers = new List<ChunkRenderer>();
    Dictionary<Vector3Int, ChunkRenderer> chunkRendererMap = new Dictionary<Vector3Int, ChunkRenderer>();

	void Start () {
		
	}
	
	void Update () {
        Vector3Int chunkPos = new Vector3Int(
            (int)(transform.position.x / ChunkData.CHUNK_SIZE),
            (int)(transform.position.y / ChunkData.CHUNK_SIZE),
            (int)(transform.position.z / ChunkData.CHUNK_SIZE)
        );
        if(!chunkRendererMap.ContainsKey(chunkPos)) {
			ChunkData chunkData = World.instance.GetChunk(chunkPos);
			if(chunkData != null) {
				CreateChunkRenderer(chunkPos, chunkData);
			}
        }

        //if(World.instance.GetChunk(chunkPos))

	}

    void CreateChunkRenderer(Vector3Int chunkPos, ChunkData chunkData) {
        Debug.Log("Creating chunk");
        GameObject newChunk = new GameObject(chunkPos.ToString());
        newChunk.gameObject.layer = LayerMask.NameToLayer("Blocks");
        ChunkRenderer chunkRenderer = newChunk.AddComponent<ChunkRenderer>();
        chunkRenderer.Initialize(chunkPos, chunkData);
        chunkRenderers.Add(chunkRenderer);
        chunkRendererMap.Add(chunkPos, chunkRenderer);
    }
}
