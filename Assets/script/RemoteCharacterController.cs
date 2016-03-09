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
	public AnimatorStateInfo currentBaseState;

	//arm movement
	private Transform rightArm, leftArm, gunEnd;
	public Vector3 manualIdleRightArmOffset;
	public Vector3 manualIdleLeftArmOffset;
	public Vector3 manualAimRightArmOffset;
	public Vector3 manualAimLeftArmOffset;
	private Vector3 gunRightArmIdleOffset;
	private GunProperties gunProp;

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
					for (int j = 0; j < gun.childCount; j++) {
						if (gun.GetChild (j).tag == "GunEnd") {
							gunEnd = gun.GetChild (j);
						}
					}
					if (gunEnd == null) ConsoleLog.SLog ("Error: Couldn't find gunEnd");
				}
			}
			if (gun == null) ConsoleLog.SLog ("Error: Couldn't find gun");
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
		transform.position = remotePlayerData.position;

		//body rotation
		transform.rotation = Quaternion.Euler(0, remotePlayerData.rotation.eulerAngles.y, 0);

		//animation state
		anim.SetInteger ("AnimNum", remotePlayerData.animState);

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

//			//deal with left arm
//			if (playerGameManager.isInAimMode) { //aim
//				leftArm.transform.rotation = 
//					Quaternion.LookRotation (gunToGazePosDiff) *
//					Quaternion.Euler (gunRightArmIdleOffset) *
//					Quaternion.Euler (manualAimLeftArmOffset);
//			} else {
//				if (gunProp.isTwoHanded) { //idle, two handed
//					leftArm.transform.rotation = 
//						Quaternion.LookRotation (gunToGazePosDiff) *
//						Quaternion.Euler (gunRightArmIdleOffset) *
//						Quaternion.Euler (manualIdleLeftArmOffset);
//				} else { //idle, one handed
//				}
//			}
//
//			if (playerGameManager.isInAimMode) {
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

	public void TakeGunDamage (float damage){
		MultiplayerController.instance.SendDamage (playerNum, damage);

		//TODO play being shot animation
	}

	//fire

	//reload

	//throw grenade
}
