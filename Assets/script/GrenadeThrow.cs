using UnityEngine;
using System.Collections;

public class GrenadeThrow : MonoBehaviour {

	public float range = 20f;
	public float damage = 70f;
	public float detonateDelay = 3f;
	public GameObject explosionEffectObject;

	private float timer = 0f;
	private Rigidbody rigidbody;
	private bool isDetonated = false;

	private GameObject explosionEffectObjectClone;
	private ParticleSystem explosionEffectParticle;
	private Light explosionEffectLight;
	private float destroy_timer = 0f;
	private bool isDestroy = false;



	// Use this for initialization
	void Start () {
		rigidbody = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () {
		timer += Time.deltaTime;
		if (timer >= detonateDelay && !isDestroy) {
			detonate ();
		}
		if (isDestroy) {
			destroy_timer += Time.deltaTime;
			if (destroy_timer >= 1f)
				destroy ();
		}
	}

	private void destroy(){
		Destroy (gameObject);
		Destroy (explosionEffectObjectClone);
		isDestroy = false;
		destroy_timer = 0f;
	}

	public void detonate() {
		isDetonated = true;
		isDestroy = true;
		Collider[] colliders = Physics.OverlapSphere (transform.position, range);

		Vector3 grenadePosition = transform.position;
		grenadePosition.y += 0.6f;

		explosionEffectObjectClone = (GameObject) Instantiate(
			explosionEffectObject, 
			grenadePosition, 
			transform.rotation
		);
		explosionEffectParticle = explosionEffectObjectClone.GetComponentInChildren <ParticleSystem> ();
		explosionEffectLight = explosionEffectObjectClone.GetComponentInChildren <Light> ();
		explosionEffectParticle.Play ();
		explosionEffectLight.intensity = 3f;

		foreach(Collider c in colliders){
			if (c.GetComponent<Rigidbody>() == null) continue;
			if (c.GetComponent<PlayerGameManager> () != null) {
				PlayerGameManager playerGameManager = c.GetComponent<PlayerGameManager> ();
				GameObject player = GameObject.FindGameObjectWithTag ("Player");
				playerGameManager.takeDamage(70.0f*((range-Vector3.Distance(player.transform.position,transform.position))/range));
			}
			if (c.GetComponent<GrenadeThrow> () != null && !c.GetComponent<GrenadeThrow> ().isDetonated) {
				c.GetComponent<GrenadeThrow> ().detonate ();
			}
			c.GetComponent<Rigidbody>().AddExplosionForce (damage, transform.position, range,0.0f,ForceMode.Impulse);
		}
	}

}
