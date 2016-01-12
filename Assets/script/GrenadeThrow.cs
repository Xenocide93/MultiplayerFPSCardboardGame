using UnityEngine;
using System.Collections;

public class GrenadeThrow : MonoBehaviour {

	public float range = 20f;
	public float damage = 70f;
	public float detonateDelay = 3f;

	private float timer = 0f;
	private Rigidbody rigidbody;

	// Use this for initialization
	void Start () {
		rigidbody = GetComponent<Rigidbody> ();
	}
	
	// Update is called once per frame
	void Update () {
		timer += Time.deltaTime;
		if (timer >= detonateDelay) {
			detonate ();
		}
	}

	void detonate() {
		Collider[] colliders = Physics.OverlapSphere (transform.position, range);
		foreach(Collider c in colliders){
			if (c.GetComponent<Rigidbody>() == null) continue;
			if (c.GetComponent<PlayerGameManager> () != null) {
				PlayerGameManager playerGameManager = c.GetComponent<PlayerGameManager> ();
				GameObject player = GameObject.FindGameObjectWithTag ("Player");
				playerGameManager.takeDamage(70.0f*((range-Vector3.Distance(player.transform.position,transform.position))/range));
			}
			c.GetComponent<Rigidbody>().AddExplosionForce (damage, transform.position, range,0.0f,ForceMode.Impulse);
		}
	}

}
