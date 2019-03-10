using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemData {

	public int itemID;
	public int stackSize;
    public string itemName;

    public ModelType modelType;
    public Sprite sprite; 
    public GameObject customModel;

    public bool placeable;
    public int placeableBlockID;

    //public bool burnable;
    public int burnTime;

    public bool craftable;

    public enum ModelType {
        Cube = 0,
        Sprite = 1,
        Custom = 2,
    }
    public enum EquipSlot {
        None = 0,
        Head = 1,
        Chest = 2,
        Legs = 3,
        Boots = 4,
        Gloves = 5,
        Jewelry = 6,
//        Offhand = 6,
    }

    public EquipSlot slot;

    //TODO usable/equipable/consumable

    //public GameObject modelInstance; //When on ground
	//public BlockType blockType; //TODO remove
	public int amount; //TODO remove

	//TODO recipe
}
