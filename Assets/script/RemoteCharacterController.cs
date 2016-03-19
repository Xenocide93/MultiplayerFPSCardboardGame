using UnityEngine;
using System.Collections;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;


public class RemoteCharacterController : MonoBehaviour {

	public int characterType; // set this in editor and save as prefab parameter
	[HideInInspector]
	public int playerNum; //set this when instantiate in MultiplayerController

	private PlayerData remotePlayerData;

	//animation
	public float animSpeed = 1.5f;
	private Animator anim;

	//arm movement
	public Vector3 manualIdleRightArmOffset;
	public Vector3 manualIdleLeftArmOffset;
	public Vector3 manualAimRightArmOffset;
	public Vector3 manualAimLeftArmOffset;
	private Vector3 gunRightArmIdleOffset;
	private Transform rightArm, leftArm, gunEnd;

	//gun
	public GameObject bulletHole;
	public int MaxBulletHole;
	private ArrayList bulletHoleArray;
	private GunProperties gunProp;
	private AudioSource[] gunSound;

	//grenade
	public GameObject grenadePrefab;

	//head movement
	private Transform characterHead;
	private Transform aimDirection;

	void Awake() {
		ConsoleLog.SLog("Remote Awake() called");
	}

	void Start () {
		ConsoleLog.SLog("Remote Start() called");
		CheckDuplicateRemoteGameObject ();

		try {
			anim = GetComponent<Animator> ();

			Transform spine2 = transform.GetChild (2).GetChild (0).GetChild (2).GetChild (0).GetChild (0);

			//find head and aimDirection
			characterHead = spine2.GetChild (1).GetChild (0);
			for (int i = 0; i < characterHead.childCount; i++) {
				if (characterHead.GetChild (i).tag == "CameraPos") {
					aimDirection = characterHead.GetChild (i);
				}
			}

			if (aimDirection == null) ConsoleLog.SLog ("Error: Couldn't find aimDirection");

			//find left and right arm
			leftArm = spine2.GetChild (0).GetChild (0);
			rightArm = spine2.GetChild (2).GetChild (0);


			//find gun and gunEnd
			Transform rightHand = rightArm.GetChild(0).GetChild(0);
			Transform gun = null;
			for (int i = 0; i < rightHand.childCount; i++) {
				if (rightHand.GetChild (i).tag == "MyGun") {
					gun = rightHand.GetChild (i);
					gunProp = gun.GetComponent<GunProperties> ();
					gunSound = gun.GetComponents<AudioSource> ();
					for (int j = 0; j < gun.childCount; j++) {
						if (gun.GetChild (j).tag == "GunEnd") {
							gunEnd = gun.GetChild (j);
						}
					}
					if (gunEnd == null) ConsoleLog.SLog ("Error: Couldn't find gunEnd");
				}
			}
			if (gun == null) ConsoleLog.SLog ("Error: Couldn't find gun");

			bulletHoleArray = new ArrayList (MaxBulletHole);
		} catch (System.Exception e) {
			ConsoleLog.SLog("Error in start() of RemoteCharacterController\n" + e.Message);
		}
	}

	void Update () {
		//update remote character: body position, body rotation, head rotation, looking direction, animation state

		//if nothing new, skip update
		if(!isNeedUpdate()) return;
//		ConsoleLog.SLog ("[" + playerNum + "] Update () " + (int)Time.realtimeSinceStartup);

		remotePlayerData = MultiplayerController.instance.latestPlayerDatas[playerNum];

		//if remote player change weapon (change character)
		if(remotePlayerData.characterType != this.characterType){
			InstantiateNewCharacter (); //the rest of this function will be ignored (destroy)
		}

		//body position
		Vector3 velocity = Vector3.zero;
		transform.position = Vector3.SmoothDamp (
			transform.position,
			remotePlayerData.position,
			ref velocity,
			MultiplayerController.instance.timeBetweenBroadcast * 0.9f
		);

		//body rotation
		float degreePerSec = 0f;
		transform.rotation = Quaternion.Euler (
			0,
			Mathf.SmoothDampAngle (
				transform.rotation.eulerAngles.y,
				remotePlayerData.rotation.eulerAngles.y,
				ref degreePerSec,
				MultiplayerController.instance.timeBetweenBroadcast * 0.9f
			),
			0
		);

		//animation state
		if (anim.GetInteger ("AnimNum") != remotePlayerData.animState) {
//			ConsoleLog.SLog ("AnimNum change: change animation (" +
//				anim.GetInteger ("AnimNum") + ", " + remotePlayerData.animState
//			);

			anim.SetInteger ("AnimNum", remotePlayerData.animState);
			anim.SetTrigger ("NewAnimation");
		} else {
//			ConsoleLog.SLog ("AnimNum the same: ignore");
		}

		MultiplayerController.instance.hasNewPlayerDatas [playerNum] = false;
		MultiplayerController.instance.updatedLastFrame [playerNum] = true;
	}

	void LateUpdate(){
		//always do LateUpdate() to keep the head sync
		//otherwise head rotation will be overwritten by animator

		//head rotation
		characterHead.localRotation = Quaternion.Euler (
			0,
			remotePlayerData.rotation.eulerAngles.z,
			-remotePlayerData.rotation.eulerAngles.x
		);

		//aim to gun where head forward
		UpdateArm ();
	}

	private void UpdateArm (){
//		ConsoleLog.SLog ("[" + playerNum + "] UpdateArm()" + (int)Time.realtimeSinceStartup);

		try {
			RaycastHit hit;
			bool isHit = Physics.Raycast (aimDirection.position, aimDirection.forward, out hit, gunProp.gunRange);

			Vector3 hitPoint;
			if(isHit) {
				hitPoint = hit.point;
			} else {
				hitPoint = aimDirection.transform.position + aimDirection.transform.forward * gunProp.gunRange;
			}

			//recalculate offset
			gunRightArmIdleOffset = - gunEnd.transform.rotation.eulerAngles + rightArm.transform.rotation.eulerAngles;
			Vector3 gunToGazePosDiff = hitPoint - gunEnd.transform.position;

			//apply offset to arm
			Quaternion rightArmRotation = Quaternion.LookRotation (gunToGazePosDiff) * Quaternion.Euler (gunRightArmIdleOffset);
			rightArm.transform.rotation = rightArmRotation;

			//deal with left arm
			if (remotePlayerData.isAim) { //aim
				leftArm.transform.rotation = 
					Quaternion.LookRotation (gunToGazePosDiff) *
					Quaternion.Euler (gunRightArmIdleOffset) *
					Quaternion.Euler (manualAimLeftArmOffset);
			} else {
				if (gunProp.isTwoHanded) { //idle, two handed
					leftArm.transform.rotation = 
						Quaternion.LookRotation (gunToGazePosDiff) *
						Quaternion.Euler (gunRightArmIdleOffset) *
						Quaternion.Euler (manualIdleLeftArmOffset);
				} else { //idle, one handed
					
				}
			}

//			if (remotePlayerData.isAim) {
//
//			} else {
//
//			}
		} catch (System.Exception e) {
			ConsoleLog.SLog ("Error in Remote UpdateArm() [" + playerNum + "]\n" + e.Message);
		}
	}

	private bool isNeedUpdate(){
		if (!MultiplayerController.instance.hasNewPlayerDatas [playerNum]) {
//			ConsoleLog.SLog("[" + playerNum + "] 1111 nothing new");
			return false;
		}
		if (!PlayGamesPlatform.Instance.RealTime.IsRoomConnected ()) {
//			ConsoleLog.SLog("[" + playerNum + "] 2222 not connected");
			return false;
		}
		if(MultiplayerController.instance.localPlayerNumber == -1) {
//			ConsoleLog.SLog("[" + playerNum + "] 3333 haven't assign playerNum");
			return false;
		}
		return true;
	}

	private void InstantiateNewCharacter(){
		//instantiate the correct character (weapon)
		ConsoleLog.SLog("[" + playerNum + "] +++++ InstantiateNewCharacter() +++++");

		MultiplayerController.instance.characterGameObjects [playerNum] = Instantiate(
			MultiplayerController.instance.GetCharPrefab(remotePlayerData.characterType),
			remotePlayerData.position,
			Quaternion.Euler(0, remotePlayerData.rotation.eulerAngles.y, 0)
		) as GameObject;

		MultiplayerController.instance
			.characterGameObjects [playerNum]
			.GetComponent<RemoteCharacterController> ()
			.playerNum = this.playerNum;

		//destroy the old one
		Destroy (gameObject);
	}

	private void CheckDuplicateRemoteGameObject(){
		if (MultiplayerController.instance.characterGameObjects [playerNum] != gameObject) {
			ConsoleLog.SLog("Duplicate Remote GameObject : new instant removed");
			Destroy (gameObject);
		}
	}

	//called by local PlayerGameManager with remote character is shot
	public void TakeGunDamage (float damage){
		MultiplayerController.instance.SendDamage (playerNum, damage);

		//TODO play being shot animation
	}

	//called from MultiplayerController when the original character of this remote fire
	public void FireGun (Ray fireRay) {
		AudioSource.PlayClipAtPoint (gunSound [0].clip, fireRay.origin);

		try {
			RaycastHit hit;

			if (Physics.Raycast (fireRay, out hit, gunProp.gunRange)) {
				Debug.DrawRay (fireRay.origin, fireRay.direction, Color.yellow, 10f);

				if(hit.transform.GetComponent<RemoteCharacterController>() != null) return;

				//hit moveable object
				if (hit.rigidbody != null) {
					hit.rigidbody.AddForceAtPosition (
						fireRay.direction * gunProp.firePower, 
						hit.point, 
						ForceMode.Impulse
					);
				}

				if (hit.transform.GetComponent<Hit> () != null) {
					hit.transform.GetComponent<Hit> ().Hited ();
				}

				if (hit.transform.GetComponent<MilitaryBarrel> () != null) {
					MilitaryBarrel barrelScript = hit.transform.GetComponent<MilitaryBarrel> ();

					//cannot let bullet from remote player destroy item barrel, otherwise item inside might not be the same
					//let ItemIdGenerator destroy it from network and sync the item type inside
					if(barrelScript.hitCount >= 3) barrelScript.hitCount = 3;
					barrelScript.Hited ();
				}

				if (hit.transform.GetComponent<OilBarrel> () != null) {
					hit.transform.GetComponent<OilBarrel> ().Hited ();
				}

				if (hit.transform.GetComponent<SlimeBarrel> () != null) {
					SlimeBarrel barrelScript = hit.transform.GetComponent<SlimeBarrel> ();

					//cannot let bullet from remote player destroy item barrel, otherwise item inside might not be the same
					//let ItemIdGenerator destroy it from network and sync the item type inside
					if(barrelScript.hitCount >= 3) barrelScript.hitCount = 3;
					barrelScript.Hited ();
				}

				if (hit.transform.GetComponent<ItemId> () != null) {
					ConsoleLog.SLog ("Remote Hit ItemId: " + hit.transform.GetComponent<ItemId> ().id);
				}

				//bullet hole effect
				if (bulletHoleArray.Count >= MaxBulletHole) {
					Destroy ((GameObject)bulletHoleArray [0]);
					bulletHoleArray.RemoveAt (0);
				}

				GameObject tempBulletHole = (GameObject)Instantiate (bulletHole, hit.point, Quaternion.identity);
				tempBulletHole.transform.rotation = Quaternion.FromToRotation (tempBulletHole.transform.forward, hit.normal) * tempBulletHole.transform.rotation;
				bulletHoleArray.Add (tempBulletHole);
				tempBulletHole.transform.parent = hit.transform;
			}
		} catch (System.Exception e) {
			ConsoleLog.SLog ("Error FireGun in remote character " + playerNum);
			ConsoleLog.SLog (e.Message);
		}
	}

	//called from MultiplayerController when the original character of this remote thorw hand grenade
	public void ThrowGrenade(Vector3 position, Vector3 rotation, Vector3 force){
		GameObject grenadeClone = (GameObject) Instantiate(
			grenadePrefab, 
			position, 
			Quaternion.Euler(rotation)
		);
		grenadeClone.GetComponent<Rigidbody> ().AddForce (
			force, 
			ForceMode.Impulse
		);
	}
}
