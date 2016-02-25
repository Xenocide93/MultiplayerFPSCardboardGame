using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerGameManager : MonoBehaviour {

	//variable
	public int bulletLoadMax = 30;
	public int bulletStoreMax = 210;
	public int grenadeStore = 5;
	public float health = 100;
	public float reloadAlertRate = 3.0f;
	public Text debugText;

	//gun seeting
	GameObject gun;
	GunProperties gunProperties;
	GameObject gunLight;
	public float gunEffectTime = 0.05f;
	private float gunEffectTimer = 0.0f;
	private bool isShowGunEffect = false;

	public GameObject grenade;
	public float grenadeThrowForce = 10f;
	private GrenadeThrow grenadeProperties;
	private EllipsoidParticleEmitter gunFlashEmitter;

	//reload system
	private float reloadTimer = 0.0f;
	private float reloadAlertTimer = 0.0f;
	private bool isAlertReload = false;
	private bool isReloading = false;
	private bool isWalking = false;

	//fire system
	public GameObject bulletHole;
	public int bulletHoleMaxAmount;
	private ArrayList bulletHoleArray;
	private float fireTimer = 0.0f;
	private GameObject cardboardCamera;
	private CardboardHead cardboardHead;

	public bool forceAim;

	[Range(0f, 5f)]
	public float Accuracy;

	private Animator anim;
	private int bulletLoadCurrent = 30;
	private int bulletStoreCurrent = 210;
	[HideInInspector]
	public bool isInAimMode = false;
	private AudioSource footstepsAudio;

	//UI component
	private Transform healthBar;
	private TextMesh bulletText;
	private TextMesh reloadText;
	private TextMesh grenadeText;
	private GameObject HUD;

	//sound effect
	AudioSource[] gunAudio; 


	// Use this for initialization
	void Start () {
		//HUD
		HUD = GameObject.FindGameObjectWithTag("HUD");
		healthBar = HUD.transform.GetChild (0) as Transform;
		bulletText = HUD.transform.GetChild (1).GetComponent<TextMesh>();
		reloadText = HUD.transform.GetChild (2).GetComponent<TextMesh>();
		grenadeText = HUD.transform.GetChild (3).GetComponent<TextMesh>();

		anim = GetComponent<Animator> ();
		bulletText.text = bulletLoadCurrent + "/" + bulletStoreCurrent;
		cardboardCamera = GameObject.FindGameObjectWithTag("PlayerHead");
		cardboardHead = cardboardCamera.GetComponent<CardboardHead> ();
		gun = GameObject.FindGameObjectWithTag ("MyGun");
		gunProperties = gun.GetComponent<GunProperties> ();
		gunAudio = gun.GetComponents<AudioSource> ();
		gunLight = GameObject.FindGameObjectWithTag ("GunLight");
		gunFlashEmitter = GameObject.FindGameObjectWithTag ("GunFlash").GetComponent<EllipsoidParticleEmitter>();
		gunFlashEmitter.emit = false;
		grenadeText.text = grenadeStore + "";
		footstepsAudio = GetComponent<AudioSource> ();
		bulletHoleArray = new ArrayList (bulletHoleMaxAmount);
	}

	// Update is called once per frame
	void Update () {
		fireTimer += Time.deltaTime;

		if (isShowGunEffect) {showGunEffect (false);}
		if (isAlertReload) { alertReload ();}
		if (isReloading) {reloadWithDelay ();}

		detectInput ();
	}

	public void detectInput(){
		//for keyboard

		//walking
		if (Input.GetButton ("Horizontal") || Input.GetButton ("Vertical")) {
			if (!isWalking) {
				footstepsAudio.Play ();
				isWalking = true;
			}
		} else if (!Input.GetButton ("Horizontal") && !Input.GetButton ("Vertical")) {
			footstepsAudio.Stop();
			isWalking = false;
		}

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

		//reload
		if (Input.GetKeyDown (KeyCode.R) ||
			Input.GetKeyDown(KeyCode.JoystickButton2)) {

			isReloading = true;
			isAlertReload = false;
			reloadAlertTimer = 0f;
			reloadText.text = "RELOADING";
			AudioSource.PlayClipAtPoint (gunAudio [1].clip,gun.transform.position);
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
		if(forceAim) isInAimMode = true; // for debugging
		anim.SetBool ("Aim", isInAimMode);


		//throw grenade
		if(Input.GetKeyDown(KeyCode.G) || Input.GetKeyDown(KeyCode.JoystickButton3)){
			throwGrenade ();
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

	public void throwGrenade(){
		if (grenadeStore <= 0) { return;}

		grenadeStore--;
		grenadeText.text = grenadeStore + "";
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

	public void fireGun(){
		//TODO fire animation

		if (fireTimer < gunProperties.rateOfFire) { //has just fire, cooldown
			return;
		} if (isReloading) { //not finish reload, can't fire
			return;
		} if (bulletLoadCurrent <= 0) { //out of bullet, alert to reload
			isAlertReload = true;
		} else { //bullet left, fire!
			AudioSource.PlayClipAtPoint (gunAudio [0].clip, gun.transform.position);
			showGunEffect (true);
			gunFlashEmitter.Emit ();

			fireTimer = 0f;
			bulletLoadCurrent--;
			bulletText.text = bulletLoadCurrent + "/" + bulletStoreCurrent;


			if (cardboardHead.isAimHit) {
				//random shoot ray to simulate gun inaccuracy
				Vector3 direction = Random.insideUnitCircle * Accuracy; //Accuracy use for testing only
				direction.z = cardboardHead.shootHit.point.z;
				direction = cardboardHead.transform.TransformDirection (direction.normalized);

				Ray randomRay = new Ray (cardboardHead.transform.position, direction);
				RaycastHit hit;

				if (Physics.Raycast (randomRay, out hit, gunProperties.gunRange)) {
					//hit player
					//TODO reduce target's health

					//hit moveable object
					if (cardboardHead.shootHit.rigidbody != null) {
						cardboardHead.shootHit.rigidbody.AddForceAtPosition (
							cardboardCamera.transform.forward * gunProperties.firePower, 
							cardboardHead.shootHit.point, 
							ForceMode.Impulse
						);
					}

					//bullet hole effect
					if (bulletHoleArray.Count >= bulletHoleMaxAmount) {
						Destroy ((GameObject)bulletHoleArray [0]);
						bulletHoleArray.RemoveAt (0);
					}
					GameObject tempBulletHole = (GameObject)Instantiate (bulletHole, hit.point, Quaternion.identity);
					tempBulletHole.transform.rotation = Quaternion.FromToRotation (tempBulletHole.transform.forward, cardboardHead.shootHit.normal) * tempBulletHole.transform.rotation;
					bulletHoleArray.Add (tempBulletHole);
					tempBulletHole.transform.parent = hit.transform;
				}
			}

			if (bulletLoadCurrent == 0) { isAlertReload = true;}
		}
	}

	void showGunEffect(bool isTurnOn){
		if (isTurnOn) {
			isShowGunEffect = true;
			gunLight.GetComponent<Light> ().enabled = true;
		} else {
			isShowGunEffect = false;
			gunLight.GetComponent<Light> ().enabled = false;
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

	public void addStoreGrenade(int grenadeCount){
		grenadeStore += grenadeCount;
		grenadeText.text = grenadeStore + "";
	}

	public void addHealth(float heal){
		health += heal;
		if (health > 100f) { health = 100f;}

		Vector3 healthBarScale = healthBar.transform.localScale;
		healthBarScale = new Vector3(1f, health / 100f, 1f);
		healthBar.transform.localScale = healthBarScale;
	}
}