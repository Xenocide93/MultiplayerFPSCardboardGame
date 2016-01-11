using UnityEngine;
using System.Collections;

public class PlayerGameManager : MonoBehaviour {

	//variable
	public int bulletLoadMax = 30;
	public int bulletStoreMax = 210;
	public float health = 100;
	public float reloadAlertRate = 3.0f;

	//gun seeting
	//TODO get these from gun's properties
	public bool isAutomaticGun = false;
	public float rateOfFire = 0.0f;
	public float reloadTime = 3.0f;
	public float gunRange = 100f;
	public float firePower = 200;

	//reload system
	private float reloadTimer = 0.0f;
	private float reloadAlertTimer = 0.0f;
	private bool isAlertReload = false;
	private bool isReloading = false;

	//fire system
	private float fireTimer = 0.0f;
	private Ray shootRay;
	private RaycastHit shootHit;
	private int shootableMask;
	private GameObject cardboardCamera;

	private Animator anim;
	private int bulletLoadCurrent = 30;
	private int bulletStoreCurrent = 210;
	private bool isInAimMode = false;

	//UI component
	public Transform healthBar;
	public TextMesh bulletText;
	public TextMesh reloadText;


	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		bulletText.text = bulletLoadCurrent + "/" + bulletStoreCurrent;
		shootableMask = LayerMask.GetMask ("Shootable");
		cardboardCamera = GameObject.FindGameObjectWithTag("PlayerHead");
	}
	
	// Update is called once per frame
	void Update () {
		fireTimer += Time.deltaTime;

		if (isAlertReload) { alertReload ();}
		if (isReloading) {reloadWithDelay ();}

		detectInput ();


	}

	public void detectInput(){
		//fire
		if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Period)) {
			fireGun ();
		}
		//relode
		if (Input.GetKeyDown (KeyCode.R)) {
			isReloading = true;
			isAlertReload = false;
			reloadAlertTimer = 0f;
			reloadText.text = "RELOADING";

			//TODO play reload animation
		}

		//aim mode
		if (Input.GetKey (KeyCode.Slash) && !isInAimMode) { isInAimMode = true;} 
		if (Input.GetKeyUp (KeyCode.Slash) && isInAimMode ) { isInAimMode = false;}
		anim.SetBool ("Aim", isInAimMode);

		//debug
		if (Input.GetKeyDown(KeyCode.O)){
			takeDamage (10);
		}
	}

	public void fireGun(){
		//TODO raycast to object
		//TODO fire animation
		//TODO check automatic fire

		if (fireTimer < rateOfFire) { return;}
		if (isReloading) {return;} //not finish reload, can't fire

		if (bulletLoadCurrent <= 0) {
			//out of bullet, alert to reload
			isAlertReload = true;
		} else {
			//bullet left, fire!
			fireTimer = 0f;
			bulletLoadCurrent--;
			bulletText.text = bulletLoadCurrent + "/" + bulletStoreCurrent;

			shootRay.origin = cardboardCamera.transform.position;
			shootRay.direction = cardboardCamera.transform.forward;

			if (Physics.Raycast (shootRay, out shootHit, gunRange, shootableMask)) {
				//hit player
				//TODO reduce target's health

				//hit moveable object
				shootHit.rigidbody.AddForceAtPosition(cardboardCamera.transform.forward * firePower, shootHit.point, ForceMode.Impulse);
			}

			if (bulletLoadCurrent == 0) { isAlertReload = true;}
		}
	}

	public void reloadWithDelay(){
		reloadTimer += Time.deltaTime;

		if (reloadTimer > reloadTime) {
			reloadTimer = 0.0f;
			isReloading = false;
			reloadText.text = "";
			reloadGun ();
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
			isAlertReload = false;

			//TODO reload animation

		} else if (bulletStoreCurrent > 0) {
			//some bullet left, but not full mag
			bulletLoadCurrent = bulletStoreCurrent;
			bulletStoreCurrent = 0;
			bulletText.text = bulletLoadCurrent + "/" + bulletStoreCurrent;
			isAlertReload = false;

			//TODO reload animation

		} else {
			//no more bullet
			//TODO display alert
		}
	}

	public void alertReload (){
		reloadAlertTimer += Time.deltaTime;
		if (reloadAlertTimer > reloadAlertRate/2) {
			reloadText.text = "RELOAD";
		}
		if (reloadAlertTimer > reloadAlertRate) {
			reloadAlertTimer = 0f;
			reloadText.text = "";
		}
	}
		
	public void takeDamage(int damage){
		health -= damage;
		if (health < 0f) { health = 0;}

		Vector3 healthBarScale = healthBar.transform.localScale;
		healthBarScale = new Vector3(1f, health / 100f, 1f);
		healthBar.transform.localScale = healthBarScale;
	}
}
