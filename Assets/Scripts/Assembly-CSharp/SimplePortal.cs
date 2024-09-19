using UnityEngine;

public class SimplePortal : MonoBehaviour
{
	public Transform t;

	public Camera cam;

	public int channel;

	private bool opened;

	private Vector3 pos;

	private Vector3 vel;

	private Quaternion rot;

	private RenderTexture renderTexture;

	private GameObject objMesh;

	private MaterialPropertyBlock block;

	private void Awake()
	{
		t = base.transform;
		objMesh = t.Find("Mesh").gameObject;
		PortalsManager.instance.AddPortal(this);
	}

	public void Setup(PortalPoint point)
	{
		renderTexture = new RenderTexture(Screen.width / 2, Screen.height / 2, 16, RenderTextureFormat.ARGB32);
		renderTexture.Create();
		GameObject gameObject = new GameObject("Portal Camera");
		gameObject.transform.SetParent(point.transform);
		gameObject.transform.localPosition = Vector3.zero;
		cam = gameObject.AddComponent<Camera>();
		cam.targetTexture = renderTexture;
		cam.renderingPath = RenderingPath.DeferredShading;
		cam.gameObject.SetActive(value: false);
		block = new MaterialPropertyBlock();
		objMesh.GetComponent<MeshRenderer>().GetPropertyBlock(block);
		Open();
	}

	public void Open()
	{
		block.SetTexture("_MainTex", renderTexture);
		objMesh.GetComponent<MeshRenderer>().SetPropertyBlock(block);
		cam.gameObject.SetActive(value: true);
		opened = true;
	}

	private void LateUpdate()
	{
		if (opened)
		{
			cam.fieldOfView = Camera.main.fieldOfView;
			pos = t.InverseTransformPoint(Camera.main.transform.position);
			rot = Quaternion.LookRotation(t.InverseTransformDirection(Camera.main.transform.forward));
			cam.transform.localPosition = pos;
			cam.transform.localRotation = rot;
			cam.nearClipPlane = pos.magnitude;
		}
	}

	private void OnTriggerEnter(Collider c)
	{
		if (opened)
		{
			c.transform.position = cam.transform.position;
			c.GetComponentInChildren<MouseLook>().LookInDir(cam.transform.forward);
			c.attachedRigidbody.velocity = cam.transform.TransformDirection(t.InverseTransformDirection(c.attachedRigidbody.velocity)) * 1.25f;
			Game.fading.InstantFade(1f);
			Game.fading.Fade(0f);
			QuickEffectsPool.Get("Orb Explosion", cam.transform.position + cam.transform.forward * 2f, Quaternion.LookRotation(cam.transform.forward)).Play();
		}
	}

	private void OnDrawGizmosSelected()
	{
		if ((bool)t && (bool)cam)
		{
			Debug.DrawLine(t.position, cam.transform.position, Color.green);
			Debug.DrawRay(t.position, t.forward, Color.yellow);
			Debug.DrawRay(cam.transform.position, cam.transform.forward, Color.yellow);
		}
	}
}
