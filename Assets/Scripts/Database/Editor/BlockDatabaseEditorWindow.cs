using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

//[InitializeOnLoad]
public class BlockDatabaseEditorWindow : EditorWindow {

    [MenuItem("Window/Block Editor")]
    public static void OpenWindow()
    {
        // Get existing open window or if none, make a new one:
        BlockDatabaseEditorWindow window = EditorWindow.GetWindow<BlockDatabaseEditorWindow>("Block Editor");
        window.Show();
    }

    BlockDatabaseScriptableObject blockDatabase;
    ItemDatabaseScriptableObject itemDatabase;

    void OnEnable() {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
        blockDatabase = AssetDatabase.LoadAssetAtPath<BlockDatabaseScriptableObject>("Assets/Scripts/Database/BlockDatabase.asset");
        if(blockDatabase == null) {

            blockDatabase = ScriptableObject.CreateInstance<BlockDatabaseScriptableObject>();

            AssetDatabase.CreateAsset(blockDatabase, "Assets/Scripts/Database/BlockDatabase.asset");
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();

            Selection.activeObject = blockDatabase;
        }
        itemDatabase = AssetDatabase.LoadAssetAtPath<ItemDatabaseScriptableObject>("Assets/Scripts/Database/ItemDatabase.asset");
        minSize = new Vector2(540f, 390f);
    }

    int selected = 0;
    int dropsSelected = 0;
    bool showDrops = false;
    bool spinPreview = true;
    int manualSpinPreview = 0;
    bool showPreviewMeshLegend = false; 

	Vector2 leftScrollPosition;
    //Vector2 rightScrollPosition;
    Camera previewCamera;
    GameObject previewMesh;
    Material[] previewMaterials;

    GUIStyle leftAlignStyle;
    GUIStyle boxWhiteTextStyle;

    void OnGUI()
    {
        if(EditorApplication.isPlaying) {
            //GUI.Box(position, "");
            GUILayout.Label("Can't edit during play mode"); //TODO center this
            return;
        }

        if (blockDatabase == null)
            return;

        GUILayout.BeginHorizontal();

            DrawLeftWindow();
            GUILayout.Box("", GUILayout.Width(3f), GUILayout.Height(this.position.height - 22f));

            GUILayout.BeginVertical();
                //rightScrollPosition = GUILayout.BeginScrollView(rightScrollPosition);
                    GUILayout.BeginHorizontal();
		                BlockData block = blockDatabase.blocks[selected];
                        DrawStatsWindow(block, selected);

                        GUILayout.BeginVertical();
                            EditorGUI.BeginChangeCheck();
                            DrawTextureMap();
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (showPreviewMeshLegend == false)
                                    UpdatePreviewModelTextures(block);
                            }

                            //if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
                            //{
                                DrawPreviewWindow(block);
                            //}

                            //if (GUILayout.Button("Update Textures"))
                            //{
                            //    UpdatePreviewModelTextures(block);
                            //}
                        GUILayout.EndVertical();
                    GUILayout.EndHorizontal();
                //GUILayout.EndScrollView();
            GUILayout.EndVertical();
        GUILayout.EndHorizontal();

        //TODO indicate uncompiled through graphic or color
        if (GUILayout.Button("Compile Database"))
        {
            BuildAtlas();
			EditorUtility.SetDirty(blockDatabase);
            AssetDatabase.SaveAssets();
			SerializedObject obj = new SerializedObject(blockDatabase);
			obj.ApplyModifiedProperties();
            Debug.LogError("Block Database compiled");
        }
    }

    void DrawLeftWindow() {
        GUILayout.BeginVertical(GUILayout.Width(120f));

        if (leftAlignStyle == null)
        {
            leftAlignStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
            leftAlignStyle.alignment = TextAnchor.MiddleLeft;
        }

        leftScrollPosition = GUILayout.BeginScrollView(leftScrollPosition);
        for (int i = 0; i < blockDatabase.blocks.Count; i++)
        {
            if (GUILayout.Button(i + ": " + blockDatabase.blocks[i].name, leftAlignStyle))
            {
                selected = i;
                UpdatePreviewModelTextures(blockDatabase.blocks[i]);
            }
        }

        GUILayout.EndScrollView();

        if (GUILayout.Button("Add new"))
        {
            BlockData block = new BlockData
            {
                textures = new Texture2D[6],
                texturePosition = new BlockData.TexturePosition[6],
                solid = true,
                blockID = blockDatabase.blocks.Count
                //Add more default stuff here
            };
            blockDatabase.blocks.Add(block);
            selected = blockDatabase.blocks.Count - 1;
            UpdatePreviewModelTextures(block);
        }
        GUILayout.EndVertical();
    }

    void DrawStatsWindow(BlockData block, int index) {
        GUILayout.BeginVertical();
        EditorGUILayout.LabelField("BlockID", block.blockID.ToString());
        block.name = EditorGUILayout.TextField("Name", block.name, GUILayout.ExpandWidth(false));
        block.solid = EditorGUILayout.Toggle("Solid", block.solid, GUILayout.ExpandWidth(false));
        block.hardnessTier = (BlockData.HardnessTier)EditorGUILayout.EnumPopup("Hardness", block.hardnessTier, GUILayout.ExpandWidth(false));
        block.brightness = EditorGUILayout.IntField("Brightness", block.brightness, GUILayout.ExpandWidth(false));
        showDrops = EditorGUILayout.Foldout(showDrops, "Drops");
        if (showDrops)
        {
            EditorGUI.indentLevel++;
            if (GUILayout.Button("Add", GUILayout.ExpandWidth(false)))
            {
                block.drops.Add(new BlockData.DropData());
            }
            for (int j = 0; j < block.drops.Count; j++)
            {
                if (dropsSelected == j)
                {
                    block.drops[j].itemID = EditorGUILayout.IntField("Item ID", block.drops[j].itemID, GUILayout.ExpandWidth(false));
                    if (itemDatabase) 
                        EditorGUILayout.LabelField("Linked to: ", block.drops[j].itemID < itemDatabase.items.Count ? itemDatabase.items[block.drops[j].itemID].itemName : "None");
                    else
                        EditorGUILayout.LabelField("Linked to: ", "Can't find item database");
                    block.drops[j].amount = EditorGUILayout.IntField("Amount", block.drops[j].amount, GUILayout.ExpandWidth(false));
                    block.drops[j].percentChance = EditorGUILayout.Slider("Chance", block.drops[j].percentChance, 0f, 100f, GUILayout.ExpandWidth(false));
                }
                else
                {
                    if (GUILayout.Button(block.drops[j].itemID.ToString(), EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
                    {
                        dropsSelected = j;
                    }
                }
            }
            EditorGUI.indentLevel--;
        }
        if(blockDatabase.blocks.Count > 1) {
			if (GUILayout.Button("Delete", GUILayout.MaxWidth(100f)))
			{
				blockDatabase.blocks.RemoveAt(selected);
				selected--;
				UpdatePreviewModelTextures(blockDatabase.blocks[selected]);
				return;
			}
        }
        GUILayout.EndVertical();
    }

    void DrawPreviewWindow(BlockData block) {
        GUILayoutUtility.GetRect(144, 10f, GUILayout.ExpandWidth(false));
        var rect = GUILayoutUtility.GetRect(144f, 144f, GUILayout.ExpandWidth(false));
        if (previewCamera == null)
        {
            GameObject camObj = new GameObject("PreviewCamera");
            camObj.hideFlags = HideFlags.HideAndDontSave;
            previewCamera = camObj.AddComponent<Camera>();
            previewCamera.clearFlags = CameraClearFlags.SolidColor;
            previewCamera.cullingMask = LayerMask.GetMask("EditorBlock");

            if (EditorGUIUtility.isProSkin)
                previewCamera.backgroundColor = (Color)(new Color32(51, 51, 51, 0));
            else
                previewCamera.backgroundColor = (Color)(new Color32(135, 135, 135, 0));

            previewCamera.orthographic = true;
            previewCamera.orthographicSize = 1f;
            previewCamera.enabled = false;
        }
        if (previewMesh == null)
        {
            previewMesh = new GameObject("PreviewMesh");
            MeshFilter meshFilter = previewMesh.AddComponent<MeshFilter>();
            previewMesh.AddComponent<MeshRenderer>();
            meshFilter.mesh = new Mesh();
            previewMesh.layer = LayerMask.NameToLayer("EditorBlock");

            MeshUtility.CreatePreviewCube(meshFilter);

            previewMesh.hideFlags = HideFlags.HideAndDontSave;
            previewMesh.transform.position = previewCamera.gameObject.transform.position + previewCamera.gameObject.transform.forward * 2f;
            previewMesh.transform.Rotate(-35f, 0f, -35f);
        }
        if (previewMaterials == null)
        {
            previewMaterials = new Material[6];
            for (int j = 0; j < 6; j++)
            {
                previewMaterials[j] = new Material(Shader.Find("Unlit/Transparent"));
                previewMaterials[j].hideFlags = HideFlags.HideAndDontSave;
            }
            previewMesh.GetComponent<MeshRenderer>().materials = previewMaterials;
            UpdatePreviewModelTextures(block);
        }
        if (previewCamera)
        {
            previewCamera.Render();
            Handles.DrawCamera(rect, previewCamera, DrawCameraMode.Normal);

            EditorGUILayout.BeginHorizontal();
            bool leftButton = GUILayout.RepeatButton(EditorGUIUtility.IconContent("Animation.Reverse"), EditorStyles.miniButton, GUILayout.ExpandWidth(false), GUILayout.Height(16f));
            bool rightButton = GUILayout.RepeatButton(EditorGUIUtility.IconContent("Animation.Forward"), EditorStyles.miniButton, GUILayout.ExpandWidth(false), GUILayout.Height(16f));
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
            if (GUILayout.Button(spinPreview ? EditorGUIUtility.IconContent("Animation.Pause") : EditorGUIUtility.IconContent("Animation.Play"), EditorStyles.miniButton, GUILayout.ExpandWidth(false), GUILayout.Height(16f)))
            {
                spinPreview = !spinPreview;
            }
            if (GUILayout.Button(showPreviewMeshLegend ? "Textured" : " Legend ", EditorStyles.miniButton, GUILayout.ExpandWidth(false), GUILayout.Height(16f)))
            {
                showPreviewMeshLegend = !showPreviewMeshLegend;
                UpdatePreviewModelTextures(block);
            }

            EditorGUILayout.EndHorizontal();
        } else {
            Debug.LogError("No preview Camera");
        }
    }

    void DrawTextureMap() {
        float width = 48f;
        float height = 48f;

        GUILayoutUtility.GetRect(48 * 3f, 10f, GUILayout.ExpandWidth(false)); //A bit hacky to move away from edge
        var texturePickerRect = GUILayoutUtility.GetRect(width * 3f, height * 4f, GUILayout.ExpandWidth(false));

        Rect subRectTop = new Rect(texturePickerRect.x + texturePickerRect.width / 2f - width / 2f, texturePickerRect.y, width, height);
        DrawTextureBox("Up", subRectTop, 0);

        Rect subRectBottom = new Rect(texturePickerRect.x + texturePickerRect.width / 2f - width / 2f, texturePickerRect.y + height * 2f, width, height);
        DrawTextureBox("Down", subRectBottom, 1);

        Rect subRectLeft = new Rect(texturePickerRect.x + texturePickerRect.width / 2f - width / 2f - width, texturePickerRect.y + height, width, height);
        DrawTextureBox("Left", subRectLeft, 2);

        Rect subRectRight = new Rect(texturePickerRect.x + texturePickerRect.width / 2f - width / 2f + width, texturePickerRect.y + height, width, height);
        DrawTextureBox("Right", subRectRight, 3);

        Rect subRectFront = new Rect(texturePickerRect.x + texturePickerRect.width / 2f - width / 2f, texturePickerRect.y + height, width, height);
        DrawTextureBox("Front", subRectFront, 4);

        Rect subRectBack = new Rect(texturePickerRect.x + texturePickerRect.width / 2f - width / 2f, texturePickerRect.y + height * 3f, width, height);
        DrawTextureBox("Back", subRectBack, 5);
    }

    int currentPickerWindowControlID;
    int textureIndexPickWindow = -1;

    void DrawTextureBox(string label, Rect rect, int textureIndex) {
        if(boxWhiteTextStyle == null) {
            boxWhiteTextStyle = new GUIStyle(GUI.skin.box);
            boxWhiteTextStyle.normal.textColor = Color.black;//new Color(0.706f, 0.706f, 0.706f, 1f);
        }

        if(blockDatabase.blocks[selected].textures[textureIndex] == null) {
            GUI.DrawTexture(rect, EditorGUIUtility.IconContent("PreviewMeshEmpty").image);
        } else {
			GUI.DrawTexture(rect, blockDatabase.blocks[selected].textures[textureIndex]);
        }
        GUI.Box(rect, label, boxWhiteTextStyle);
        if (rect.Contains(Event.current.mousePosition))
        {
            if (Event.current.type == EventType.DragUpdated)
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                Event.current.Use();
            }
            else if (Event.current.type == EventType.DragPerform)
            {
                for (int i = 0; i < DragAndDrop.objectReferences.Length; i++)
                {
                    Texture2D texture = DragAndDrop.objectReferences[i] as Texture2D;
                    if (texture != null)
                    {
                        blockDatabase.blocks[selected].textures[textureIndex] = texture;
                        GUI.changed = true;
                        break;
                    }
                }
                Event.current.Use();
            }
            else if ((Event.current.type == EventType.MouseDown) && (Event.current.button == 0))
            {
                EditorGUIUtility.PingObject(blockDatabase.blocks[selected].textures[textureIndex]);
                Event.current.Use();
            }
            else if ((Event.current.type == EventType.MouseDown) && (Event.current.button == 1))
            {
                currentPickerWindowControlID = EditorGUIUtility.GetControlID(FocusType.Passive);// + 100 + textureIndex;
                EditorGUIUtility.ShowObjectPicker<Texture2D>(null, false, "", currentPickerWindowControlID);
                textureIndexPickWindow = textureIndex;
                Event.current.Use();
            }
        }
        if (
            Event.current.commandName == "ObjectSelectorClosed"
            && EditorGUIUtility.GetObjectPickerControlID() == currentPickerWindowControlID
            && textureIndexPickWindow == textureIndex
        )
        {
            blockDatabase.blocks[selected].textures[textureIndex] = EditorGUIUtility.GetObjectPickerObject() as Texture2D;
            GUI.changed = true;
            //Event.current.Use();
        }
    }

    string[] previewMeshIcons = new string[] {
        "PreviewMeshUp", "PreviewMeshDown", "PreviewMeshLeft", 
        "PreviewMeshRight", "PreviewMeshFront", "PreviewMeshBack" 
    };

    void UpdatePreviewModelTextures(BlockData block)
    {
        if(block == null) {
            return;
        }
        if(showPreviewMeshLegend) {
            previewMaterials[0].mainTexture = EditorGUIUtility.IconContent(previewMeshIcons[0]).image;
            previewMaterials[1].mainTexture = EditorGUIUtility.IconContent(previewMeshIcons[1]).image;
            previewMaterials[2].mainTexture = EditorGUIUtility.IconContent(previewMeshIcons[2]).image;
            previewMaterials[3].mainTexture = EditorGUIUtility.IconContent(previewMeshIcons[3]).image;
            previewMaterials[4].mainTexture = EditorGUIUtility.IconContent(previewMeshIcons[4]).image;
            previewMaterials[5].mainTexture = EditorGUIUtility.IconContent(previewMeshIcons[5]).image;
        } else {
			if(previewMaterials != null) {
				for (int j = 0; j < 6; j++)
				{
                    if(block.textures[j] == null) {
                        previewMaterials[j].mainTexture = EditorGUIUtility.IconContent(previewMeshIcons[j]).image;
                    } else {
						previewMaterials[j].mainTexture = block.textures[j];
                    }
				}
			}
        }
    }

    void Update()
    {
        if(previewMesh != null) {
            if(manualSpinPreview != 0) {
                previewMesh.transform.Rotate(0f, -0.175f*2f * (float)manualSpinPreview, 0f, Space.World);

            } else {
                if(spinPreview) {
					previewMesh.transform.Rotate(0f, -0.175f, 0f, Space.World);
                }
            }
        } 
        Repaint();
    }

    void BuildAtlas() {
		Texture2D texture = new Texture2D(2048, 2048, TextureFormat.RGBA32, true); //TODO make fit only to what we need

        Dictionary<Texture2D, Vector2Int> addedTextures = new Dictionary<Texture2D, Vector2Int>();

		int x = 0;
		int y = 0;
        for (int i = 0; i < blockDatabase.blocks.Count; i++)
        {
			BlockData block = blockDatabase.blocks[i];
			for(int j = 0; j < 6; j++) {
                if (block.textures[j] != null)
                {
                    if (addedTextures.ContainsKey(block.textures[j]))
                    {
                        block.texturePosition[j] = new BlockData.TexturePosition(addedTextures[block.textures[j]]);
                    }
                    else
                    {
                        addedTextures.Add(block.textures[j], new Vector2Int(x, y));
                        AddTexture(texture, block, (BlockData.Direction)j, ref x, ref y);
                    }
                }
			}
        }
        string filePath = "Assets/Scripts/Database/Atlas.png";
		byte[] bytes = texture.EncodeToPNG();
		FileStream stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
		BinaryWriter writer = new BinaryWriter(stream);
		writer.Write(bytes);
		writer.Close();
		stream.Close();
		DestroyImmediate(texture);
		AssetDatabase.Refresh();

        Material masterMaterial = AssetDatabase.LoadAssetAtPath<Material>("Assets/Scripts/Database/masterMaterial.mat");
        if(masterMaterial == null) {
            masterMaterial = new Material(Shader.Find("Transparent/Diffuse"));
            AssetDatabase.CreateAsset(masterMaterial, "Assets/Scripts/Database/masterMaterial.mat");
        }
        masterMaterial.mainTexture = AssetDatabase.LoadAssetAtPath<Texture>(filePath);
        AssetDatabase.SaveAssets();
    }

    void AddTexture(Texture2D atlas, BlockData block, BlockData.Direction direction, ref int x, ref int y) {
		atlas.SetPixels(x*32, y*32, 32, 32, block.textures[(int)direction].GetPixels());
		block.texturePosition[(int)direction] = new BlockData.TexturePosition(x,y);
		x++;
		if(x >= 64) {
			y++;
			x = 0;
		}
	}

    void OnDestroy()
    {
        if(previewCamera != null)
            DestroyImmediate(previewCamera.gameObject);
        if(previewMesh != null) {
            DestroyImmediate(previewMesh);
        }
        if(previewMaterials != null) {
			for(int i = 0; i < previewMaterials.Length; i++)
			{
				if (previewMaterials[i])
					DestroyImmediate(previewMaterials[i]);
			}
            previewMaterials = null;
        }
    }

    //Idk why the camera breaks when I go into play mode. Hacky way to just refresh it
    private void OnPlayModeChanged(PlayModeStateChange state)
    {
        if(state == PlayModeStateChange.EnteredEditMode) {
            OnDestroy();
        }
    }
}
