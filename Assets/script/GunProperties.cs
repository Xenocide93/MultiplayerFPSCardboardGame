using UnityEngine;
using System.Collections;

public class GunProperties : MonoBehaviour {

	public bool isAutomatic = false;
	public bool isTwoHanded = false;
	public float rateOfFire = 0.5f;
	public float reloadTime = 3f;
	public float gunRange = 100f;
	public float firePower = 100f;
	public int zoomRange;

	// guntype
	// 1 == pistol
	// 2 == rifle
	// 3 == shotgun
	// 4 == sniperrifle
	public int gunType;
	[Range(0f, 0.3f)]
	public float accuracy;
	[Range(0f, 0.3f)]
	public float aimAccuracy;
	[Range(0f, 0.3f)]
	public float walkingAccuracy;
	[Range(0f, 0.3f)]
	public float walkingAimAccuracy;

	void OnTriggerEnter(Collider c) {
		//TODO: change character prefab
		if (c.GetComponent<PlayerGameManager> () != null)
		Destroy (gameObject);	
	}
}
