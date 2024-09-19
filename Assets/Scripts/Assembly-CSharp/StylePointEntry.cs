using System;

[Serializable]
public class StylePointEntry : IEquatable<StylePointEntry>
{
	public float timer;

	public int count;

	public StylePointTypes type;

	public StylePointCard card;

	public override bool Equals(object obj)
	{
		if (!(obj is StylePointEntry other))
		{
			return false;
		}
		return Equals(other);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public bool Equals(StylePointEntry other)
	{
		return type == other.type;
	}
}
