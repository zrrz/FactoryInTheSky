using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ItemDatabaseEditorWindow : EditorWindow {

    [MenuItem("Window/Item Editor")]
    public static void OpenWindow()
    {
        // Get existing open window or if none, make a new one:
        ItemDatabaseEditorWindow window = (ItemDatabaseEditorWindow)EditorWindow.GetWindow<ItemDatabaseEditorWindow>("Item Editor");
        window.Show();
    }

    ItemDatabaseScriptableObject itemDatabase;
    BlockDatabaseScriptableObject blockDatabase;

    Vector2 leftScrollPosition;
    GUIStyle leftAlignStyle;
    int selectedItem = 0;
    bool spinPreview = true;
    int manualSpinPreview = 0;
    float rotationY;

    Camera previewCamera;
    GameObject previewMesh;
    Material previewMaterial;
    Material[] previewMaterials;

    void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseScriptableObject>("Assets/Scripts/Database/ItemDatabase.asset");
        if (itemDatabase == null)
        {

            itemDatabase = ScriptableObject.CreateInstance<ItemDatabaseScriptableObject>();
            itemDatabase.items = new List<ItemData>();
            ItemData item = new ItemData
            {
                itemID = (ushort)itemDatabase.items.Count,
                itemName = "NewItem",
                stackSize = 64,
                //Add more default stuff here
            };
            itemDatabase.items.Add(item);
            AssetDatabase.CreateAsset(itemDatabase, "Assets/Scripts/Database/ItemDatabase.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = itemDatabase;
        }
        blockDatabase = AssetDatabase.LoadAssetAtPath<BlockDatabaseScriptableObject>("Assets/Scripts/Database/BlockDatabase.asset");
        minSize = new Vector2(540f, 300f);
    }

    void OnGUI()
    {
        if (EditorApplication.isPlaying)
        {
            //GUI.Box(position, "");
            GUILayout.Label("Can't edit during play mode"); //TODO center this
            return;
        }

        if (itemDatabase == null)
            return;

        GUILayout.BeginHorizontal();

        DrawLeftWindow();
        GUILayout.Box("", GUILayout.Width(3f), GUILayout.Height(this.position.height - 22f));

            GUILayout.BeginVertical();

                GUILayout.BeginHorizontal();

                ItemData item = itemDatabase.items[selectedItem];
                DrawStatsWindow(item, selectedItem);

                GUILayout.BeginVertical();
                    EditorGUI.BeginChangeCheck();
                    //DrawTextureMap();
                    if (item.modelType == ItemData.ModelType.Sprite)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Sprite", GUILayout.Width(40f));
                        item.sprite = (Sprite)EditorGUILayout.ObjectField(item.sprite, typeof(Sprite), false, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                    } 
                    else if (item.modelType == ItemData.ModelType.Custom)
                    {
                        GUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Model", GUILayout.Width(40f));
                        item.customModel = (GameObject)EditorGUILayout.ObjectField(item.customModel, typeof(GameObject), false, GUILayout.ExpandWidth(false));
                        GUILayout.EndHorizontal();
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        UpdatePreviewModel(item);
                    }

                    //if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
                    //{
                    DrawPreviewWindow(item);
                    //}
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

            GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Compile Database"))
        {
            //BuildAtlas();
            EditorUtility.SetDirty(itemDatabase);
            AssetDatabase.SaveAssets();
            SerializedObject obj = new SerializedObject(itemDatabase);
            obj.ApplyModifiedProperties();
            Debug.LogError("Item Database compiled");
        }
    }

    void DrawLeftWindow()
    {
        GUILayout.BeginVertical(GUILayout.Width(120f));

        if (leftAlignStyle == null)
        {
            leftAlignStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
            leftAlignStyle.alignment = TextAnchor.MiddleLeft;
        }

        leftScrollPosition = GUILayout.BeginScrollView(leftScrollPosition);
        for (int i = 0; i < itemDatabase.items.Count; i++)
        {
            if (GUILayout.Button(i + ": " + itemDatabase.items[i].itemName, leftAlignStyle))
            {
                selectedItem = i;
                UpdatePreviewModel(itemDatabase.items[i]);
            }
        }

        GUILayout.EndScrollView();

        if (GUILayout.Button("Add new"))
        {
            ItemData item = new ItemData
            {
                itemID = (ushort)itemDatabase.items.Count,
                itemName = "NewItem",
                stackSize = 64,
                //Add more default stuff here
            };
            itemDatabase.items.Add(item);
            selectedItem = itemDatabase.items.Count - 1;
            UpdatePreviewModel(item);
        }
        GUILayout.EndVertical();
    }

    void DrawStatsWindow(ItemData item, int index)
    {
        GUILayout.BeginVertical();
        EditorGUILayout.LabelField("itemID", index.ToString());
        item.itemName = EditorGUILayout.TextField("Name", item.itemName, GUILayout.ExpandWidth(false));
        item.placeable = EditorGUILayout.Toggle("Placeable", item.placeable, GUILayout.ExpandWidth(false));
        //TODO maybe auto toggle placeable if ModelType.Block
        EditorGUI.BeginChangeCheck();
        if(item.placeable) {
            item.placeableBlockID = EditorGUILayout.IntField("Block ID", item.placeableBlockID, GUILayout.ExpandWidth(false));
            if(blockDatabase)
                EditorGUILayout.LabelField("Linked to: ", item.placeableBlockID < blockDatabase.blocks.Count ? blockDatabase.blocks[item.placeableBlockID].name : "None");
            else
                EditorGUILayout.LabelField("Linked to: ", "Can't find block database"); 
        }
        item.modelType = (ItemData.ModelType)EditorGUILayout.EnumPopup("Model Type", item.modelType, GUILayout.ExpandWidth(false));
        if (EditorGUI.EndChangeCheck())
        {
            UpdatePreviewModel(item);
        }
        item.stackSize = EditorGUILayout.IntField("Stack Size", item.stackSize, GUILayout.ExpandWidth(false));

        //showDrops = EditorGUILayout.Foldout(showDrops, "Drops");
        //if (showDrops)
        //{
        //    EditorGUI.indentLevel++;
        //    if (GUILayout.Button("Add", GUILayout.ExpandWidth(false)))
        //    {
        //        block.drops.Add(new BlockData.DropData());
        //    }
        //    for (int j = 0; j < block.drops.Count; j++)
        //    {
        //        if (dropsSelected == j)
        //        {
        //            block.drops[j].itemID = EditorGUILayout.IntField("Item ID", block.drops[j].itemID, GUILayout.ExpandWidth(false));
        //            //TODO link to items
        //            //EditorGUILayout.LabelField("Linked to: ", block.drops[j].itemID < blockDatabase.blocks.Count ? blockDatabase.blocks[block.drops[j].itemID].name : "None");
        //            block.drops[j].amount = EditorGUILayout.IntField("Amount", block.drops[j].amount, GUILayout.ExpandWidth(false));
        //            block.drops[j].percentChance = EditorGUILayout.Slider("Chance", block.drops[j].percentChance, 0f, 100f, GUILayout.ExpandWidth(false));
        //        }
        //        else
        //        {
        //            if (GUILayout.Button(block.drops[j].itemID.ToString(), EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
        //            {
        //                dropsSelected = j;
        //            }
        //        }
        //    }
        //    EditorGUI.indentLevel--;
        //}
        if (itemDatabase.items.Count > 1)
        {
            if (GUILayout.Button("Delete", GUILayout.MaxWidth(100f)))
            {
                itemDatabase.items.RemoveAt(selectedItem);
                selectedItem--;
                UpdatePreviewModel(itemDatabase.items[selectedItem]);
                return;
            }
        }
        GUILayout.EndVertical();
    }

    string[] previewMeshIcons = new string[] {
        "PreviewMeshUp", "PreviewMeshDown", "PreviewMeshLeft",
        "PreviewMeshRight", "PreviewMeshFront", "PreviewMeshBack"
            };

    void UpdatePreviewModel(ItemData item)
    {
        if (previewMesh != null) 
            DestroyImmediate(previewMesh);
        
        if (item == null)
        {
            return;
        }

        if(item.modelType == ItemData.ModelType.Cube) {
            if (item.placeableBlockID >= blockDatabase.blocks.Count)
            {
                return;
            }

            BlockData block = blockDatabase.blocks[item.placeableBlockID];

            previewMesh = new GameObject("PreviewMesh");
            MeshFilter meshFilter = previewMesh.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = previewMesh.AddComponent<MeshRenderer>();
            meshRenderer.material = previewMaterial;
            meshFilter.mesh = new Mesh();

            MeshUtility.CreatePreviewCube(meshFilter);

            meshRenderer.materials = previewMaterials;

            if (previewMaterials != null)
            {
                for (int j = 0; j < 6; j++)
                {
                    if (block.textures[j] == null)
                    {
                        previewMaterials[j].mainTexture = EditorGUIUtility.IconContent(previewMeshIcons[j]).image;
                    }
                    else
                    {
                        previewMaterials[j].mainTexture = block.textures[j];
                    }
                }
            }

            previewMesh.hideFlags = HideFlags.HideAndDontSave;
            previewMesh.transform.position = previewCamera.gameObject.transform.position + previewCamera.gameObject.transform.forward * 2f;
            previewMesh.transform.rotation = Quaternion.Euler(-35f, rotationY, -35f);
        } 
        else if (item.modelType == ItemData.ModelType.Sprite)
        {
            if (item.sprite == null)
            {
                return;
            }
            previewMesh = CreateSprite(item.sprite);
            previewMesh.transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            previewMesh.transform.position =
                       previewCamera.gameObject.transform.position
                       + previewCamera.gameObject.transform.forward * 2f
                       + previewCamera.gameObject.transform.up * -0.5f;
        }
        else if (item.modelType == ItemData.ModelType.Custom)
        {
            if (item.customModel == null)
            {
                return;
            }
            previewMesh = GameObject.Instantiate(item.customModel);
            previewMesh.transform.position =
                       previewCamera.gameObject.transform.position
                           + previewCamera.gameObject.transform.forward * 2f;
            previewMesh.transform.rotation = Quaternion.Euler(-35f, rotationY, -35f);
        }
        previewMesh.layer = LayerMask.NameToLayer("EditorItem");
    }

    GameObject CreateSprite(Sprite sprite)
    {
        MeshData meshData = MeshUtility.CreateMeshFromSprite(sprite);

        GameObject obj = new GameObject("PreviewSprite");
        MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
        //if(Application.isPlaying) {
        //    meshRenderer.material = new Material(previewMaterial);
        //    meshRenderer.material.mainTexture = sprite.texture;
        //}
        //else {
            meshRenderer.sharedMaterial = previewMaterial;
            meshRenderer.sharedMaterial.mainTexture = sprite.texture;
            obj.hideFlags = HideFlags.HideAndDontSave;
        //}

        MeshFilter filter = obj.AddComponent<MeshFilter>();

        //if(Application.isPlaying) {
        //    filter.mesh = new Mesh();
        //    filter.mesh.Clear();
        //    filter.mesh.vertices = meshData.vertices.ToArray();
        //    filter.mesh.triangles = meshData.triangles.ToArray();

        //    filter.mesh.uv = meshData.uv.ToArray();
        //    filter.mesh.RecalculateNormals();
        //} else {
            filter.sharedMesh = new Mesh();
            filter.sharedMesh.Clear();
            filter.sharedMesh.vertices = meshData.vertices.ToArray();
            filter.sharedMesh.triangles = meshData.triangles.ToArray();

            filter.sharedMesh.uv = meshData.uv.ToArray();
            filter.sharedMesh.RecalculateNormals();
        //}

        Vector3 size = obj.AddComponent<BoxCollider>().size;
        size.z *= 2f;
        obj.GetComponent<BoxCollider>().size = size;

        return obj;
    }

    void DrawPreviewWindow(ItemData item)
    {
        var rect = GUILayoutUtility.GetRect(144f, 144f, GUILayout.ExpandWidth(false));
        if (previewCamera == null)
        {
            GameObject camObj = new GameObject("PreviewCamera");
            camObj.hideFlags = HideFlags.HideAndDontSave;
            previewCamera = camObj.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.cullingMask = LayerMask.GetMask("EditorItem");

            if (EditorGUIUtility.isProSkin)
                previewCamera.backgroundColor = (Color)(new Color32(51, 51, 51, 0));
            else
                previewCamera.backgroundColor = (Color)(new Color32(135, 135, 135, 0));

            previewCamera.orthographic = true;
            previewCamera.orthographicSize = 1f;
            previewCamera.enabled = false;
        }
        if (previewMaterial == null)
        {
            previewMaterial = new Material(Shader.Find("Unlit/Transparent"));
            previewMaterial.hideFlags = HideFlags.HideAndDontSave;

            UpdatePreviewModel(item);
        }
        if(previewMaterials == null) {
            previewMaterials = new Material[6];
            for (int j = 0; j < 6; j++)
            {
                previewMaterials[j] = new Material(Shader.Find("Unlit/Transparent"));
                previewMaterials[j].hideFlags = HideFlags.HideAndDontSave;
            }
        }
        if (previewCamera)
        {
            previewCamera.Render();
            Handles.DrawCamera(rect, previewCamera, DrawCameraMode.Normal);

            EditorGUILayout.BeginHorizontal();
            bool leftButton = GUILayout.RepeatButton(EditorGUIUtility.IconContent("Animation.Reverse"), EditorStyles.miniButton, GUILayout.ExpandWidth(false));
            bool rightButton = GUILayout.RepeatButton(EditorGUIUtility.IconContent("Animation.Forward"), EditorStyles.miniButton, GUILayout.ExpandWidth(false));
            if (leftButton)
            {
                manualSpinPreview = -1;
                spinPreview = false;
            }
            else if (rightButton)
            {
                manualSpinPreview = 1;
                spinPreview = false;
            }
            else
            {
                manualSpinPreview = 0;
            }
            if (GUILayout.Button(spinPreview ? EditorGUIUtility.IconContent("Animation.Pause") : EditorGUIUtility.IconContent("Animation.Play"), EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
            {
                spinPreview = !spinPreview;
            }
            //if (GUILayout.Button(showPreviewMeshLegend ? "Textured" : " Legend ", EditorStyles.miniButton, GUILayout.ExpandWidth(false), GUILayout.Height(18f)))
            //{
            //    showPreviewMeshLegend = !showPreviewMeshLegend;
            //    UpdatePreviewModelTextures(block);
            //}

            EditorGUILayout.EndHorizontal();
        }
        else
        {
            Debug.LogError("No preview Camera");
        }
    }

    void OnDestroy()
    {
        if (previewCamera != null)
            DestroyImmediate(previewCamera.gameObject);
        if (previewMesh != null)
        {
            DestroyImmediate(previewMesh);
        }
       
        if (previewMaterial != null)
            DestroyImmediate(previewMaterial);
        
        if (previewMaterials != null)
        {
            for (int i = 0; i < previewMaterials.Length; i++)
            {
                if (previewMaterials[i] != null)
                    DestroyImmediate(previewMaterials[i]);
            }
            previewMaterials = null;
        }
    }

    void Update()
    {
        if (previewMesh != null)
        {
            if (manualSpinPreview != 0)
            {
                previewMesh.transform.Rotate(0f, -0.175f * 2f * (float)manualSpinPreview, 0f, Space.World);
                rotationY = previewMesh.transform.eulerAngles.y;
            }
            else
            {
                if (spinPreview)
                {
                    previewMesh.transform.Rotate(0f, -0.175f, 0f, Space.World);
                    rotationY = previewMesh.transform.eulerAngles.y;
                }
            }
        }
        Repaint();
    }

    //Idk why the camera breaks when I go into play mode. Hacky way to just refresh it
    private void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredEditMode)
        {
            OnDestroy();
        }
    }
}
