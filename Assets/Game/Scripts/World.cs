using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour {

    List<Anchor> anchors = new List<Anchor>();

    public static World instance;

    Dictionary<Vector3Int, ChunkData> chunks = new Dictionary<Vector3Int, ChunkData>();

	private void Awake()
	{
        if(instance != null) {
            Debug.LogError("Instance of World already exists");
            this.enabled = false;
            return;
        }
        instance = this;
	}

	void Start () {
        Debug.Log("Generate World");
        GenerateWorld();
	}
	
	void Update () {
		
	}

    void GenerateWorld() {
        //Starting chunk
        ChunkData chunkData = new ChunkData();
        chunkData.SetBlock(6, 7, 6, 1);
        for (int x = 5; x < 10; x++) {
            for (int z = 5; z < 10; z++)
            {
                chunkData.SetBlock(x, 7, z, 2);
            } 
        }

        chunkData.SetBlock(7, 8, 7, 3);
        chunkData.SetBlock(7, 9, 7, 3);
        chunkData.SetBlock(7, 10, 7, 3);
        chunkData.SetBlock(7, 11, 7, 3);
		chunkData.SetBlock(6, 11, 7, 4);
        chunkData.SetBlock(7, 11, 6, 4);
        chunkData.SetBlock(7, 11, 8, 4);
        chunkData.SetBlock(8, 11, 7, 4);
        for (int x = 6; x < 9; x++)
        {
            for (int z = 6; z < 9; z++)
            {
                chunkData.SetBlock(x, 12, z, 4);
            }
        }
        chunkData.SetBlock(7, 13, 7, 4);

        chunks.Add(new Vector3Int(0, 0, 0), chunkData);
    }

    public ChunkData GetChunk(Vector3Int pos) {
        ChunkData chunkData;
        if(chunks.TryGetValue(pos, out chunkData)) {
            return chunkData;
        }
        return null;
    }

    public void RegisterAnchor(Anchor anchor)
    {
#if DEBUG
        if(anchors.Contains(anchor)) {
            Debug.LogError("Trying to add Anchor that already is in list of anchors");
            return;
        }
#endif
        anchors.Add(anchor);
    }

    public void UnregisterAnchor(Anchor anchor) {
#if DEBUG
        if (!anchors.Contains(anchor))
        {
            Debug.LogError("Trying to Remove Anchor that isn't in list of anchors");
            return;
        }
#endif
        anchors.Remove(anchor);
    }
}
