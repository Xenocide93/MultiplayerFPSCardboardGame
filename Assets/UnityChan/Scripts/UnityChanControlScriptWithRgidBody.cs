//
// Mecanimのアニメーションデータが、原点で移動しない場合の Rigidbody付きコントローラ
// サンプル
// 2014/03/13 N.Kobyasahi
//
using UnityEngine;
using System.Collections;

[RequireComponent(typeof (Animator))]
[RequireComponent(typeof (CapsuleCollider))]
[RequireComponent(typeof (Rigidbody))]

public class UnityChanControlScriptWithRgidBody : MonoBehaviour
{	
	
	public float animSpeed = 1.5f;
	public float lookSmoother = 3.0f;
	public bool useCurves = true;
	public float useCurvesHeight = 0.5f;
	public float jumpPower = 3.0f; 

	private float forwardSpeed;
	private float backwardSpeed;
	private float sideSpeed; 

	private CapsuleCollider col;
	private Rigidbody rb;
	private Vector3 velocity;
	private Vector3 sideVelocity;

	private float orgColHight;
	private Vector3 orgVectColCenter;

	//camera
	private GameObject cardboardMain;
	private GameObject cardboardCamera;
	private Vector3 cameraOffset;
	private GameObject gazePointer;

	//arm movement
	private GameObject rightArm, leftArm, gunEnd;
	public Vector3 manualIdleRightArmOffset;
	public Vector3 manualIdleLeftArmOffset;
	public Vector3 manualAimRightArmOffset;
	public Vector3 manualAimLeftArmOffset;
	private PlayerGameManager playerGameManager;

	private Vector3 gunRightArmIdleOffset;

	//animation
	private Animator anim;
	private AnimatorStateInfo currentBaseState;

	static int idleState = Animator.StringToHash("Base Layer.pistol idle normal");
	static int locoState = Animator.StringToHash("Base Layer.Locomotion");
	static int jumpState = Animator.StringToHash("Base Layer.Jump");
	static int restState = Animator.StringToHash("Base Layer.Rest");
	static int idleAimState = Animator.StringToHash("Base Layer.pistol idle aim");

	void Start ()
	{
		anim = GetComponent<Animator>();
		col = GetComponent<CapsuleCollider>();
		rb = GetComponent<Rigidbody>();
		orgColHight = col.height;
		orgVectColCenter = col.center;

		cardboardMain = GameObject.FindGameObjectWithTag ("CardboardMain");
		cardboardCamera = GameObject.FindGameObjectWithTag("PlayerHead");
		cameraOffset = cardboardMain.transform.position - transform.position;
		gazePointer = GameObject.FindGameObjectWithTag ("GazePointer");

		playerGameManager = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerGameManager> ();
		rightArm = GameObject.FindGameObjectWithTag ("RightArm");
		leftArm = GameObject.FindGameObjectWithTag ("LeftArm");
		gunEnd = GameObject.FindGameObjectWithTag ("GunEnd");

		gunRightArmIdleOffset = - gunEnd.transform.rotation.eulerAngles + rightArm.transform.rotation.eulerAngles;
	}

	void FixedUpdate ()
	{
		float h = Input.GetAxis("Horizontal");
		float v = Input.GetAxis("Vertical");
		anim.SetFloat("Speed", v);
		anim.SetFloat("Direction", h);
		anim.speed = animSpeed;
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0);

		//for gravity problem
		RaycastHit hit = new RaycastHit();
		if (Physics.Raycast (transform.position, -Vector3.up, out hit)) {
			if (hit.distance >= 0.1) {
				rb.useGravity = false;
				rb.AddForce(Physics.gravity * rb.mass * 20);
			}
		}

		if (anim.GetBool("Aim")) {
			forwardSpeed = 1f;
			backwardSpeed = 1f;
			sideSpeed = 1f;
		} else {
			forwardSpeed = 2f;
			backwardSpeed = 2f;
			sideSpeed = 2f;
		}

		velocity = new Vector3(0, 0, v);
		velocity = transform.TransformDirection(velocity);
		if (v > 0.1) {
			velocity *= forwardSpeed;
		} else if (v < -0.1) {
			velocity *= backwardSpeed;
		}

		sideVelocity = new Vector3 (h, 0, 0);
		sideVelocity = transform.TransformDirection (sideVelocity);
		sideVelocity *= sideSpeed;

		
		if (Input.GetButtonDown("Jump")) {
			if(!anim.IsInTransition(0)) {
				rb.AddForce(Vector3.up * jumpPower, ForceMode.VelocityChange);
				anim.SetTrigger("JumpTrigger");
			}
		}

		transform.localPosition += velocity * Time.fixedDeltaTime;
		transform.localPosition += sideVelocity * Time.fixedDeltaTime;

		//move camera with player
		cardboardMain.transform.position = transform.position + cameraOffset;

		// Locomotion
		if (currentBaseState.fullPathHash == locoState){
			if(useCurves){
				resetCollider();
			}
		}
		else if(currentBaseState.fullPathHash == jumpState)
		{
			if(!anim.IsInTransition(0))
			{
				if(useCurves){
					float jumpHeight = anim.GetFloat("JumpHeight");
					float gravityControl = anim.GetFloat("GravityControl"); 
					if(gravityControl > 0)
					rb.useGravity = false;
										
					Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
					RaycastHit hitInfo = new RaycastHit();
					if (Physics.Raycast(ray, out hitInfo))
					{
						if (hitInfo.distance > useCurvesHeight)
						{
							col.height = orgColHight - jumpHeight;
							float adjCenterY = orgVectColCenter.y + jumpHeight;
							col.center = new Vector3(0, adjCenterY, 0);
						}
						else
						{
							resetCollider();
						}
					}
				}
			}
		}
		else if (currentBaseState.fullPathHash == idleState)
		{
			if(useCurves){
				resetCollider();
			}
		}
	}

	void LateUpdate(){
		

		//rotate arm to make to aim gun properly
		//for pistol, only rotate right arm
		gunRightArmIdleOffset = - gunEnd.transform.rotation.eulerAngles + rightArm.transform.rotation.eulerAngles;
		Vector3 gunToGazePosDiff = gazePointer.transform.position - gunEnd.transform.position;
		rightArm.transform.rotation = Quaternion.LookRotation (gunToGazePosDiff) * Quaternion.Euler (gunRightArmIdleOffset);

		if (playerGameManager.isInAimMode) {
			leftArm.transform.rotation = 
				Quaternion.LookRotation (gunToGazePosDiff) * 
				Quaternion.Euler (gunRightArmIdleOffset) * 
				Quaternion.Euler(manualAimLeftArmOffset);
		}
	}

	void OnGUI()
	{
//		GUI.Box(new Rect(Screen.width -260, 10 ,250 ,150), "Interaction");
//		GUI.Label(new Rect(Screen.width -245,30,250,30),"Up/Down Arrow : Go Forwald/Go Back");
//		GUI.Label(new Rect(Screen.width -245,50,250,30),"Left/Right Arrow : Turn Left/Turn Right");
//		GUI.Label(new Rect(Screen.width -245,70,250,30),"Hit Space key while Running : Jump");
//		GUI.Label(new Rect(Screen.width -245,90,250,30),"Hit Spase key while Stopping : Rest");
//		GUI.Label(new Rect(Screen.width -245,110,250,30),"Left Control : Front Camera");
//		GUI.Label(new Rect(Screen.width -245,130,250,30),"Alt : LookAt Camera");
	}

	void resetCollider()
	{
		col.height = orgColHight;
		col.center = orgVectColCenter;
	}
}
