using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableHashSet : HashSet<Vector3Int>, ISerializationCallbackReceiver
{
	[NonSerialized]
	public HashSet<Vector3Int> Keys = new HashSet<Vector3Int>();

	[SerializeField]
	private List<Vector3Int> serializedKeys = new List<Vector3Int>();

	public void OnBeforeSerialize()
	{
		serializedKeys.Clear();
		foreach (Vector3Int key in Keys)
		{
			serializedKeys.Add(key);
		}
	}

	public void OnAfterDeserialize()
	{
		Keys.Clear();
		foreach (Vector3Int serializedKey in serializedKeys)
		{
			Keys.Add(serializedKey);
		}
	}
}
