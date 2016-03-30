using UnityEngine;
using System.Collections;

public class crosshairResizer : MonoBehaviour {

	private float size;
	private float multiplier = 1f;
	private Transform[] crosshairs = new Transform[4];
	private Transform parent;
	private GunProperties gunprop;
	private PlayerGameManager playerGameManager;
	private CardboardHead cardboardHead;
	private Transform headPos;

	// Use this for initialization
	void Start () {
		parent = transform.parent;
		crosshairs[0] = transform.GetChild (0);
		crosshairs[1] = transform.GetChild (1);
		crosshairs[2] = transform.GetChild (2);
		crosshairs[3] = transform.GetChild (3);

		FindComponents ();
	}

	private void CheckNullComponents(){
		if (
			gunprop == null ||
			playerGameManager == null ||
			cardboardHead == null ||
			headPos
		){
			FindComponents();
		}
	}

	private void FindComponents() {
		gunprop = GameObject.FindGameObjectWithTag ("MyGun").GetComponent<GunProperties>();
		playerGameManager = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerGameManager> ();
		cardboardHead = GameObject.FindGameObjectWithTag("PlayerHead").GetComponent<CardboardHead>();
		headPos = GameObject.FindGameObjectWithTag ("CameraPos").transform;
	}

	void Update () {
		CheckNullComponents ();

		if (gunprop.gunType == 4) {
			size = 0.01f;
		} else {
			float distanceOffset = 0f;
			if (cardboardHead.isAimHit) {
				distanceOffset = Vector3.Distance (cardboardHead.shootHit.point, headPos.position);
				distanceOffset = distanceOffset/100f;
			}
			if (playerGameManager.isInAimMode && playerGameManager.isWalking) {
				size = gunprop.walkingAimAccuracy + distanceOffset;
			} else if (playerGameManager.isInAimMode && !playerGameManager.isWalking) {
				size = gunprop.aimAccuracy + distanceOffset;
			} else if (!playerGameManager.isInAimMode && playerGameManager.isWalking) {
				size = gunprop.walkingAccuracy;
			} else {
				size = gunprop.accuracy;
			}
		}
		expandCrosshair (size);
	}

	public void expandCrosshair(float size){
		for (int i = 0; i < 4; i++) {
			crosshairs [i].position = 
				parent.position - crosshairs [i].up * size * multiplier * parent.parent.localScale.x;
		}
	}
}
