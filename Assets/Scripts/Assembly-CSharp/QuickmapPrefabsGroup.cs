using System;
using UnityEngine;

[Serializable]
public class QuickmapPrefabsGroup
{
	public CanvasGroup cg;

	public string groupName;

	public int startIndex;

	public int endIndex;

	public bool lookAtCamera;

	public bool snapToCellCenter;
}
