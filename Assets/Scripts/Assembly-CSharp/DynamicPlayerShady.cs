using System;
using UnityEngine;

public class DynamicPlayerShady : MonoBehaviour
{
	public float fall;

	public float Layer1Volume;

	public float Layer2Volume;

	public float Layer3Volume;

	public DynamicPlayer player;

	private void Awake()
	{
		BaseEnemy.OnAnyEnemyDie = (Action)Delegate.Combine(BaseEnemy.OnAnyEnemyDie, new Action(Check));
	}

	private void OnDestroy()
	{
		BaseEnemy.OnAnyEnemyDie = (Action)Delegate.Remove(BaseEnemy.OnAnyEnemyDie, new Action(Check));
	}

	private void Check()
	{
		Layer3Volume = ((CrowdControl.instance.activeEnemies > 0) ? 1 : 0);
		player.SetSourceVolume(2, Layer3Volume);
		Game.audioManager.Gain(2.5f);
	}

	private void Update()
	{
		Layer1Volume = Mathf.MoveTowards(Layer1Volume, 1f, Time.deltaTime);
		player.SetSourceVolume(0, Layer1Volume);
		Layer2Volume = Mathf.MoveTowards(Layer2Volume, ((StyleRanking.instance.combo.timer > 0.25f) & (StyleRanking.instance.combo.combo > 3)) ? 1 : 0, Time.deltaTime);
		player.SetSourceVolume(1, Layer2Volume);
		Layer3Volume = Mathf.MoveTowards(Layer3Volume, (CrowdControl.instance.activeEnemies > 0) ? 1 : 0, Time.deltaTime);
		player.SetSourceVolume(2, Layer3Volume);
	}
}
