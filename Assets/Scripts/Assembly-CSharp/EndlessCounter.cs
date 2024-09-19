using System;
using UnityEngine;

public class EndlessCounter : MonoBehaviour
{
	public GameObject bonfirePrefab;

	public int maxCount = 9;

	private int count;

	private ArenaBonfire[] bonfires;

	private void Start()
	{
		bonfires = new ArenaBonfire[maxCount];
		for (int i = 0; i < maxCount; i++)
		{
			bonfires[i] = UnityEngine.Object.Instantiate(bonfirePrefab, base.transform).GetComponent<ArenaBonfire>();
			bonfires[i].Setup();
			bonfires[i].t.localPosition = Quaternion.Euler(0f, 360f / (float)maxCount * (float)i, 0f) * (-Vector3.forward * 10f);
			bonfires[i].t.LookAt(base.transform.position);
		}
		BaseEnemy.OnEnenyDie = (Action<BaseEnemy>)Delegate.Combine(BaseEnemy.OnEnenyDie, new Action<BaseEnemy>(OnEnemyDie));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		BaseEnemy.OnEnenyDie = (Action<BaseEnemy>)Delegate.Remove(BaseEnemy.OnEnenyDie, new Action<BaseEnemy>(OnEnemyDie));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Reset()
	{
		count = 0;
		for (int i = 0; i < bonfires.Length; i++)
		{
			bonfires[i].Reset();
		}
	}

	private void OnEnemyDie(BaseEnemy e)
	{
		if (Game.mission.state <= MissionState.MissionStates.InProcess && count < maxCount)
		{
			count++;
			bonfires[count - 1].Activate();
			if (count == maxCount)
			{
				Game.mission.SetState(2);
				CrowdControl.instance.KillTheRest();
				CameraController.shake.Shake();
			}
		}
	}
}
