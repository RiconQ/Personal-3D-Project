using System;
using UnityEngine;

[Serializable]
public class CrowdToken
{
	public BaseEnemy actor;

	public float delay;

	public void Setup(BaseEnemy e)
	{
		actor = e;
		delay = 0.5f;
	}

	public bool Tick()
	{
		delay = Mathf.MoveTowards(delay, 0f, Time.deltaTime);
		if (delay == 0f || !actor.isActiveAndEnabled || actor.dead || !actor.tTarget)
		{
			actor = null;
		}
		return actor;
	}
}
