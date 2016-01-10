using UnityEngine;
using System;
using System.Collections;

//[ExecuteInEditMode()]
public class RealtimeReflections : MonoBehaviour
{
	public int cubemapSize = 128;
	public float nearClip = 0.01f;
	public float farClip = 500;
	public bool oneFacePerFrame = false;
	public Material[] materials;
    public ReflectionProbe[] reflectionProbes;
	public LayerMask layerMask = -1;
	private Camera cam;
	private RenderTexture renderTexture;

	void OnEnable(){
		layerMask.value = -1;
	}

	void Start()
    {
        foreach (var probe in reflectionProbes) {
            probe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
            probe.type = UnityEngine.Rendering.ReflectionProbeType.Cube;
            probe.boxProjection = true;
            probe.resolution = cubemapSize;//jsbd
            probe.transform.parent = transform.parent;
            probe.transform.localPosition = Vector3.zero;
        }

        if (materials.Length <= 0)
            return;

		UpdateCubemap(63);
	}

	void LateUpdate()
    {
        if (materials.Length <= 0)
            return;

		if (oneFacePerFrame)
		{
			int faceToRender = Time.frameCount % 6;
			int faceMask = 1 << faceToRender;
			UpdateCubemap(faceMask);
		}
		else
		{
			UpdateCubemap(63); // all six faces
		}
	}

	void UpdateCubemap(int faceMask)
	{
		if (!cam)
		{
			GameObject go = new GameObject("CubemapCamera", typeof(Camera));
			go.hideFlags = HideFlags.HideAndDontSave;
			go.transform.position = transform.position;
			go.transform.rotation = Quaternion.identity;
			cam = go.GetComponent<Camera>();
			cam.cullingMask = layerMask;
			cam.nearClipPlane = nearClip;
			cam.farClipPlane = farClip;
			cam.enabled = false;
		}

		if (!renderTexture)
		{
			renderTexture = new RenderTexture(cubemapSize, cubemapSize, 16);
			renderTexture.isPowerOfTwo = true;
			renderTexture.isCubemap = true;
			renderTexture.hideFlags = HideFlags.HideAndDontSave;
			foreach (Renderer r in GetComponentsInChildren<Renderer>())
			{
				foreach (Material m in r.sharedMaterials)
				{
					if (m.HasProperty("_Cube"))
						m.SetTexture("_Cube", renderTexture);
				}
			}

            foreach (var probe in reflectionProbes)
                probe.customBakedTexture = renderTexture;
		}

		cam.transform.position = transform.position;
		cam.RenderToCubemap(renderTexture, faceMask);
	}

	void OnDisable()
	{
		DestroyImmediate(cam);
		DestroyImmediate(renderTexture);
	}
}
