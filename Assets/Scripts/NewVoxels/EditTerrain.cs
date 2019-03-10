using UnityEngine;
using System.Collections;

public static class EditTerrain
{
    public static Vector3Int GetBlockPos(Vector3 pos)
    {
        Vector3Int blockPos = new Vector3Int(
            Mathf.RoundToInt(pos.x),
            Mathf.RoundToInt(pos.y),
            Mathf.RoundToInt(pos.z)
            );

        return blockPos;
    }

    public static Vector3Int GetBlockPos(RaycastHit hit, bool adjacent = false)
    {
        Vector3 pos = new Vector3(
            MoveWithinBlock(hit.point.x, hit.normal.x, adjacent),
            MoveWithinBlock(hit.point.y, hit.normal.y, adjacent),
            MoveWithinBlock(hit.point.z, hit.normal.z, adjacent)
            );

        return GetBlockPos(pos);
    }

    static float MoveWithinBlock(float pos, float norm, bool adjacent = false)
    {
        if (pos - (int)pos == 0.5f || pos - (int)pos == -0.5f)
        {
            if (adjacent)
            {
                pos += (norm / 2);
            }
            else
            {
                pos -= (norm / 2);
            }
        }

        return pos;
    }

	public static bool BreakBlock(RaycastHit hit, bool adjacent = false) {
        ChunkRenderer chunkRenderer = hit.collider.GetComponent<ChunkRenderer>();
		if (chunkRenderer == null)
			return false;

        Vector3Int pos = GetBlockPos(hit, adjacent);

        int blockID = chunkRenderer.chunkData.GetBlock(pos.x, pos.y, pos.z) ;
        BlockLoader.GetBlock(blockID).Break(new Vector3(pos.x, pos.y, pos.z));
        chunkRenderer.chunkData.SetBlock(pos.x, pos.y, pos.z, 0);

		return true;
	}

    public static bool PlaceBlock(RaycastHit hit, int blockID, bool adjacent = false) {
        ChunkRenderer chunkRenderer = hit.collider.GetComponent<ChunkRenderer>();
        if (chunkRenderer == null)
            return false;

//        hit.point += hit.normal;
        Vector3Int pos = GetBlockPos(hit, adjacent);

        chunkRenderer.chunkData.SetBlock(pos.x, pos.y, pos.z, blockID);

        return true;
    }

    public static bool SetBlock(RaycastHit hit, int blockID, BlockData blockData, bool adjacent = false)
    {
        ChunkRenderer chunk = hit.collider.GetComponent<ChunkRenderer>();
        if (chunk == null)
            return false;

        Vector3Int pos = GetBlockPos(hit, adjacent);

        chunk.chunkData.SetBlock(pos.x, pos.y, pos.z, blockID);
        //chunk.world.SetBlockData(pos.x, pos.y, pos.z, blockData);

        return true;
    }

    public static int GetBlockID(RaycastHit hit, bool adjacent = false)
    {
        ChunkRenderer chunk = hit.collider.GetComponent<ChunkRenderer>();
        if (chunk == null)
            return 0;

        Vector3Int pos = GetBlockPos(hit, adjacent);


        return chunk.chunkData.GetBlock(pos.x, pos.y, pos.z);
    }
}