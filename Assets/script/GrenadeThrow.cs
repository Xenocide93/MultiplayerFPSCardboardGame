using UnityEngine;
using System.Collections;

public class GrenadeThrow : MonoBehaviour {

	public float range = 4.5f;
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
	private float damage_timer = 0f;
	private bool isDestroy = false;
	private bool isDamage = false;
	private AudioSource detonateAudio; 


	// Use this for initialization
	void Start () {
		rigidbody = GetComponent<Rigidbody> ();
		detonateAudio = GetComponent<AudioSource> ();
	}
	
	// Update is called once per frame
	void Update () {
		timer += Time.deltaTime;
		if (timer >= detonateDelay && !isDestroy) {
			detonate ();
		}
		if (isDamage) {
			damage_timer += Time.deltaTime;
			if (damage_timer >= 0.21f) {
				AudioSource.PlayClipAtPoint (detonateAudio.clip,transform.position);
				takeDamage ();
			}
		}
		if (isDestroy) {
			explosionEffectLight.intensity -= 1;
			ParticleSystem.Particle []particleList = new ParticleSystem.Particle[explosionEffectParticle.particleCount];
			explosionEffectParticle.GetParticles(particleList);
			for(int i = 0; i < particleList.Length; ++i) {
				byte r = particleList [i].color.r;
				byte g = particleList [i].color.g;
				byte b = particleList [i].color.b;
				byte a = particleList [i].color.a;
				a--;a--;
				particleList[i].color = new Color32(r, g, b, a);
			}   
			explosionEffectParticle.SetParticles(particleList, explosionEffectParticle.particleCount);

			destroy_timer += Time.deltaTime;
			if (destroy_timer >= 1.6f)
				destroy ();
		}
	}

	private void takeDamage() {
		Collider[] colliders = Physics.OverlapSphere (transform.position, range);
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
		isDamage = false;
		damage_timer = 0f;
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
		isDamage = true;
		explosionEffectObjectClone = (GameObject) Instantiate(
			explosionEffectObject, 
			transform.position, 
			transform.rotation
		);
		explosionEffectParticle = explosionEffectObjectClone.GetComponentInChildren <ParticleSystem> ();
		explosionEffectLight = explosionEffectObjectClone.GetComponentInChildren <Light> ();
		explosionEffectParticle.Play ();
		explosionEffectLight.intensity = 3f;
	}

}
