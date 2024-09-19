using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TheEndless : MonoBehaviour
{
	public MegaCubeWorld world;

	public NavMeshSurface surface;

	public OffMeshLinkManager links;

	public int startY;

	public Vector3Int aPos;

	public Vector3Int bPos;

	public Vector3 startPos;

	public GameObject enemy;

	public List<BaseEnemy> enemies = new List<BaseEnemy>();

	public int index;

	private void Start()
	{
		startPos = Game.player.t.position;
		startY = Mathf.RoundToInt(startPos.y);
		aPos.x = 0;
		bPos.x = 18;
		aPos.z = 0;
		bPos.z = 18;
		for (int i = 0; i < 10; i++)
		{
			BaseEnemy component = UnityEngine.Object.Instantiate(enemy, Vector3.one, Quaternion.identity).GetComponent<BaseEnemy>();
			component.ManualReset = true;
			component.gameObject.SetActive(value: false);
			enemies.Add(component);
		}
		world.ClearAll();
		world.FillRegionAtPoint(Vector3Int.zero);
		world.CreateMeshRuntime();
		CrowdControl.OnLastActiveDied = (Action)Delegate.Combine(CrowdControl.OnLastActiveDied, new Action(Next));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void OnDestroy()
	{
		CrowdControl.OnLastActiveDied = (Action)Delegate.Remove(CrowdControl.OnLastActiveDied, new Action(Next));
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
	}

	private void Next()
	{
		startY += 12;
		ref Vector3Int reference = ref aPos;
		int y = (bPos.y = startY);
		reference.y = y;
		int num2 = UnityEngine.Random.Range(-6, 6);
		int num3 = UnityEngine.Random.Range(-6, 6);
		aPos.x += 6 * num2;
		bPos.x += 6 * num2;
		aPos.z += 6 * num3;
		bPos.z += 6 * num3;
		world.TranslateWorldPosition(aPos, out aPos);
		world.TranslateWorldPosition(bPos, out bPos);
		world.posA = aPos;
		world.posB = bPos;
		world.AddArea(aPos, bPos, add: true);
		surface.BuildNavMesh();
		links.CreateLinks();
		enemies[index].Reset();
		enemies[index].t.position = aPos + Vector3.up * 3f;
		enemies[index].gameObject.SetActive(value: true);
		index = index.Next(enemies.Count);
	}

	private void Reset()
	{
		startY = Mathf.RoundToInt(startPos.y);
		aPos.x = 0;
		bPos.x = 18;
		aPos.z = 0;
		bPos.z = 18;
		foreach (BaseEnemy enemy in enemies)
		{
			enemy.Reset();
			enemy.DeactivateEnemy();
		}
		world.ClearAll();
		world.FillRegionAtPoint(Vector3Int.zero);
		world.CreateMeshRuntime();
	}

	private void Update()
	{
		Input.GetKeyDown(KeyCode.G);
	}
}
