using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NameTagController : MonoBehaviour {

	public int gamePlayerNum = 0;
	public float yOffset = 0f;

	private Transform cardboardCamera;
	private GameObject canvas;
	private RectTransform canvasRectTransform;
	private Image avatar;
	private Text playerName;
	private GameObject deadText;

	private bool isSetAvatar = false;
	private bool isSetName = false;

	private bool isSafeToSetValue = false;

	// Use this for initialization
	void Start () {

		cardboardCamera = transform.parent;
		canvas = transform.GetChild (0).gameObject;
		canvasRectTransform = canvas.GetComponent<RectTransform> ();
		avatar = transform.GetChild (0).GetChild (0).GetChild (0).GetComponent<Image> ();
		playerName = transform.GetChild (0).GetChild (0).GetChild (1).GetComponent<Text> ();
		deadText = transform.GetChild (0).GetChild (0).GetChild (2).gameObject;

		deadText.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {

		if (!MultiplayerController.instance.isGameStart) {
			return;
		}

		if (!isSafeToSetValue) {
			CheckConditions ();
			return;
		}

		if (MultiplayerController.instance.localGamePlayerNumber == gamePlayerNum) {
			Destroy (gameObject);
		}

		SetPosition ();

		if (MultiplayerController.instance.latestPlayerDatas [gamePlayerNum].health <= 0f) {
			deadText.SetActive (true);
		} else {
			deadText.SetActive (false);
		}
	}

	private void CheckConditions(){
		if (MultiplayerController.instance == null) {
			canvas.SetActive (false);
			return;
		} else if (
			gamePlayerNum > MultiplayerController.instance.MaxOpponents ||
			MultiplayerController.instance.latestPlayerDatas == null ||
			MultiplayerController.instance.playersAvatar == null ||
			MultiplayerController.instance.playersName == null
		) {
			canvas.SetActive (false);
			return;
		} else if (
			MultiplayerController.instance.localGamePlayerNumber == gamePlayerNum ||
			MultiplayerController.instance.playersTeamNumber[MultiplayerController.instance.localGamePlayerNumber] !=
			MultiplayerController.instance.playersTeamNumber[gamePlayerNum] ||
			MultiplayerController.instance.gameMode == MultiplayerController.GAMEMODE_DEATHMATCH
		){
			Destroy (gameObject);
		} else if (
			MultiplayerController.instance.latestPlayerDatas.Length <= gamePlayerNum ||
			MultiplayerController.instance.playersAvatar.Length <= gamePlayerNum ||
			MultiplayerController.instance.playersName.Length <= gamePlayerNum
		) {
			canvas.SetActive (false);
			return;
		} else if (
			MultiplayerController.instance.latestPlayerDatas [gamePlayerNum] == null ||
			MultiplayerController.instance.playersAvatar [gamePlayerNum] == null ||
			MultiplayerController.instance.playersName [gamePlayerNum] == null
		) {
			canvas.SetActive (false);
			return;
		} else {
			isSafeToSetValue = true;
			canvas.SetActive (true);

			if (!isSetAvatar) {
				SetAvatar (MultiplayerController.instance.playersAvatar [gamePlayerNum]);
			}
			if (!isSetName) {
				SetName (MultiplayerController.instance.playersName [gamePlayerNum]);
			}
		}
	}

	private void SetPosition(){
		Vector3 direction = 
			MultiplayerController.instance.latestPlayerDatas [gamePlayerNum].position 
			+ new Vector3 (0f, yOffset, 0f)
			- transform.parent.position;
		
		direction = Vector3.Normalize (direction);

		canvasRectTransform.position = transform.parent.position + direction * 0.6f;
		canvasRectTransform.rotation = Quaternion.LookRotation (direction);
	}

	private void SetName (string name){
		isSetName = true;

		playerName.text = name;
	}

	private void SetAvatar (Texture2D texture){
		isSetAvatar = true;

		Sprite avatarSprite = Sprite.Create (
			MultiplayerController.instance.playersAvatar [gamePlayerNum],
			new Rect (
				0, 
				0, 
				MultiplayerController.instance.playersAvatar [gamePlayerNum].width,
				MultiplayerController.instance.playersAvatar [gamePlayerNum].height
			),
			new Vector2 (0f, 0f)
		);
		avatar.overrideSprite = avatarSprite;
	}
}
