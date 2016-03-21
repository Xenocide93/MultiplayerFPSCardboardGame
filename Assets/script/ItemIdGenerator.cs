using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemIdGenerator : MonoBehaviour {
	public const int HEALTH_BOX = 1;
	public const int AMMO_BOX = 2;
	public const int GRENADE_BOX = 3;
	public const int MILITARY_BARREL = 4;
	public const int OIL_BARREL = 5;
	public const int SLIME_BARREL = 6;
	public const int ASSULT_RIFLE = 7;
	public const int SNIPER_RIFLE = 8;
	public const int SHOTGUN = 9;
	public const int CRATE = 10;

	public static ItemIdGenerator instance;

	private int currentId = 0;
	private Dictionary<int, GameObject> destroyableGameObjects;

	void Awake() {
		if (instance == null) {
			instance = this;
		} else if (instance != this) {
			Destroy (gameObject);
		}

		destroyableGameObjects = new Dictionary<int, GameObject> ();
	}

	public int GenerateId(GameObject item){
		int id = currentId;
		currentId++;
		destroyableGameObjects.Add (id, item);

		return id;
	}

	public static void CheckItemExist(int id) {
		try {
			if (ItemIdGenerator.instance.destroyableGameObjects [id] == null){
				ConsoleLog.SLog ("item " + id + " not exist");
			} else {
				ConsoleLog.SLog ("item " + id + " exist");
			}
		} catch (System.Exception e) {
			ConsoleLog.SLog ("Error in CheckItemExist(" + id + ")\n" + e.Message);
		}
	}

	public bool IsItemExist (int id) {
		if (destroyableGameObjects [id] == null) {
			return false;
		}
		return true;
	}

	public void RemoveByLocal (int id) {
		destroyableGameObjects.Remove (id);
	}


	public void DestroyItemByRemote (int id, System.Object something = null){
		//if item's gameobject has been destroyed already by remote's bullet, nothing will happen.
		if(!IsItemExist(id)) return;

		try {
			
			ItemIdGenerator.CheckItemExist(id);
			GameObject beingDestroyedItem = destroyableGameObjects [id];

			int type = destroyableGameObjects [id].GetComponent<ItemId>().type;

			switch (type) {
			case HEALTH_BOX:
			case AMMO_BOX:
			case GRENADE_BOX:
			case ASSULT_RIFLE:
			case SNIPER_RIFLE:
			case SHOTGUN:
				Destroy(beingDestroyedItem);
				break;

			case MILITARY_BARREL:
				int spawnItemType = (int) something;
				beingDestroyedItem.transform.GetChild(0).GetComponent<MilitaryBarrel>().DestroyIt(spawnItemType);
				break;
			case OIL_BARREL:
				if(!beingDestroyedItem.transform.GetChild(0).GetComponent<OilBarrel>().isDetonated)
				beingDestroyedItem.transform.GetChild(0).GetComponent<OilBarrel>().Detonate();
				break;
			case SLIME_BARREL:
				int spawnGunType = (int) something;
				beingDestroyedItem.transform.GetChild(0).GetComponent<SlimeBarrel>().DestroyIt(spawnGunType);
				break;

			case CRATE:
				beingDestroyedItem.GetComponent<Hit>().DestroyIt();
				break;

			default:
				ConsoleLog.SLog ("Error: Destroyable item with type default");
				break;
			}

			destroyableGameObjects.Remove (id);

		} catch (System.Exception e) {
			ConsoleLog.SLog ("Error in destroyItem()\n" + e.Message);
		}
	}
}
