using System;
using UnityEngine;

[Serializable]
public class DamageData
{
	public float amount;

	public Vector3 dir;

	public bool knockdown;

	public DamageType newType;

	[HideInInspector]
	public StylePoint stylePoint;

	public int playerState;

	internal void CopyTo(DamageData target)
	{
		target.amount = amount;
		target.dir = dir;
		target.knockdown = knockdown;
		target.newType = newType;
		target.playerState = playerState;
		target.stylePoint = stylePoint;
	}

	public void Callback(BaseEnemy enemy)
	{
		if ((bool)stylePoint)
		{
			StyleRanking.instance.RegStylePoint(stylePoint);
		}
		else if ((bool)newType)
		{
			newType.Callback(enemy);
		}
	}
}
