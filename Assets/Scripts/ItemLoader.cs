using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemLoader : MonoBehaviour {

    [SerializeField]
    ItemData[] itemArray;

    Dictionary<int, ItemData> items;

    static ItemLoader instance;

    public Material cubeMaterial;

    void Awake() {
        if(instance != null) {
            Debug.LogError("Already a Item in scene. Disabling", this);
            this.enabled = false;
            return;
        }
        instance = this;

        InitializeItems();
    }

    void InitializeItems() {
        items = new Dictionary<int, ItemData>();
        for(int i = 0; i < itemArray.Length; i++) {
            if(items.ContainsKey(itemArray[i].itemID)) {
                Debug.LogError("DUPLICATE BLOCK ID. SKIPPING ITEM");
            } else {
                items.Add(itemArray[i].itemID, itemArray[i]);
            }
        }
    }

    //TODO uuuuuuuuuugh replace this.
    public static ItemData GetItemData(int ID) {
        ItemData item;
        if(instance.items.TryGetValue(ID, out item)) {
			return item;
        } else {
            Debug.LogError("Item ID: " + ID + " not found.");
            return null;
        }
    }

    public static ItemData CreateItem(int ID) {
        ItemData item;
        if(instance.items.TryGetValue(ID, out item)) {
			return Clone(item);
        } else {
            Debug.LogError("Item ID: " + ID + " not found.");
            return null;
        }
    }

	//TODO maybe show multiple of the item depending on stack size
	public static GameObject CreateModel(int ID) {
		ItemData item;
		if(instance.items.TryGetValue(ID, out item)) {
            GameObject itemObj;
            if (item.modelType == ItemData.ModelType.Cube)
            {
                itemObj = CreateCube(ID);
                itemObj.transform.localScale = Vector3.one * 0.4f;
//                List<Vector2> uvs = new List<Vector2>();
//                uvs.AddRange(FaceUVs(ID, BlockInstance.Direction.Down));
//                uvs.AddRange(FaceUVs(ID, BlockInstance.Direction.Up));
//                uvs.AddRange(FaceUVs(ID, BlockInstance.Direction.South));
//                uvs.AddRange(FaceUVs(ID, BlockInstance.Direction.North));
//                uvs.AddRange(FaceUVs(ID, BlockInstance.Direction.West));
//                uvs.AddRange(FaceUVs(ID, BlockInstance.Direction.East));
//                itemObj.GetComponent<MeshFilter>().mesh.uv = uvs.ToArray();
//                itemObj.GetComponent<MeshFilter>().mesh.RecalculateNormals();
                //TODO fix. This is really bad. 1 Drawcall per and recreating each time
            }
            else if (item.modelType == ItemData.ModelType.Sprite)
            {
                itemObj = CreateSprite(ItemLoader.GetItemData(ID).sprite);
                itemObj.transform.localScale = Vector3.one * 0.45f;
            }
            else if (item.modelType == ItemData.ModelType.Custom)
            {
                itemObj = ((GameObject)Instantiate(item.customModel));
            }
            else
            {
                itemObj = null;
                Debug.LogError("Unknown Model Type");
                return null;
            }
			ItemPickup itemPickup = itemObj.AddComponent<ItemPickup>();
			itemPickup.itemID = ID;
			itemPickup.amount = 1;
            itemObj.AddComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
            itemObj.GetComponent<Rigidbody>().collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			return itemObj;
		} else {
			Debug.LogError("Item ID: " + ID + " not found.");
			return null;
		}
	}

    //Man I don't like this
    static ItemData Clone(ItemData item) {
        ItemData newItem = new ItemData();
        newItem.itemID = item.itemID;
        newItem.stackSize = item.stackSize;
        newItem.itemName = item.itemName;
        //        item.amount = amount; Done after the clone

        newItem.slot = item.slot;
        newItem.sprite = item.sprite;
        newItem.customModel = item.customModel;
        newItem.placeable = item.placeable;
        newItem.placeableBlockID = item.placeableBlockID;
		return newItem;
    }

    static GameObject CreateSprite(Sprite sprite) {
        MeshData meshData = MeshUtility.CreateMeshFromSprite(sprite);

        GameObject obj = new GameObject("Sprite");
        obj.AddComponent<MeshRenderer>().material = new Material(instance.cubeMaterial);
        obj.GetComponent<MeshRenderer>().material.mainTexture = sprite.texture;
//        instance.cubeMaterial;
        MeshFilter filter = obj.AddComponent<MeshFilter>();

        filter.mesh.Clear();
        filter.mesh.vertices = meshData.vertices.ToArray();
        filter.mesh.triangles = meshData.triangles.ToArray();

        filter.mesh.uv = meshData.uv.ToArray();
        filter.mesh.RecalculateNormals();

        Vector3 size = obj.AddComponent<BoxCollider>().size;
        size.z *= 2f;
        obj.GetComponent<BoxCollider>().size = size;

        return obj;
    }

    static GameObject CreateCube(int blockID)
    {
        //int ID = (int)blockType;

        MeshData meshData = new MeshData();

        meshData.useRenderDataForCol = true;

        meshData = FaceData(blockID, BlockData.Direction.Down, meshData);
        meshData = FaceData(blockID, BlockData.Direction.Up, meshData);
        meshData = FaceData(blockID, BlockData.Direction.South, meshData);
        meshData = FaceData(blockID, BlockData.Direction.North, meshData);
        meshData = FaceData(blockID, BlockData.Direction.West, meshData);
        meshData = FaceData(blockID, BlockData.Direction.East, meshData);

        GameObject obj = new GameObject("Cube");
        obj.AddComponent<MeshRenderer>().material = instance.cubeMaterial;
        MeshFilter filter = obj.AddComponent<MeshFilter>();

        filter.mesh.Clear();
        filter.mesh.vertices = meshData.vertices.ToArray();
        filter.mesh.triangles = meshData.triangles.ToArray();

        filter.mesh.uv = meshData.uv.ToArray();
        filter.mesh.RecalculateNormals();

        obj.AddComponent<BoxCollider>();
//        coll.sharedMesh = null;
//        Mesh mesh = new Mesh();
//        mesh.vertices = meshData.colVertices.ToArray();
//        mesh.triangles = meshData.colTriangles.ToArray();
//        mesh.RecalculateNormals();

        return obj;
    }

    static MeshData FaceData(int ID, BlockData.Direction direction, MeshData meshData)
    {
        switch (direction)
        {
            case BlockData.Direction.Up:
                meshData.AddVertex(new Vector3(-0.5f, 0.5f, 0.5f));
                meshData.AddVertex(new Vector3(0.5f, 0.5f, 0.5f));
                meshData.AddVertex(new Vector3(0.5f, 0.5f, -0.5f));
                meshData.AddVertex(new Vector3(-0.5f, 0.5f, -0.5f));
                break;
            case BlockData.Direction.Down:
                meshData.AddVertex(new Vector3(-0.5f, -0.5f, -0.5f));
                meshData.AddVertex(new Vector3(0.5f, -0.5f, -0.5f));
                meshData.AddVertex(new Vector3(0.5f, -0.5f, 0.5f));
                meshData.AddVertex(new Vector3(-0.5f, -0.5f, 0.5f));
                break;
            case BlockData.Direction.North:
                meshData.AddVertex(new Vector3(0.5f, -0.5f, 0.5f));
                meshData.AddVertex(new Vector3(0.5f, 0.5f, 0.5f));
                meshData.AddVertex(new Vector3(-0.5f, 0.5f, 0.5f));
                meshData.AddVertex(new Vector3(-0.5f, -0.5f, 0.5f));
                break;
            case BlockData.Direction.South:
                meshData.AddVertex(new Vector3(-0.5f, -0.5f, -0.5f));
                meshData.AddVertex(new Vector3(-0.5f, 0.5f, -0.5f));
                meshData.AddVertex(new Vector3(0.5f, 0.5f, -0.5f));
                meshData.AddVertex(new Vector3(0.5f, -0.5f, -0.5f));
                break;
            case BlockData.Direction.East:
                meshData.AddVertex(new Vector3(0.5f, -0.5f, -0.5f));
                meshData.AddVertex(new Vector3(0.5f, 0.5f, -0.5f));
                meshData.AddVertex(new Vector3(0.5f, 0.5f, 0.5f));
                meshData.AddVertex(new Vector3(0.5f, -0.5f, 0.5f));
                break;
            case BlockData.Direction.West:
                meshData.AddVertex(new Vector3(-0.5f, -0.5f, 0.5f));
                meshData.AddVertex(new Vector3(-0.5f, 0.5f, 0.5f));
                meshData.AddVertex(new Vector3(-0.5f, 0.5f, -0.5f));
                meshData.AddVertex(new Vector3(-0.5f, -0.5f, -0.5f));
                break;
            default:
                break;
        }   

        meshData.AddQuadTriangles();
        meshData.uv.AddRange(FaceUVs(ID, direction));
        return meshData;
    }

    static Vector2[] FaceUVs(int ID, BlockData.Direction direction)
    {
        Vector2[] UVs = new Vector2[4];

        BlockData.TexturePosition tilePos;
        if (BlockLoader.GetBlock(ID).texturePosition != null && BlockLoader.GetBlock(ID).texturePosition.Length > (int)direction)
        {
            tilePos = BlockLoader.GetBlock(ID).texturePosition[(int)direction];
        }
        else
        {
            tilePos = new BlockData.TexturePosition(0, 0);
        }

        UVs[0] = new Vector2(BlockData.TILE_SIZE * tilePos.x + BlockData.TILE_SIZE,
             BlockData.TILE_SIZE * tilePos.y);
        UVs[1] = new Vector2(BlockData.TILE_SIZE * tilePos.x + BlockData.TILE_SIZE,
             BlockData.TILE_SIZE * tilePos.y + BlockData.TILE_SIZE);
        UVs[2] = new Vector2(BlockData.TILE_SIZE * tilePos.x,
             BlockData.TILE_SIZE * tilePos.y + BlockData.TILE_SIZE);
        UVs[3] = new Vector2(BlockData.TILE_SIZE * tilePos.x,
             BlockData.TILE_SIZE * tilePos.y);

        return UVs;
    }
}
