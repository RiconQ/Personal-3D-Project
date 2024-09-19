using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GamePrefs : MonoBehaviour
{
	public static Action<string> OnValueUpdated = delegate
	{
	};

	public static Dictionary<string, int> cached = new Dictionary<string, int>();

	public static string resetcode = "26012023-2131";

	public KeyboardInputs inputs;

	private string savePath;

	public int GetValue(string prefs)
	{
		int num = 0;
		if (!cached.ContainsKey(prefs))
		{
			num = PlayerPrefs.GetInt(prefs);
			cached.Add(prefs, num);
		}
		else
		{
			num = cached[prefs];
		}
		return num;
	}

	public void UpdateValue(string prefs, int value)
	{
		if (cached.TryGetValue(prefs, out var value2) && value2 != value)
		{
			cached[prefs] = value;
			if (OnValueUpdated != null)
			{
				OnValueUpdated(prefs);
			}
		}
	}

	private void Awake()
	{
		savePath = Path.Combine(Application.persistentDataPath, "savefile");
		Resources.Load("app_build_time");
		if (PlayerPrefs.GetInt(resetcode) == 0)
		{
			PlayerPrefs.DeleteAll();
			PlayerPrefs.SetInt(resetcode, 1);
			Setup();
			inputs.Reset();
			Save();
		}
		else
		{
			Load();
		}
	}

	private void Setup()
	{
		PlayerPrefs.SetInt("PostProcessing", 1);
		PlayerPrefs.SetInt("Bloom", 1);
		PlayerPrefs.SetInt("MouseSensitivity", 10);
		PlayerPrefs.SetInt("FOV", 2);
		PlayerPrefs.SetInt("Sounds Volume", 8);
		PlayerPrefs.SetInt("Music Volume", 8);
		PlayerPrefs.SetInt("Tips", 1);
		PlayerPrefs.SetInt("PlayerUI", 1);
		cached.Add("PostProcessing", 1);
		cached.Add("Bloom", 1);
		cached.Add("MouseSensitivity", 10);
		cached.Add("FOV", 2);
		cached.Add("Sounds Volume", 8);
		cached.Add("Music Volume", 8);
		cached.Add("Tips", 1);
		cached.Add("PlayerUI", 1);
	}

	private void Save()
	{
		using BinaryWriter binaryWriter = new BinaryWriter(File.Open(savePath, FileMode.Create));
		binaryWriter.Write(inputs.playerKeys.Length);
		for (int i = 0; i < inputs.playerKeys.Length; i++)
		{
			binaryWriter.Write((int)inputs.playerKeys[i].key);
		}
	}

	private void Load()
	{
		using BinaryReader binaryReader = new BinaryReader(File.Open(savePath, FileMode.Open));
		int num = binaryReader.ReadInt32();
		for (int i = 0; i < num; i++)
		{
			inputs.playerKeys[i].key = (KeyCode)binaryReader.ReadInt32();
		}
	}

	private void OnApplicationQuit()
	{
		foreach (KeyValuePair<string, int> item in cached)
		{
			PlayerPrefs.SetInt(item.Key, item.Value);
		}
		Save();
	}
}
