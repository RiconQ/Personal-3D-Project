using UnityEngine;
using UnityEngine.UI;

public class TextWorldPosition : MonoBehaviour
{
	public RectTransform t;

	public RectTransform tTxt;

	public RectTransform tFrame;

	public RectTransform tCanvas;

	public CanvasScaler scaler;

	public Transform tTarget;

	public Vector3 offset;

	public Vector3 pos;

	public Vector3 targetScreenPos;

	private CanvasGroup cg;

	private void Start()
	{
		Invoke("UpdateSize", Time.fixedDeltaTime);
	}

	private void UpdateSize()
	{
		tFrame.sizeDelta = tTxt.sizeDelta;
	}

	private void Update()
	{
		cg = GetComponent<CanvasGroup>();
	}

	private void LateUpdate()
	{
		targetScreenPos = Camera.main.WorldToScreenPoint(tTarget.position);
		targetScreenPos.x *= tCanvas.sizeDelta.x / (float)Screen.width;
		targetScreenPos.y *= tCanvas.sizeDelta.y / (float)Screen.height;
		pos.x = Mathf.Clamp(targetScreenPos.x + offset.x, tTxt.sizeDelta.x / 2f, tCanvas.sizeDelta.x * scaler.scaleFactor - tTxt.sizeDelta.x / 2f);
		pos.y = Mathf.Clamp(targetScreenPos.y + offset.y, tTxt.sizeDelta.y / 2f, tCanvas.sizeDelta.y * scaler.scaleFactor - tTxt.sizeDelta.y / 2f);
		pos.z = 0f;
		t.anchoredPosition3D = pos;
		cg.alpha = ((targetScreenPos.z > 0f) ? 1 : 0);
	}
}
