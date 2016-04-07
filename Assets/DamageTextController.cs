using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DamageTextController : MonoBehaviour {

	public static float sDamage;

	public float maxScale, minScale, enlargeTime, shrinkTime;

	private Vector3 enlargeScale, shrinkScale;
	private TextMesh textMesh;
	private GameObject cardboardHead;
	private float timer = 0f;

	// Use this for initialization
	void Start () {
		cardboardHead = GameObject.FindGameObjectWithTag ("PlayerHead");
		textMesh = GetComponent<TextMesh> ();

		enlargeScale = new Vector3 (-maxScale, maxScale, maxScale);
		shrinkScale = new Vector3 (-minScale, minScale, minScale);

		textMesh.text = ((int) sDamage) + "";
	}

	void FixedUpdate () {
		Animate ();
		TurnTowardPlayer ();
	}

	private void TurnTowardPlayer(){
		transform.rotation = 
			Quaternion.LookRotation (cardboardHead.transform.position - transform.position);
	}

	private void Animate(){
		timer += Time.deltaTime;

		if (timer < enlargeTime) {

			float fragTime = timer / enlargeTime;
			textMesh.transform.localScale = Vector3.Lerp (new Vector3 (-0.1f, 0.1f, 0.1f), enlargeScale, fragTime);

		} else if (timer >= enlargeTime && timer < enlargeTime + shrinkTime) {

			float fragTime = (timer - enlargeTime) / shrinkTime;
			textMesh.transform.localScale = Vector3.Lerp (textMesh.transform.localScale, shrinkScale, fragTime);

		} else if (timer >= enlargeTime + shrinkTime) {

			Destroy (gameObject);

		}
	}
}
