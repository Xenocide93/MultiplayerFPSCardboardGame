using UnityEngine;
using System.Collections;

public class GunProperties : MonoBehaviour {

	public bool isAutomatic = false;
	public bool isTwoHanded = false;
	public int bulletLoadMax;
	public int bulletStoreMax;
	[HideInInspector] public int bulletLoadCurrent;
	[HideInInspector] public int bulletStoreCurrent;
	public float rateOfFire = 0.5f;
	public float reloadTime = 3f;
	public float gunRange = 100f;
	public float firePower = 100f;
	public float zoomRange;

	// guntype
	// 1 == pistol
	// 2 == rifle
	// 3 == shotgun
	// 4 == sniperrifle
	public int gunType;
	public GameObject characterWithGunPrefab;

	[Range(0f, 0.3f)] public float accuracy;
	[Range(0f, 0.3f)] public float aimAccuracy;
	[Range(0f, 0.3f)] public float walkingAccuracy;
	[Range(0f, 0.3f)] public float walkingAimAccuracy;

	void Awake() {
		bulletLoadCurrent = bulletLoadMax;
		bulletStoreCurrent = bulletStoreMax;
	}

	void OnTriggerEnter(Collider c) {
		if (c.GetComponent<PlayerGameManager> () != null) {
			
			//retrive old character data
			Vector3 pos = c.transform.position;
			Quaternion rot = c.transform.rotation;
			PlayerGameManager playerGameManager = c.GetComponent<PlayerGameManager> ();
			float health = playerGameManager.health;
			int grenadeStore = playerGameManager.grenadeStore;

			//destroy old character
			playerGameManager = null;
			Destroy (c.gameObject);

			if (c.gameObject != null) {
				ConsoleLog.SLog ("Why the fuck are you still exist!!??");
			}
				
			//trigger cardboardHead to find new character component
			CardboardHead cardboardHead = GameObject.FindGameObjectWithTag("PlayerHead").GetComponent<CardboardHead>();
			cardboardHead.isCharacterSync = false;

			//instantiate new character
			GameObject newCharacter = Instantiate (characterWithGunPrefab, pos, rot) as GameObject;
			PlayerGameManager newCharGameManager = newCharacter.GetComponent<PlayerGameManager> ();

			//set retrived data to new character
			newCharGameManager.health = health;
			newCharGameManager.grenadeStore = grenadeStore;

			Destroy (gameObject);	
		}
	}
}
