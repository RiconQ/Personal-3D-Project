using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MCWTiledFloor : MonoBehaviour
{
	public PrefabsArray tiles;

	public PrefabsArray ceilingTiles;

	public List<GameObject> instances = new List<GameObject>();

	private MegaCubeWorld mcWorld;

	private bool[] sides = new bool[4];

	private bool[] diagonals = new bool[4];

	private int rotation;

	private bool a;

	private bool b;

	private Vector3Int temp;

	private void Awake()
	{
		mcWorld = GetComponent<MegaCubeWorld>();
		MegaCubeWorld megaCubeWorld = mcWorld;
		megaCubeWorld.OnChange = (Action)Delegate.Combine(megaCubeWorld.OnChange, new Action(Clear));
	}

	private void OnDestroy()
	{
		MegaCubeWorld megaCubeWorld = mcWorld;
		megaCubeWorld.OnChange = (Action)Delegate.Remove(megaCubeWorld.OnChange, new Action(Clear));
	}

	private void Clear()
	{
		if (instances.Count <= 0)
		{
			return;
		}
		foreach (GameObject instance in instances)
		{
			UnityEngine.Object.DestroyImmediate(instance);
		}
		instances.Clear();
	}

	[Button]
	public void ReplaceFloor()
	{
		Clear();
		if (!mcWorld)
		{
			mcWorld = GetComponent<MegaCubeWorld>();
		}
		MeshFilter[] componentsInChildren = GetComponentsInChildren<MeshFilter>(includeInactive: true);
		GameObject gameObject = GameObject.Find("Floor Tiles");
		if (!gameObject)
		{
			gameObject = new GameObject("Floor Tiles");
		}
		gameObject.transform.SetSiblingIndex(mcWorld.transform.GetSiblingIndex());
		Transform parent = gameObject.transform;
		if ((bool)tiles)
		{
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				if (componentsInChildren[i].gameObject.name == "5")
				{
					componentsInChildren[i].gameObject.SetActive(value: false);
					int num = CheckFloorTileIndex(componentsInChildren[i].transform.position.SnapToInt());
					instances.Add(UnityEngine.Object.Instantiate(tiles.prefabs[num], componentsInChildren[i].transform.position, Quaternion.Euler(0f, rotation * 90, 0f), parent));
				}
			}
		}
		if (!ceilingTiles)
		{
			return;
		}
		for (int j = 0; j < componentsInChildren.Length; j++)
		{
			if (componentsInChildren[j].gameObject.name == "4")
			{
				componentsInChildren[j].gameObject.SetActive(value: false);
				int num2 = CheckFloorTileIndex(componentsInChildren[j].transform.position.SnapToInt(), -1);
				instances.Add(UnityEngine.Object.Instantiate(ceilingTiles.prefabs[num2], componentsInChildren[j].transform.position, Quaternion.Euler(0f, rotation * 90, 180f), parent));
			}
		}
	}

	public int CheckFloorTileIndex(Vector3Int pos, int sign = 1)
	{
		int num = 4;
		for (int i = 0; i < 4; i++)
		{
			a = mcWorld.CheckNeighbour(pos, i);
			b = mcWorld.CheckNeighbour(pos + new Vector3Int(0, 6 * sign, 0), i);
			sides[i] = !a || b;
			if (sides[i])
			{
				num--;
			}
		}
		switch (num)
		{
		case 0:
			return 5;
		case 1:
			if (!sides[0])
			{
				rotation = 0;
			}
			else if (!sides[2])
			{
				rotation = 1;
			}
			else if (!sides[1])
			{
				rotation = 2;
			}
			else
			{
				rotation = 3;
			}
			return 4;
		case 2:
			if (sides[0] && sides[1])
			{
				rotation = 0;
				return 3;
			}
			if (sides[2] && sides[3])
			{
				rotation = 1;
				return 3;
			}
			if (sides[0] && sides[2])
			{
				rotation = 0;
			}
			else if (sides[2] && sides[1])
			{
				rotation = 1;
			}
			else if (sides[1] && sides[3])
			{
				rotation = 2;
			}
			else
			{
				rotation = 3;
			}
			temp = pos;
			temp.z += 6 * (sides[0] ? 1 : (-1));
			temp.x += 6 * (sides[2] ? 1 : (-1));
			diagonals[0] = !mcWorld.CheckCertainNeighbour(temp) || mcWorld.CheckCertainNeighbour(temp + new Vector3Int(0, 6 * sign, 0));
			if (!diagonals[0])
			{
				return 2;
			}
			return 6;
		case 3:
		{
			if (sides[0])
			{
				rotation = 0;
			}
			else if (sides[2])
			{
				rotation = 1;
			}
			else if (sides[1])
			{
				rotation = 2;
			}
			else
			{
				rotation = 3;
			}
			temp = pos;
			Vector3 vector = Quaternion.Euler(0f, rotation * 90, 0f) * new Vector3Int(0, 0, 6);
			temp += vector.SnapToInt();
			temp += Vector3.Cross(vector, Vector3.up).SnapToInt();
			diagonals[0] = !mcWorld.CheckCertainNeighbour(temp) || mcWorld.CheckCertainNeighbour(temp + new Vector3Int(0, 6 * sign, 0));
			Debug.DrawLine(pos, temp, diagonals[0] ? Color.green : Color.red, 4f);
			temp -= Vector3.Cross(vector, Vector3.up).SnapToInt() * 2;
			diagonals[1] = !mcWorld.CheckCertainNeighbour(temp) || mcWorld.CheckCertainNeighbour(temp + new Vector3Int(0, 6 * sign, 0));
			Debug.DrawLine(pos, temp, diagonals[1] ? Color.green : Color.red, 4f);
			if (!diagonals[0] && !diagonals[1])
			{
				return 1;
			}
			if (diagonals[1] && !diagonals[0])
			{
				return 7;
			}
			if (diagonals[0] && !diagonals[1])
			{
				return 8;
			}
			return 9;
		}
		case 4:
		{
			int num2 = 0;
			for (int j = 0; j < 4; j++)
			{
				temp = pos;
				switch (j)
				{
				case 0:
					temp.x -= 6;
					temp.z -= 6;
					break;
				case 1:
					temp.z += 6;
					temp.x -= 6;
					break;
				case 2:
					temp.z += 6;
					temp.x += 6;
					break;
				default:
					temp.z -= 6;
					temp.x += 6;
					break;
				}
				diagonals[j] = !mcWorld.CheckCertainNeighbour(temp) || mcWorld.CheckCertainNeighbour(temp + new Vector3Int(0, 6 * sign, 0));
				if (diagonals[j])
				{
					num2++;
				}
			}
			switch (num2)
			{
			case 0:
				return 0;
			case 1:
				if (diagonals[0])
				{
					rotation = 0;
				}
				else if (diagonals[1])
				{
					rotation = 1;
				}
				else if (diagonals[2])
				{
					rotation = 2;
				}
				else
				{
					rotation = 3;
				}
				return 10;
			case 2:
				if (diagonals[0] && diagonals[2])
				{
					rotation = 0;
					return 12;
				}
				if (diagonals[1] && diagonals[3])
				{
					rotation = 1;
					return 12;
				}
				if (diagonals[0] && diagonals[1])
				{
					rotation = 0;
				}
				else if (diagonals[1] && diagonals[2])
				{
					rotation = 1;
				}
				else if (diagonals[2] && diagonals[3])
				{
					rotation = 2;
				}
				else
				{
					rotation = 3;
				}
				return 11;
			case 4:
				return 13;
			default:
				return 0;
			}
		}
		default:
			return 0;
		}
	}
}
