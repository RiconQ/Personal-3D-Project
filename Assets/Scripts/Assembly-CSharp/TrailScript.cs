using UnityEngine;

public class TrailScript : MonoBehaviour
{
	public Gradient colorGradient;

	public bool playOnEnable;

	public bool deactivateOnEnable;

	private MaterialPropertyBlock block;

	private int state = -1;

	private float timer;

	private float scrollValue;

	public float speed = 5f;

	public Vector2 scroll = new Vector2(0.5f, -1f);

	public Vector3 aSize = new Vector3(0.75f, 0.75f, 0.75f);

	public Vector3 bSize = new Vector3(1.25f, 1.25f, 1.25f);

	public Transform t { get; private set; }

	public MeshFilter filter { get; private set; }

	public MeshRenderer rend { get; private set; }

	private void Awake()
	{
		t = base.transform;
		filter = GetComponentInChildren<MeshFilter>();
		rend = GetComponentInChildren<MeshRenderer>();
		rend.enabled = false;
		block = new MaterialPropertyBlock();
		rend.GetPropertyBlock(block);
		if (playOnEnable)
		{
			t.SetParent(null);
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnEnable()
	{
		if (playOnEnable)
		{
			Play();
		}
	}

	private void Update()
	{
		switch (state)
		{
		case 0:
			timer = 0f;
			scrollValue = 0.5f;
			t.localScale = aSize;
			block.SetVector("_MainTex_ST", new Vector4(1f, 1f, scrollValue, 0f));
			block.SetColor("_TintColor", colorGradient.Evaluate(timer));
			rend.SetPropertyBlock(block);
			rend.enabled = true;
			state++;
			break;
		case 1:
			timer = Mathf.MoveTowards(timer, 1f, Time.unscaledDeltaTime * speed);
			scrollValue = Mathf.Lerp(scroll.x, scroll.y, timer);
			block.SetVector("_MainTex_ST", new Vector4(1f, 1f, scrollValue, 0f));
			block.SetColor("_TintColor", colorGradient.Evaluate(timer));
			rend.SetPropertyBlock(block);
			t.localScale = Vector3.Lerp(aSize, bSize, timer);
			if (timer == 1f)
			{
				state++;
			}
			break;
		case 2:
			rend.enabled = false;
			state = -1;
			if (deactivateOnEnable)
			{
				base.gameObject.SetActive(value: false);
			}
			break;
		}
	}

	public void SetColor(Color c)
	{
		rend.GetPropertyBlock(block);
		block.SetColor("_TintColor", c);
		rend.SetPropertyBlock(block);
	}

	public void Play()
	{
		state = 0;
	}

	public void Play(Vector3 localAngles)
	{
		t.localEulerAngles = localAngles;
		state = 0;
	}

	public void Play(Vector3 localAngles, int xScale = 1)
	{
		t.localEulerAngles = localAngles;
		t.localScale = t.localScale.With(xScale);
		state = 0;
	}

	public void Reset()
	{
		state = -1;
		if ((bool)rend)
		{
			rend.enabled = false;
		}
	}

	private void OnDrawGizmos()
	{
		if ((bool)rend && rend.enabled)
		{
			Gizmos.matrix = t.localToWorldMatrix;
			Gizmos.DrawWireCube(Vector3.zero, filter.sharedMesh.bounds.size);
		}
	}
}
