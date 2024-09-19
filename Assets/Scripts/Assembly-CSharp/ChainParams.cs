using UnityEngine;

[CreateAssetMenu(fileName = "Chain Params", menuName = "My Presets/Chain Params", order = 1)]
public class ChainParams : ScriptableObject
{
	public AnimationCurve curve = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public Vector2 size = new Vector2(0.4f, 0.4f);

	public Material mat;
}
