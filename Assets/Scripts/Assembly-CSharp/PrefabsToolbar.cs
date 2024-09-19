using QuickmapEditor;
using UnityEngine;

public class PrefabsToolbar : MonoBehaviour
{
	public PrefabsButton[] buttons;

	private void Awake()
	{
		buttons = GetComponentsInChildren<PrefabsButton>();
	}

	public bool GetSelectedPrefab(out GameObject prefab)
	{
		PrefabsButton[] array = buttons;
		foreach (PrefabsButton prefabsButton in array)
		{
			if (prefabsButton.toggled)
			{
				prefab = prefabsButton.prefabs[prefabsButton.index];
				return true;
			}
		}
		prefab = null;
		return false;
	}
}
