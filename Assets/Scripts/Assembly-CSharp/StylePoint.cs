using UnityEngine;

[CreateAssetMenu(fileName = "Stylepoint", menuName = "Ranking/Stylepoint")]
public class StylePoint : ScriptableObject
{
	public enum Type
	{
		Action = 0,
		Chaos = 1,
		Fatal = 2
	}

	public string publicName;

	[TextArea]
	public string discription;

	public Type type;

	public string GetPublicName()
	{
		if (publicName.Length <= 0)
		{
			return base.name;
		}
		return publicName;
	}
}
