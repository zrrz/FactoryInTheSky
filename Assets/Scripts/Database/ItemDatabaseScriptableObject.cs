using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabaseScriptableObject : ScriptableObject {
    [SerializeField]
    public List<ItemData> items;
}
