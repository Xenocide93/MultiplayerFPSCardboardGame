using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerGameManager : MonoBehaviour {

	//variable
	public int bulletLoadMax = 30;
	public int bulletStoreMax = 210;
	public float health = 100;
	public float reloadAlertRate = 3.0f;
	public Text debugText;

	//gun seeting
	GameObject gun;
	GunProperties gunProperties;

	public GameObject grenade;
	public float grenadeThrowForce = 10f;
	GrenadeThrow grenadeProperties;

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

	//sound effect
	AudioSource[] gunAudio; 


	// Use this for initialization
	void Start () {
		anim = GetComponent<Animator> ();
		bulletText.text = bulletLoadCurrent + "/" + bulletStoreCurrent;
		shootableMask = LayerMask.GetMask ("Shootable");
		cardboardCamera = GameObject.FindGameObjectWithTag("PlayerHead");
		gun = GameObject.FindGameObjectWithTag ("MyGun");
		gunProperties = gun.GetComponent<GunProperties> ();
		gunAudio = gun.GetComponents<AudioSource> ();

	}
	
	// Update is called once per frame
	void Update () {
		fireTimer += Time.deltaTime;

		if (isAlertReload) { alertReload ();}
		if (isReloading) {reloadWithDelay ();}

		detectInput ();


	}

	public void detectInput(){
		//for keyboard
		//fire
		if (gunProperties.isAutomatic) {
			if (Input.GetButton("Fire1") || 
				Input.GetKey(KeyCode.Period) || 
				Input.GetKey(KeyCode.JoystickButton7)) {
				fireGun ();
			}

		} else {
			if (Input.GetButtonDown("Fire1") || 
				Input.GetKeyDown(KeyCode.Period) || 
				Input.GetKey(KeyCode.JoystickButton7)) {
				fireGun ();
			}
		}

		//relode
		if (Input.GetKeyDown (KeyCode.R) ||
			Input.GetKeyDown(KeyCode.JoystickButton2)) {
			isReloading = true;
			isAlertReload = false;
			reloadAlertTimer = 0f;
			reloadText.text = "RELOADING";
			gunAudio [1].Play ();
			//TODO play reload animation
		}

		//aim mode
		if ((Input.GetKey (KeyCode.Slash) ||
			Input.GetKey(KeyCode.JoystickButton4) || 
			Input.GetKey(KeyCode.JoystickButton5)) && !isInAimMode) {
			isInAimMode = true;
		} 
		if ((Input.GetKeyUp (KeyCode.Slash) || 
			Input.GetKeyUp(KeyCode.JoystickButton4) ||
			Input.GetKeyUp(KeyCode.JoystickButton5) && isInAimMode )) {
			isInAimMode = false;
		}
		anim.SetBool ("Aim", isInAimMode);


		//throw grenade
		if(Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.JoystickButton3)){
			//throw grenade
			GameObject grenadeClone = (GameObject) Instantiate(
				grenade, 
				cardboardCamera.transform.position + cardboardCamera.transform.forward * 1f, 
				cardboardCamera.transform.rotation
			);
			grenadeClone.GetComponent<Rigidbody> ().AddForce (
				cardboardCamera.transform.forward * grenadeThrowForce, 
				ForceMode.Impulse
			);
		}

		//debug
		if (Input.GetKeyDown(KeyCode.O)){
			takeDamage (10);
		}

		//for controller
		if(Input.GetKeyDown(KeyCode.JoystickButton0)){ debugText.text = "button 0";} //A
		if(Input.GetKeyDown(KeyCode.JoystickButton1)){ debugText.text = "button 1";} //B
		if(Input.GetKeyDown(KeyCode.JoystickButton2)){ debugText.text = "button 2";} //X
		if(Input.GetKeyDown(KeyCode.JoystickButton3)){ debugText.text = "button 3";} //Y
		if(Input.GetKeyDown(KeyCode.JoystickButton4)){ debugText.text = "button 4";} //LB
		if(Input.GetKeyDown(KeyCode.JoystickButton5)){ debugText.text = "button 5";} //RB
		if(Input.GetKeyDown(KeyCode.JoystickButton6)){ debugText.text = "button 6";} //LT
		if(Input.GetKeyDown(KeyCode.JoystickButton7)){ debugText.text = "button 7";} //RT
		if(Input.GetKeyDown(KeyCode.JoystickButton8)){ debugText.text = "button 8";} //L analog click
		if(Input.GetKeyDown(KeyCode.JoystickButton9)){ debugText.text = "button 9";} //R analog click
		if(Input.GetKeyDown(KeyCode.JoystickButton10)){ debugText.text = "button 10";} //start
		if(Input.GetKeyDown(KeyCode.JoystickButton11)){ debugText.text = "button 11";}
		if(Input.GetKeyDown(KeyCode.JoystickButton12)){ debugText.text = "button 12";}
		if(Input.GetKeyDown(KeyCode.JoystickButton13)){ debugText.text = "button 13";}
		if(Input.GetKeyDown(KeyCode.JoystickButton14)){ debugText.text = "button 14";}
		if(Input.GetKeyDown(KeyCode.JoystickButton15)){ debugText.text = "button 15";}
		if(Input.GetKeyDown(KeyCode.JoystickButton16)){ debugText.text = "button 16";}
		if(Input.GetKeyDown(KeyCode.JoystickButton17)){ debugText.text = "button 17";}
		if(Input.GetKeyDown(KeyCode.JoystickButton18)){ debugText.text = "button 18";}
		if(Input.GetKeyDown(KeyCode.JoystickButton19)){ debugText.text = "button 19";}
	}

	public void fireGun(){
		//TODO raycast to object
		//TODO fire animation
		//TODO check automatic fire

		if (fireTimer < gunProperties.rateOfFire) { return;}
		if (isReloading) {return;} //not finish reload, can't fire

		if (bulletLoadCurrent <= 0) {
			//out of bullet, alert to reload
			isAlertReload = true;
		} else {
			//bullet left, fire!

			gunAudio[0].Play();

			fireTimer = 0f;
			bulletLoadCurrent--;
			bulletText.text = bulletLoadCurrent + "/" + bulletStoreCurrent;

			shootRay.origin = cardboardCamera.transform.position;
			shootRay.direction = cardboardCamera.transform.forward;

			if (Physics.Raycast (shootRay, out shootHit, gunProperties.gunRange, shootableMask)) {
				//hit player
				//TODO reduce target's health

				//hit moveable object
				shootHit.rigidbody.AddForceAtPosition(cardboardCamera.transform.forward * gunProperties.firePower, shootHit.point, ForceMode.Impulse);
			}

			if (bulletLoadCurrent == 0) { isAlertReload = true;}
		}
	}

	public void reloadWithDelay(){
		reloadTimer += Time.deltaTime;

		if (reloadTimer > gunProperties.reloadTime) {
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
		
	public void takeDamage(float damage){
		health -= damage;
		if (health < 0f) { health = 0;}

		Vector3 healthBarScale = healthBar.transform.localScale;
		healthBarScale = new Vector3(1f, health / 100f, 1f);
		healthBar.transform.localScale = healthBarScale;
	}

	public void addStoreBullet(int bulletCount){
		bulletStoreCurrent += bulletCount;
		bulletText.text = bulletLoadCurrent + "/" + bulletStoreCurrent;
	}
}
