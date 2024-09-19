using UnityEngine;

public class ThreatsUI : MonoBehaviour
{
	public static ThreatsUI instance;

	private RectTransform tCanvas;

	private ThreatIndicator[] indicators;

	private void Awake()
	{
		instance = this;
		tCanvas = GetComponentInParent<Canvas>().GetComponent<RectTransform>();
		indicators = GetComponentsInChildren<ThreatIndicator>();
	}

	public void SetTarget(Transform t)
	{
		ThreatIndicator[] array = indicators;
		foreach (ThreatIndicator threatIndicator in array)
		{
			if (!threatIndicator.target)
			{
				threatIndicator.Set(t);
				break;
			}
		}
	}

	private void Update()
	{
		ThreatIndicator[] array = indicators;
		foreach (ThreatIndicator threatIndicator in array)
		{
			if ((bool)threatIndicator.target)
			{
				threatIndicator.Tick(tCanvas);
			}
		}
	}
}
