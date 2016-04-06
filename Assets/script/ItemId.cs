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
		//ConsoleLog.SLog ("OnDestroy() item " + id);

		try {
			if (ItemIdGenerator.instance.IsItemExist (id)) {
				ItemIdGenerator.instance.RemoveByLocal (id);

				int smallPayload = -2;

				switch (type) {
				case MILITARY_BARREL:
					smallPayload = gameObject.transform.GetChild(0).GetComponent<MilitaryBarrel> ().randomItemType;
					break;
				case SLIME_BARREL:
					smallPayload = gameObject.transform.GetChild(0).GetComponent<SlimeBarrel> ().randomGunType;
					break;
				}

				if (smallPayload == -2) {
					//ConsoleLog.SLog ("item " + id + " no payload");
					MultiplayerController.instance.SendDestroyItem (id);
				} else {
					//ConsoleLog.SLog ("item " + id + " payload " + ((int)smallPayload));
					MultiplayerController.instance.SendDestroyItem (id, smallPayload);
				}
			}
		} catch (System.Exception e) {
			ConsoleLog.SLog ("Error in ItemId.OnDestroy()\n" + e.Message);
		}
	}
}
