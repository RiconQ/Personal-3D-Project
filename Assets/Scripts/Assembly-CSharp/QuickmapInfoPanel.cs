using System;
using UnityEngine;
using UnityEngine.UI;

public class QuickmapInfoPanel : MonoBehaviour
{
	public RectTransform[] tPanels;

	public CanvasGroup[] cgs;

	public Text txtLevelName;

	private int index;

	private void Awake()
	{
		if (Quickmap.customMapName.Length > 0)
		{
			txtLevelName.text = Quickmap.customMapName.ToUpper();
		}
		for (int i = 0; i < tPanels.Length; i++)
		{
			tPanels[i].anchoredPosition3D = new Vector3(0f, 16f, 0f);
			cgs[i].alpha = 0f;
		}
		QuickmapScene.OnStateChanged = (Action<int>)Delegate.Combine(QuickmapScene.OnStateChanged, new Action<int>(OnStateChanged));
	}

	private void OnDestroy()
	{
		QuickmapScene.OnStateChanged = (Action<int>)Delegate.Remove(QuickmapScene.OnStateChanged, new Action<int>(OnStateChanged));
	}

	private void OnStateChanged(int stateIndex)
	{
	}

	private void Update()
	{
		if (index != ((MyButton.last != null) ? 1 : 0))
		{
			index = ((MyButton.last != null) ? 1 : 0);
			for (int i = 0; i < cgs.Length; i++)
			{
				cgs[i].alpha = 0f;
			}
		}
		if (cgs[index].alpha != 1f)
		{
			cgs[index].alpha = Mathf.MoveTowards(cgs[index].alpha, 1f, Time.deltaTime * 2f);
		}
	}
}
