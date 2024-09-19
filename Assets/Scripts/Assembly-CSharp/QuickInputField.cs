using System;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class QuickInputField : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public static Action OnEnter = delegate
	{
	};

	public Text text;

	public bool clickable;

	public int maxCharCount = 20;

	private StringBuilder sb = new StringBuilder(20);

	private bool active;

	private bool ignoreFirst;

	public void OnPointerClick(PointerEventData data)
	{
		if (clickable)
		{
			Activate();
		}
	}

	public void Activate(string newText = "")
	{
		active = true;
		ignoreFirst = true;
		text.text = newText;
	}

	public void Reset()
	{
		active = false;
		if (!clickable)
		{
			text.text = "Type the name *E";
		}
		sb.Clear();
	}

	private string UppercaseFirst(string s)
	{
		char[] array = s.ToCharArray();
		for (int i = 0; i < array.Length; i++)
		{
			if (i == 0 || array[i - 1] == ' ')
			{
				array[i] = char.ToUpper(array[i]);
			}
			else
			{
				array[i] = char.ToLower(array[i]);
			}
		}
		return new string(array);
	}

	private void Update()
	{
		if (!active)
		{
			return;
		}
		if (ignoreFirst)
		{
			ignoreFirst = false;
			return;
		}
		if (sb.Length < maxCharCount && text.text.Length < maxCharCount)
		{
			for (int i = 97; i < 122; i++)
			{
				if (Input.GetKeyDown((KeyCode)i))
				{
					Text obj = text;
					string obj2 = obj.text;
					string obj3;
					if (!Input.GetKey(KeyCode.LeftShift))
					{
						KeyCode keyCode = (KeyCode)i;
						obj3 = keyCode.ToString().ToLower();
					}
					else
					{
						KeyCode keyCode = (KeyCode)i;
						obj3 = keyCode.ToString().ToUpper();
					}
					obj.text = obj2 + obj3;
				}
			}
			for (int j = 48; j < 58; j++)
			{
				if (Input.GetKeyDown((KeyCode)j))
				{
					text.text += j - 48;
				}
			}
			if (Input.GetKeyDown(KeyCode.Space) && text.text.Length > 0)
			{
				text.text += " ";
			}
		}
		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			text.text = text.text.Remove(text.text.Length - 1, 1);
		}
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			if (!QuickmapScene.instance)
			{
				if (sb.Length > 0)
				{
					sb.Clear();
				}
				text.text = "Type the name *E";
				active = false;
				ignoreFirst = true;
			}
			else
			{
				text.text = Quickmap.customMapName;
			}
			if (TryGetComponent<QuickMenu>(out var component))
			{
				component.locked = false;
			}
		}
		if (!Input.GetKeyDown(KeyCode.Return) || text.text.Length <= 1)
		{
			return;
		}
		Quickmap.customMapName = text.text;
		if (!QuickmapScene.instance)
		{
			if (!clickable)
			{
				Game.instance.LoadLevel("QuickMapWorld");
			}
			else
			{
				Reset();
			}
			return;
		}
		QuickmapScene.instance.SaveCurrentMap();
		if (OnEnter != null)
		{
			OnEnter();
		}
		Reset();
	}
}
