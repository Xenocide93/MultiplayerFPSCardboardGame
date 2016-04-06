using UnityEngine;
using System.Collections;

public class OilBarrel : MonoBehaviour {

	public GameObject explosionEffectObject;
	public GameObject slime;
	public Mesh[] meshTypes;
	private int alternator;
	private MeshFilter closedBarrels;
	private int hitCount;
	private GameObject explosionEffectObjectClone;
	private ParticleSystem explosionEffectParticle;
	private Light explosionEffectLight;
	private float range = 4.5f;
	private float damage = 70f;
	private AudioSource detonateAudio; 
	private float damage_timer = 0f;
	private bool isDestroy = false;
	private bool isDamage = false;
	private Vector3 detonatePosition;
	[HideInInspector] public bool isDetonated = false;

	// Use this for initialization
	void Start () {
		hitCount = 0;
		alternator = 3;
		closedBarrels = GetComponent<MeshFilter>();
		detonateAudio = transform.parent.gameObject.GetComponent<AudioSource> ();
	}

	// Update is called once per frame
	void Update () {
		if (isDamage) {
			damage_timer += Time.deltaTime;
			if (damage_timer >= 0.21f) {
				AudioSource.PlayClipAtPoint (detonateAudio.clip,detonatePosition);
				TakeDamage ();
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
		}

	}

	public void Hited() {
		hitCount++;
		SetBending ();
	}

	void SetBending() {
		GetComponent<AudioSource>().Stop();
		GetComponent<AudioSource>().pitch = Random.Range(0.4f, 0.7f);
		GetComponent<AudioSource>().Play();
		if (hitCount == 5) {
			Detonate ();	
		} else if (hitCount == 1) {
			closedBarrels.mesh = meshTypes[1];
		} else if (hitCount == 4) {
			Quaternion target = Quaternion.Euler(0, 3f, 0);
			transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime*1f);
			SetSlime ();
		} else {
			Quaternion target = Quaternion.Euler(0, 3f, 0);
			transform.rotation = Quaternion.Slerp(transform.rotation, target, Time.deltaTime*1f);
		}
	}

	void SetSlime() {
		if(!slime.activeSelf) {
			slime.SetActive(true);
		} 
	}

	public void Detonate() {
		explosionEffectObjectClone = (GameObject) Instantiate(
			explosionEffectObject, 
			transform.position, 
			transform.rotation
		);
		explosionEffectParticle = explosionEffectObjectClone.GetComponentInChildren <ParticleSystem> ();
		explosionEffectLight = explosionEffectObjectClone.GetComponentInChildren <Light> ();
		explosionEffectParticle.Play ();
		explosionEffectLight.intensity = 3f;
		detonatePosition = transform.position;
		isDetonated = true;
		isDestroy = true;
		isDamage = true;
		GetComponent<Rigidbody> ().isKinematic = false;
	}

	private void TakeDamage() {
		Collider[] colliders = Physics.OverlapSphere (detonatePosition, range);
		foreach(Collider c in colliders){
			if (c.GetComponent<Rigidbody>() == null) continue;
			if (c.GetComponent<PlayerGameManager> () != null) {
				PlayerGameManager playerGameManager = c.GetComponent<PlayerGameManager> ();
				GameObject player = GameObject.FindGameObjectWithTag ("Player");
				playerGameManager.TakeDamage(70.0f*((range-Vector3.Distance(player.transform.position,detonatePosition))/range));
			}
			if (c.GetComponent<GrenadeThrow> () != null && !c.GetComponent<GrenadeThrow> ().isDetonated) {
				c.GetComponent<GrenadeThrow> ().detonate ();
			}
			if (c.GetComponent<Hit> () != null) {
				c.GetComponent<Hit> ().DestroyIt();
			}
			if (c.GetComponent<MilitaryBarrel> () != null) {
				c.GetComponent<MilitaryBarrel> ().DestroyIt ();
			}
			if (c.GetComponent<OilBarrel> () != null && !c.GetComponent<OilBarrel> ().isDetonated) {
				c.GetComponent<OilBarrel> ().Detonate ();
			}
			if (c.GetComponent<SlimeBarrel> () != null) {
				c.GetComponent<SlimeBarrel> ().DestroyIt ();
			}
			c.GetComponent<Rigidbody>().AddExplosionForce (damage, detonatePosition, range,0.0f,ForceMode.Impulse);
		}
		isDamage = false;
		damage_timer = 0f;
		Destroy (explosionEffectObjectClone,1.6f);
		Destroy (transform.parent.gameObject);
	}
}
