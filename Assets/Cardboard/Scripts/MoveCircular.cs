using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MoveCircular : MonoBehaviour {

	public float rotateY  = 5;
	public float forwardForce = 5;
	public Transform cam;
	public float MaxDistance = 100;
	public GameObject winText;
	public GameObject scoreTextObject;

	private TextMesh scoreText;


	private int score = 0;

	private Rigidbody rb;
	private Teleport teleportScript;

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		teleportScript = GetComponent<Teleport> ();
		scoreText = scoreTextObject.GetComponent<TextMesh> ();
		winText.SetActive (false);
	}

	void FixedUpdate() {

		rb.AddForce (transform.forward * forwardForce);
		transform.Rotate (0, rotateY, 0);

		if (Vector3.Distance (transform.position, cam.transform.position) > MaxDistance) {
			teleportScript.TeleportRandomly ();
		}
	}

	public void Score(){
		teleportScript.TeleportRandomly ();
		score++;
		scoreText.text = "Score: " + score;
		if (score >= 10) {
			winText.SetActive (true);
		}
	}
}
