//
// Mecanimのアニメーションデータが、原点で移動しない場合の Rigidbody付きコントローラ
// サンプル
// 2014/03/13 N.Kobyasahi
//
using UnityEngine;
using System.Collections;

// 必要なコンポーネントの列記
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

	//arm movement
	private GameObject rightArm, leftArm, GunEnd;
	public Vector3 manualIdleRightArmOffset;
	public Vector3 manualIdleLeftArmOffset;
	public Vector3 manualAimRightArmOffset;
	public Vector3 manualAimLeftArmOffset;
	private PlayerGameManager playerGameManager;

	//animation
	private Animator anim;
	private AnimatorStateInfo currentBaseState;

	static int idleState = Animator.StringToHash("Base Layer.pistol idle normal");
	static int locoState = Animator.StringToHash("Base Layer.Locomotion");
	static int jumpState = Animator.StringToHash("Base Layer.Jump");
	static int restState = Animator.StringToHash("Base Layer.Rest");
	static int idleAimState = Animator.StringToHash("Base Layer.pistol idle aim");

// 初期化
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

		playerGameManager = GameObject.FindGameObjectWithTag ("Player").GetComponent<PlayerGameManager> ();
		rightArm = GameObject.FindGameObjectWithTag ("RightArm");
		leftArm = GameObject.FindGameObjectWithTag ("LeftArm");
		GunEnd = GameObject.FindGameObjectWithTag ("GunEnd");

	}

	void FixedUpdate ()
	{
		float h = Input.GetAxis("Horizontal");				// 入力デバイスの水平軸をhで定義
		float v = Input.GetAxis("Vertical");				// 入力デバイスの垂直軸をvで定義	
		anim.SetFloat("Speed", v);							// Animator側で設定している"Speed"パラメタにvを渡す
		anim.SetFloat("Direction", h); 						// Animator側で設定している"Direction"パラメタにhを渡す
		anim.speed = animSpeed;								// Animatorのモーション再生速度に animSpeedを設定する
		currentBaseState = anim.GetCurrentAnimatorStateInfo(0);	// 参照用のステート変数にBase Layer (0)の現在のステートを設定する


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
		
		// 以下、キャラクターの移動処理
		velocity = new Vector3(0, 0, v);		// 上下のキー入力からZ軸方向の移動量を取得
		// キャラクターのローカル空間での方向に変換
		velocity = transform.TransformDirection(velocity);
		//以下のvの閾値は、Mecanim側のトランジションと一緒に調整する
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

		// 上下のキー入力でキャラクターを移動させる
		transform.localPosition += velocity * Time.fixedDeltaTime;
		transform.localPosition += sideVelocity * Time.fixedDeltaTime;

		//move camera with player
		cardboardMain.transform.position = transform.position + cameraOffset;

		// 以下、Animatorの各ステート中での処理
		// Locomotion中
		// 現在のベースレイヤーがlocoStateの時
		if (currentBaseState.fullPathHash == locoState){
			//カーブでコライダ調整をしている時は、念のためにリセットする
			if(useCurves){
				resetCollider();
			}
		}
		// JUMP中の処理
		// 現在のベースレイヤーがjumpStateの時
		else if(currentBaseState.fullPathHash == jumpState)
		{
			// ステートがトランジション中でない場合
			if(!anim.IsInTransition(0))
			{
				
				// 以下、カーブ調整をする場合の処理
				if(useCurves){
					// 以下JUMP00アニメーションについているカーブJumpHeightとGravityControl
					// JumpHeight:JUMP00でのジャンプの高さ（0〜1）
					// GravityControl:1⇒ジャンプ中（重力無効）、0⇒重力有効
					float jumpHeight = anim.GetFloat("JumpHeight");
					float gravityControl = anim.GetFloat("GravityControl"); 
					if(gravityControl > 0)
						rb.useGravity = false;	//ジャンプ中の重力の影響を切る
										
					// レイキャストをキャラクターのセンターから落とす
					Ray ray = new Ray(transform.position + Vector3.up, -Vector3.up);
					RaycastHit hitInfo = new RaycastHit();
					// 高さが useCurvesHeight 以上ある時のみ、コライダーの高さと中心をJUMP00アニメーションについているカーブで調整する
					if (Physics.Raycast(ray, out hitInfo))
					{
						if (hitInfo.distance > useCurvesHeight)
						{
							col.height = orgColHight - jumpHeight;			// 調整されたコライダーの高さ
							float adjCenterY = orgVectColCenter.y + jumpHeight;
							col.center = new Vector3(0, adjCenterY, 0);	// 調整されたコライダーのセンター
						}
						else{
							// 閾値よりも低い時には初期値に戻す（念のため）					
							resetCollider();
						}
					}
				}
			}
		}
		// IDLE中の処理
		// 現在のベースレイヤーがidleStateの時
		else if (currentBaseState.fullPathHash == idleState)
		{
			//カーブでコライダ調整をしている時は、念のためにリセットする
			if(useCurves){
				resetCollider();
			}
		}
	}

	void LateUpdate(){
		//rotate arm to make to aim gun properly
		if (playerGameManager.isInAimMode) {
			Quaternion rotateLeftArm = Quaternion.LookRotation ((cardboardCamera.transform.forward), cardboardCamera.transform.up);
			Quaternion rotateRightArm = Quaternion.LookRotation ((cardboardCamera.transform.forward), cardboardCamera.transform.up);
			Vector3 tempRotateL = rotateLeftArm.eulerAngles;
			tempRotateL.z = 0;
			tempRotateL.y = 0;
			Vector3 tempRotateR = rotateRightArm.eulerAngles;
			tempRotateR.z = 0;
			tempRotateR.y = 0;
			leftArm.transform.rotation = Quaternion.Euler(tempRotateL) * Quaternion.Euler(manualAimLeftArmOffset);
			rightArm.transform.rotation = Quaternion.Euler(tempRotateR) * Quaternion.Euler(manualAimRightArmOffset);

		} else {
			//for pistol, only rotate right arm
			Quaternion rotateArm = Quaternion.LookRotation ((cardboardCamera.transform.forward), cardboardCamera.transform.up);
			Vector3 tempRotate = rotateArm.eulerAngles;
//			tempRotate.z = 0;
			tempRotate.y = 0;
			Vector3 rotateArmVector = tempRotate + manualIdleRightArmOffset;
			rightArm.transform.Rotate (rotateArmVector, Space.World);
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


	// キャラクターのコライダーサイズのリセット関数
	void resetCollider()
	{
	// コンポーネントのHeight、Centerの初期値を戻す
		col.height = orgColHight;
		col.center = orgVectColCenter;
	}
}
