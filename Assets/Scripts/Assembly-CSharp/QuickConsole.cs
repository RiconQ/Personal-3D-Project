using UnityEngine;
using UnityEngine.UI;

public class QuickConsole : QuickUI
{
	private InputField inputField;

	public override void Awake()
	{
		base.Awake();
		inputField = GetComponentInChildren<InputField>();
	}

	public override void Activate()
	{
		base.Activate();
		inputField.ActivateInputField();
		inputField.Select();
	}

	public override void Deactivate()
	{
		base.Deactivate();
		if ((bool)inputField)
		{
			inputField.DeactivateInputField();
		}
		Game.message.Hide();
	}

	public override void Back()
	{
	}

	protected override void Update()
	{
		base.Update();
		if (Input.GetKeyDown(KeyCode.BackQuote) && QuickUI.lastActive != this)
		{
			QuickUI.lastActive.Deactivate();
			Activate();
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			base.Back();
		}
		if (!Input.GetKeyDown(KeyCode.Return))
		{
			return;
		}
		string[] array = inputField.text.Split('/');
		inputField.text = "";
		inputField.ActivateInputField();
		inputField.Select();
		switch (array.Length)
		{
		case 2:
			switch (array[0])
			{
			case "mousesens":
			{
				int result3 = 0;
				int.TryParse(array[1], out result3);
				if (result3 > 0)
				{
					if (!GamePrefs.cached.ContainsKey("MouseSensitivity"))
					{
						GamePrefs.cached.Add("MouseSensitivity", result3);
					}
					else
					{
						Game.gamePrefs.UpdateValue("MouseSensitivity", result3);
					}
					Game.message.Show($"Mouse Sens. {result3}");
				}
				break;
			}
			case "setres":
			{
				string[] array2 = array[1].Split('x');
				int result = 0;
				int result2 = 0;
				int.TryParse(array2[0], out result);
				int.TryParse(array2[1], out result2);
				if (result > 460 && result2 > 320)
				{
					Screen.SetResolution(result, result2, Screen.fullScreen);
					if (MenuItem_Resolution.OnResolutionChange != null)
					{
						MenuItem_Resolution.OnResolutionChange();
					}
				}
				break;
			}
			case "load":
				break;
			}
			break;
		case 1:
			switch (array[0])
			{
			case "debug":
				Game.debug = !Game.debug;
				Game.message.Show($"debug {Game.debug}");
				break;
			case "clear":
			case "reset":
				PlayerPrefs.DeleteAll();
				LevelsData.instance.ClearAll();
				Game.instance.Restart();
				Game.fading.InstantFade(1f);
				break;
			case "unlock":
			{
				PlayerPrefs.DeleteAll();
				LevelsData.instance.ClearAll();
				PlayerPrefs.SetInt(GamePrefs.resetcode, 1);
				HubData[] hubs = LevelsData.instance.hubs;
				foreach (HubData hubData2 in hubs)
				{
					PlayerPrefs.SetInt($"{hubData2.GetFirstLevel().sceneName}_progress", 1);
				}
				SceneData[] levels = LevelsData.instance.levels;
				foreach (SceneData sceneData in levels)
				{
					if (sceneData.IsPlayebleNotHub())
					{
						PlayerPrefs.SetFloat($"{sceneData.sceneName}_time", 99.99f);
						PlayerPrefs.SetInt($"{sceneData.sceneName}_rank", 1);
						PlayerPrefs.SetInt($"{sceneData.sceneName}_reached", 1);
					}
				}
				Game.instance.Restart();
				Game.fading.InstantFade(1f);
				break;
			}
			case "skiptut":
			{
				PlayerPrefs.DeleteAll();
				LevelsData.instance.ClearAll();
				PlayerPrefs.SetInt($"{LevelsData.instance.hubs[1].GetFirstLevel().sceneName}_progress", 1);
				foreach (SceneData level in LevelsData.instance.hubs[1].levels)
				{
					if (level.IsPlayebleNotHub())
					{
						PlayerPrefs.SetFloat($"{level.sceneName}_time", 99.99f);
					}
				}
				HubData[] hubs = LevelsData.instance.hubs;
				foreach (HubData hubData in hubs)
				{
					PlayerPrefs.SetInt($"{hubData.GetFirstLevel().sceneName}_progress", 1);
				}
				Game.instance.Restart();
				Game.fading.InstantFade(1f);
				break;
			}
			}
			break;
		case 0:
			break;
		}
	}
}
