using System;
using UnityEngine;
using UnityEngine.Rendering;

public class LightCatcher : MonoBehaviour
{
	public static Action OnLight = delegate
	{
	};

	[SerializeField]
	private Material material;

	private SphericalHarmonicsL2 harmonicsL2;

	private Vector3[] dirs = new Vector3[1];

	private Color[] colors = new Color[1];

	private Color color;

	private bool visible;

	private void Awake()
	{
		dirs[0] = Vector3.up;
		material.color = (visible ? Color.white : Color.black);
	}

	private void Update()
	{
		LightProbes.GetInterpolatedProbe(base.transform.root.position, null, out harmonicsL2);
		harmonicsL2.Evaluate(dirs, colors);
		if (visible != colors[0].grayscale > 0.1f)
		{
			visible = colors[0].grayscale > 0.1f;
			color = (visible ? Color.white : Color.black);
			if (visible && OnLight != null)
			{
				OnLight();
			}
		}
		material.color = Color.Lerp(material.color, color, Time.deltaTime * 10f);
	}
}
