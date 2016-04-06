using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerStatHUDController : MonoBehaviour {

	public int gamePlayerNum = 0;

	private GameObject canvas;
	private Image avatar;
	private Text playerName;
	private Slider health;
	private GameObject deadText;

	private bool isSetAvatar = false;
	private bool isSetName = false;

	private bool isSafeToSetValue = false;

	// Use this for initialization
	void Start () {
		canvas = transform.GetChild (0).gameObject;
		avatar = transform.GetChild (0).GetChild (0).GetChild (0).GetComponent<Image> ();
		playerName = transform.GetChild (0).GetChild (0).GetChild (1).GetComponent<Text> ();
		health = transform.GetChild (0).GetChild (0).GetChild (2).GetComponent<Slider> ();
		deadText = transform.GetChild (0).GetChild (0).GetChild (3).gameObject;

		health.value = 100f;
		deadText.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		if (!isSafeToSetValue) {
			CheckConditions ();
			return;
		}
			
		health.value = MultiplayerController.instance.latestPlayerDatas [gamePlayerNum].health;

		if (MultiplayerController.instance.latestPlayerDatas [gamePlayerNum].health <= 0f) {
			deadText.SetActive (true);
			health.gameObject.SetActive (false);
		} else {
			deadText.SetActive (false);
			health.gameObject.SetActive (true);
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
