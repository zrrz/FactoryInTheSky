using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockDatabaseScriptableObject : ScriptableObject {
    [SerializeField]
    public List<BlockData> blocks;
}
