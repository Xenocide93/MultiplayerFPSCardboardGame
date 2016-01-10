using UnityEngine;
using System.Collections;

public class PlayerGameManager : MonoBehaviour {

	//variable
	public int bulletLoadMax = 30;
	public int bulletStoreMax = 210;
	public float health = 100;

	private int bulletLoadCurrent = 30;
	private int bulletStoreCurrent = 210;

	//UI component
	public Transform healthBar;
	public TextMesh bulletText;


	// Use this for initialization
	void Start () {
		bulletText.text = bulletLoadCurrent + "/" + bulletStoreCurrent;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Fire1")) {
			fireGun ();
		}
		if (Input.GetKeyDown (KeyCode.R)) {
			reloadGun ();
		}
	}

	public void fireGun(){
		if (bulletLoadCurrent <= 0) {
			//out of bullet, reload
			//TODO alert to reload
		} else {
			//bullet left, fire!
			bulletLoadCurrent--;
			bulletText.text = bulletLoadCurrent + "/" + bulletStoreCurrent;
			//TODO fire the gun;
		}
	}

	public void reloadGun() {
		if (bulletLoadCurrent == bulletLoadMax) {
			return;
		} else if (bulletStoreCurrent >= bulletLoadMax - bulletLoadCurrent) {
			//planty of bullet left
			bulletStoreCurrent -= (bulletLoadMax - bulletLoadCurrent);
			bulletLoadCurrent = bulletLoadMax;
			bulletText.text = bulletLoadCurrent + "/" + bulletStoreCurrent;

			//TODO reload animation

		} else if (bulletStoreCurrent > 0) {
			//some bullet left, but now full mag
			bulletLoadCurrent = bulletStoreCurrent;
			bulletStoreCurrent = 0;
			bulletText.text = bulletLoadCurrent + "/" + bulletStoreCurrent;

			//TODO reload animation

		} else {
			//no more bullet
			//TODO display alert
		}
	}
}
