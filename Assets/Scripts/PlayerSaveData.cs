using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class PlayerSaveData {

	[SerializeField]
	public Vector3 position;
	[SerializeField]
	public ItemData[] inventory;
	[SerializeField]
	public ItemData[] equipment;

	public PlayerSaveData(PlayerData player) {
		position = player.transform.position;
		inventory = player.GetComponent<PlayerInventory>().inventory.items;
		equipment = player.GetComponent<PlayerInventory>().equipment.items;
	}
}
