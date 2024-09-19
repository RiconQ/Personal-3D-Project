using UnityEngine;

[CreateAssetMenu]
public class DamageType : ScriptableObject
{
	public enum Finisher
	{
		Default = 0,
		Sword = 1,
		Arrow = 2,
		Impile = 3,
		Mill = 4,
		Rip = 5,
		Spear = 6
	}

	public bool pushBody;

	public float pushForce;

	public Vector3 pushDirOffset;

	public bool rotateBody;

	public Vector3 rotation;

	public bool stun;

	public bool fire;

	public bool stugger;

	public bool kick;

	public bool slideKick;

	public Vector3 overrideKickDir;

	public Finisher finisher;

	public bool onlyIfKnocked;

	public StylePoint pointA;

	public StylePoint pointB;

	public StylePoint pointC;

	public void Callback(BaseEnemy enemy)
	{
		if (enemy.dead)
		{
			StyleRanking.instance.RegStylePoint(pointC);
		}
		else if (enemy.isActiveAndEnabled)
		{
			if (!onlyIfKnocked)
			{
				StyleRanking.instance.RegStylePoint(pointA);
			}
		}
		else if (enemy.body.lifetime == 0f)
		{
			StyleRanking.instance.RegStylePoint(pointA);
		}
		else
		{
			StyleRanking.instance.RegStylePoint(pointB);
		}
	}
}
