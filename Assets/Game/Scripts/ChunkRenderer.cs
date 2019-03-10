using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkRenderer : MonoBehaviour {

    public ChunkData chunkData;
    Vector3Int worldPos;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    MeshCollider meshCollider;

    public void Initialize(Vector3Int worldPos, ChunkData chunkData) {
        this.worldPos = worldPos;
        this.chunkData = chunkData;

        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();

        meshRenderer.material = new Material(Shader.Find("Standard"));

        meshCollider = gameObject.AddComponent<MeshCollider>();

        Mesh mesh = CreateMesh();
        meshFilter.mesh = mesh;
    }

    Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int>triangles = new List<int>();
        List<Vector2>uvs = new List<Vector2>();

        UnityEngine.Profiling.Profiler.BeginSample("Meshing");
        for (int x = 0; x < ChunkData.CHUNK_SIZE; x++)
        {
            for (int y = 0; y < ChunkData.CHUNK_SIZE; y++)
            {
                for (int z = 0; z < ChunkData.CHUNK_SIZE; z++)
                {
                    int index = x * ChunkData.CHUNK_SIZE * ChunkData.CHUNK_SIZE + y * ChunkData.CHUNK_SIZE + z;
                    int blockID = (int)chunkData.GetBlock(x, y, z);
                    //if(blockID != 0)
                        //Debug.LogError("Block at: " + x + "," + y + "," + z + " is " + (BlockType)blockID);

                    if (x < ChunkData.CHUNK_SIZE - 1) //EAST
                    {
                        int eastBlock = (int)chunkData.GetBlock(x+1, y, z);// blocks[index + 1 * ChunkData.CHUNK_SIZE * ChunkData.CHUNK_SIZE];
                        if (eastBlock != blockID)
                        { //If block to East is solid
                          //Add face east
                            vertices.AddRange(new Vector3[] {
                                new Vector3(x + 0.5f, y - 0.5f, z - 0.5f),
                                new Vector3(x + 0.5f, y + 0.5f, z - 0.5f),
                                new Vector3(x + 0.5f, y + 0.5f, z + 0.5f),
                                new Vector3(x + 0.5f, y - 0.5f, z + 0.5f)});

                            int verticesCount = vertices.Count;

                            if (eastBlock > blockID)
                            {
                                triangles.AddRange(new int[] {
                                    verticesCount - 3,
                                    verticesCount - 4,
                                    verticesCount - 2,
                                    verticesCount - 2,
                                    verticesCount - 4,
                                    verticesCount - 1
                                });
                            }
                            else
                            {
                                triangles.AddRange(new int[] {
                                    verticesCount - 4,
                                    verticesCount - 3,
                                    verticesCount - 2,
                                    verticesCount - 4,
                                    verticesCount - 2,
                                    verticesCount - 1
                                });
                            }
                        }
                    }

                    if (y < ChunkData.CHUNK_SIZE - 1) //UP
                    {
                        int upBlock = (int)chunkData.GetBlock(x, y+1, z);// blocks[index + 1 * ChunkData.CHUNK_SIZE];
                        if (upBlock != blockID)
                        { //If block to up is solid
                          //Add face up
                            vertices.AddRange(new Vector3[] {
                                new Vector3(x - 0.5f, y + 0.5f, z + 0.5f),
                                new Vector3(x + 0.5f, y + 0.5f, z + 0.5f),
                                new Vector3(x + 0.5f, y + 0.5f, z - 0.5f),
                                new Vector3(x - 0.5f, y + 0.5f, z - 0.5f)});

                            int verticesCount = vertices.Count;

                            if (upBlock > blockID)
                            {
                                triangles.AddRange(new int[] {
                                    verticesCount - 3,
                                    verticesCount - 4,
                                    verticesCount - 2,
                                    verticesCount - 2,
                                    verticesCount - 4,
                                    verticesCount - 1
                                });
                            }
                            else
                            {
                                triangles.AddRange(new int[] {
                                    verticesCount - 4,
                                    verticesCount - 3,
                                    verticesCount - 2,
                                    verticesCount - 4,
                                    verticesCount - 2,
                                    verticesCount - 1
                                });
                            }
                        }
                    }

                    if (z < ChunkData.CHUNK_SIZE - 1) //NORTH
                    {
                        int northBlock = (int)chunkData.GetBlock(x, y, z+1);// blocks[index + 1];
                        if (northBlock != blockID)
                        { //If block to up is solid
                          //Add face up
                            vertices.AddRange(new Vector3[] {
                                new Vector3(x + 0.5f, y - 0.5f, z + 0.5f),
                                new Vector3(x + 0.5f, y + 0.5f, z + 0.5f),
                                new Vector3(x - 0.5f, y + 0.5f, z + 0.5f),
                                new Vector3(x - 0.5f, y - 0.5f, z + 0.5f)});

                            int verticesCount = vertices.Count;

                            if (northBlock > blockID)
                            {
                                triangles.AddRange(new int[] {
                                    verticesCount - 3,
                                    verticesCount - 4,
                                    verticesCount - 2,
                                    verticesCount - 2,
                                    verticesCount - 4,
                                    verticesCount - 1
                                });
                            }
                            else
                            {
                                triangles.AddRange(new int[] {
                                    verticesCount - 4,
                                    verticesCount - 3,
                                    verticesCount - 2,
                                    verticesCount - 4,
                                    verticesCount - 2,
                                    verticesCount - 1
                                });
                            }
                        }
                    }
                }
            }
        }
        UnityEngine.Profiling.Profiler.EndSample();

        //UnityEngine.Profiling.Profiler.BeginSample("Uploading");
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        //Debug.LogError("verts: " + vertices.Count);

        //mesh.SetNormals
        //mesh.SetUVs(uvs);
        mesh.RecalculateNormals();
        //UnityEngine.Profiling.Profiler.EndSample();

        meshCollider.sharedMesh = mesh;

        return mesh;
    }
}
