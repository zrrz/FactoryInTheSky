using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshUtility : MonoBehaviour {

    public static void CreatePreviewCube(MeshFilter meshFilter)
    {
        meshFilter.sharedMesh.subMeshCount = 6;

        Mesh mesh = meshFilter.sharedMesh;
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles0 = new List<int>();
        List<int> triangles1 = new List<int>();
        List<int> triangles2 = new List<int>();
        List<int> triangles3 = new List<int>();
        List<int> triangles4 = new List<int>();
        List<int> triangles5 = new List<int>();

        //Up

        vertices.Add(new Vector3(0.5f, 0.5f, 0.5f));
        vertices.Add(new Vector3(0.5f, 0.5f, -0.5f));
        vertices.Add(new Vector3(-0.5f, 0.5f, -0.5f));
        vertices.Add(new Vector3(-0.5f, 0.5f, 0.5f));
        triangles0 = GetQuadTriangle(mesh, vertices);

        //Down

        vertices.Add(new Vector3(0.5f, -0.5f, -0.5f));
        vertices.Add(new Vector3(0.5f, -0.5f, 0.5f));
        vertices.Add(new Vector3(-0.5f, -0.5f, 0.5f));
        vertices.Add(new Vector3(-0.5f, -0.5f, -0.5f));
        triangles1 = GetQuadTriangle(mesh, vertices);

        //East
        vertices.Add(new Vector3(0.5f, -0.5f, -0.5f));
        vertices.Add(new Vector3(0.5f, 0.5f, -0.5f));
        vertices.Add(new Vector3(0.5f, 0.5f, 0.5f));
        vertices.Add(new Vector3(0.5f, -0.5f, 0.5f));
        triangles2 = GetQuadTriangle(mesh, vertices);

        //West
        vertices.Add(new Vector3(-0.5f, -0.5f, 0.5f));
        vertices.Add(new Vector3(-0.5f, 0.5f, 0.5f));
        vertices.Add(new Vector3(-0.5f, 0.5f, -0.5f));
        vertices.Add(new Vector3(-0.5f, -0.5f, -0.5f));
        triangles3 = GetQuadTriangle(mesh, vertices);

        //Front
        vertices.Add(new Vector3(0.5f, -0.5f, 0.5f));
        vertices.Add(new Vector3(0.5f, 0.5f, 0.5f));
        vertices.Add(new Vector3(-0.5f, 0.5f, 0.5f));
        vertices.Add(new Vector3(-0.5f, -0.5f, 0.5f));
        triangles4 = GetQuadTriangle(mesh, vertices);

        //South
        vertices.Add(new Vector3(-0.5f, -0.5f, -0.5f));
        vertices.Add(new Vector3(-0.5f, 0.5f, -0.5f));
        vertices.Add(new Vector3(0.5f, 0.5f, -0.5f));
        vertices.Add(new Vector3(0.5f, -0.5f, -0.5f));
        triangles5 = GetQuadTriangle(mesh, vertices);

        mesh.SetVertices(vertices);

        mesh.SetTriangles(triangles0, 0);
        mesh.SetTriangles(triangles1, 1);
        mesh.SetTriangles(triangles2, 2);
        mesh.SetTriangles(triangles3, 3);
        mesh.SetTriangles(triangles4, 4);
        mesh.SetTriangles(triangles5, 5);

        Vector2[] uvs = new Vector2[24];
        uvs[0] = new Vector2(0f, 0f); //top-left
        uvs[1] = new Vector2(0f, 1f); //top-right
        uvs[2] = new Vector2(1f, 1f); //bottom-right
        uvs[3] = new Vector2(1f, 0f); //bottom-left
        uvs[4] = new Vector2(0f, 0f); //top-left
        uvs[5] = new Vector2(0f, 1f); //top-right
        uvs[6] = new Vector2(1f, 1f); //bottom-right
        uvs[7] = new Vector2(1f, 0f); //bottom-left
        uvs[8] = new Vector2(0f, 0f); //top-left
        uvs[9] = new Vector2(0f, 1f); //top-right
        uvs[10] = new Vector2(1f, 1f); //bottom-right
        uvs[11] = new Vector2(1f, 0f); //bottom-left
        uvs[12] = new Vector2(0f, 0f); //top-left
        uvs[13] = new Vector2(0f, 1f); //top-right
        uvs[14] = new Vector2(1f, 1f); //bottom-right
        uvs[15] = new Vector2(1f, 0f); //bottom-left
        uvs[16] = new Vector2(0f, 0f); //top-left
        uvs[17] = new Vector2(0f, 1f); //top-right
        uvs[18] = new Vector2(1f, 1f); //bottom-right
        uvs[19] = new Vector2(1f, 0f); //bottom-left
        uvs[20] = new Vector2(0f, 0f); //top-left
        uvs[21] = new Vector2(0f, 1f); //top-right
        uvs[22] = new Vector2(1f, 1f); //bottom-right
        uvs[23] = new Vector2(1f, 0f); //bottom-left

        mesh.uv = uvs;

        meshFilter.mesh = mesh;
    }

    static List<int> GetQuadTriangle(Mesh mesh, List<Vector3> vertices)
    {
        List<int> triangles = new List<int>();
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 3);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 4);
        triangles.Add(vertices.Count - 2);
        triangles.Add(vertices.Count - 1);

        return triangles;
    }


	public static MeshData CreateMeshFromSprite(Sprite sprite) {
        float offsetX = -sprite.texture.width / 2f;
		MeshData meshData = new MeshData();
		for(int x = 0; x < sprite.texture.width; x++) {
			for(int y = 0; y < sprite.texture.height; y++) {
				Color color = sprite.texture.GetPixel(x,y);
				if(color.a > 0f) {
                    meshData = GetPixelData(sprite.texture, x, y, meshData, offsetX, 0f);
				}
			}
		}

		return meshData;

//		Mesh mesh = new Mesh();
//		mesh.SetVertices(meshData.vertices);
//		mesh.SetTriangles(meshData.triangles, 0);
//
//		mesh.SetUVs(0, meshData.uv);
//		filter.mesh.RecalculateNormals();
//
//		coll.sharedMesh = null;
//		Mesh mesh = new Mesh();
//		mesh.SetVertices(meshData.colVertices);
//		mesh.SetTriangles(meshData.colTriangles, 0);
//		mesh.RecalculateNormals();
//
//		coll.sharedMesh = mesh;
	}

	public static MeshData GetPixelData
	(Texture2D texture, int x, int y, MeshData meshData, float offsetX = 0f, float offsetY = 0f)
	{
//		meshData.useRenderDataForCol = true;

		if(y < texture.height - 1) {
			if(texture.GetPixel(x, y+1).a > 0) {

			} else {
                meshData = GetFaceData(texture, BlockData.Direction.Up, x, y, meshData, offsetX, offsetY);
			}
		}

        if(y > 0) {
            if(texture.GetPixel(x, y-1).a > 0) {

            } else {
                meshData = GetFaceData(texture, BlockData.Direction.Down, x, y, meshData, offsetX, offsetY);
            }
        }

        meshData = GetFaceData(texture, BlockData.Direction.South, x, y, meshData, offsetX, offsetY);

        meshData = GetFaceData(texture, BlockData.Direction.North, x, y, meshData, offsetX, offsetY);

        if(x < texture.width - 1) {
            if(texture.GetPixel(x + 1, y).a > 0) {

            } else {
                meshData = GetFaceData(texture, BlockData.Direction.East, x, y, meshData, offsetX, offsetY);
            }
        }

        if(x > 0) {
            if(texture.GetPixel(x - 1, y).a > 0) {

            } else {
                meshData = GetFaceData(texture, BlockData.Direction.West, x, y, meshData, offsetX, offsetY);
            }
        }

		return meshData;
	}

    static MeshData GetFaceData (Texture2D texture, BlockData.Direction direction, int x, int y, MeshData meshData, float offsetX = 0f, float offsetY = 0f)
	{
        float size = 1f / 32f;
        offsetX *= size;
        offsetY *= size;
        switch (direction)
        {
            case BlockData.Direction.Up:
                meshData.AddVertex(new Vector3(offsetX + x*size - size/2f, offsetY + y*size + size/2f, size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size + size/2f, offsetY + y*size + size/2f, size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size + size/2f, offsetY + y*size + size/2f, -size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size - size/2f, offsetY + y*size + size/2f, -size/2f));
                break;
            case BlockData.Direction.Down:
                meshData.AddVertex(new Vector3(offsetX + x*size - size/2f, offsetY + y*size - size/2f, -size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size + size/2f, offsetY + y*size - size/2f, -size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size + size/2f, offsetY + y*size - size/2f, size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size - size/2f, offsetY + y*size - size/2f, size/2f));
                break;
            case BlockData.Direction.North:
                meshData.AddVertex(new Vector3(offsetX + x*size + size/2f, offsetY + y*size - size/2f, size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size + size/2f, offsetY + y*size + size/2f, size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size - size/2f, offsetY + y*size + size/2f, size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size - size/2f, offsetY + y*size - size/2f, size/2f));
                break;
            case BlockData.Direction.East:
                meshData.AddVertex(new Vector3(offsetX + x*size + size/2f, offsetY + y*size - size/2f, -size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size + size/2f, offsetY + y*size + size/2f, -size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size + size/2f, offsetY + y*size + size/2f, size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size + size/2f, offsetY + y*size - size/2f, size/2f));
                break;
            case BlockData.Direction.South:
                meshData.AddVertex(new Vector3(offsetX + x*size - size/2f, offsetY + y*size - size/2f, -size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size - size/2f, offsetY + y*size + size/2f, -size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size + size/2f, offsetY + y*size + size/2f, -size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size + size/2f, offsetY + y*size - size/2f, -size/2f));
                break;
            case BlockData.Direction.West:
                meshData.AddVertex(new Vector3(offsetX + x*size - size/2f, offsetY + y*size - size/2f, size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size - size/2f, offsetY + y*size + size/2f, size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size - size/2f, offsetY + y*size + size/2f, -size/2f));
                meshData.AddVertex(new Vector3(offsetX + x*size - size/2f, offsetY + y*size - size/2f, -size/2f));
                break;
        }
		

		meshData.AddQuadTriangles();

        float tileSize = 1f / 32f;
        Vector2[] UVs = new Vector2[4];

        UVs[0] = new Vector2(tileSize * x + tileSize, tileSize * y);
        UVs[1] = new Vector2(tileSize * x + tileSize, tileSize * y + tileSize);
        UVs[2] = new Vector2(tileSize * x, tileSize * y + tileSize);
        UVs[3] = new Vector2(tileSize * x, tileSize * y);
        meshData.AddUVs(UVs);
		return meshData;
	}
}
