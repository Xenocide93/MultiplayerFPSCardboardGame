using UnityEngine;
using System.Collections;

public class ItemId : MonoBehaviour {
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

	public int id;
	public int type = 0;

	// Use this for initialization
	void Start () {
		id = ItemIdGenerator.instance.GenerateId (gameObject);
	}

	void OnDestroy(){
		if (ItemIdGenerator.instance.IsItemExist (id)) {
			ItemIdGenerator.instance.RemoveByLocal (id);
			MultiplayerController.instance.SendDestroyItem (id);
		}
	}
}
