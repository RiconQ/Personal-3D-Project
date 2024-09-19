using UnityEngine;
using UnityEngine.UI;

public class MenuItem_KeyBinding : QuickMenuItem
{
	public Text txtLabel;

	public Text txtContent;

	public TextAnimator animLabel;

	public TextAnimator animContent;

	private QuickRebindMenu menu;

	private bool waitForInput;

	private int skip;

	private string temp;

	public override void Awake()
	{
		menu = GetComponentInParent<QuickRebindMenu>();
		base.Awake();
	}

	private void Start()
	{
		animLabel.ResetChars();
		animContent.ResetChars();
	}

	public void Setup(string name, KeyCode key)
	{
		if (txtLabel.text != name)
		{
			txtLabel.text = name;
			animLabel.ResetChars();
		}
		temp = key.ToString();
		switch (key)
		{
		case KeyCode.Mouse0:
			temp = "LMB";
			break;
		case KeyCode.Mouse1:
			temp = "RMB";
			break;
		case KeyCode.Mouse2:
			temp = "Middle Mouse";
			break;
		case KeyCode.Mouse3:
		case KeyCode.Mouse4:
		case KeyCode.Mouse5:
		case KeyCode.Mouse6:
			temp = $"Mouse {(int)(key - 325 + 3)}";
			break;
		case KeyCode.LeftControl:
			temp = "LControl";
			break;
		case KeyCode.RightControl:
			temp = "RControl";
			break;
		case KeyCode.LeftShift:
			temp = "LShift";
			break;
		case KeyCode.RightShift:
			temp = "RShift";
			break;
		}
		txtContent.text = temp;
		animContent.ResetAndPlay();
	}

	public override Vector2 GetSize()
	{
		Vector2 sizeDelta = txtLabel.rectTransform.sizeDelta;
		sizeDelta.x += 12f + txtContent.rectTransform.sizeDelta.x;
		return sizeDelta;
	}

	public override Vector2 GetPosition()
	{
		Vector2 result = default(Vector2);
		result.x = GetSize().x / 2f + base.t.anchoredPosition3D.x - base.t.sizeDelta.x / 2f;
		result.y = base.t.anchoredPosition3D.y;
		return result;
	}

	public override bool Accept()
	{
		if (!waitForInput && skip == 0)
		{
			txtContent.text = "...";
			menu.Invoke("UpdateFrame", Time.deltaTime);
			animContent.ResetAndPlay();
			skip = 3;
			menu.locked = true;
			waitForInput = true;
			return true;
		}
		return false;
	}

	private void Update()
	{
		if (skip > 0)
		{
			skip--;
		}
		if (!waitForInput || skip > 0)
		{
			return;
		}
		for (int i = 0; i < menu.keycodes.Length; i++)
		{
			KeyCode keyCode = (KeyCode)menu.keycodes[i];
			if (!Input.GetKeyDown(keyCode))
			{
				continue;
			}
			int num = -1;
			for (int j = 0; j < menu.inputs.playerKeys.Length; j++)
			{
				if (keyCode == menu.inputs.playerKeys[j].key && j != menu.GetIndex())
				{
					num = j;
					break;
				}
			}
			temp = keyCode.ToString();
			switch (keyCode)
			{
			case KeyCode.Mouse0:
				temp = "LMB";
				break;
			case KeyCode.Mouse1:
				temp = "RMB";
				break;
			case KeyCode.Mouse2:
				temp = "Middle Mouse";
				break;
			case KeyCode.Mouse3:
			case KeyCode.Mouse4:
			case KeyCode.Mouse5:
			case KeyCode.Mouse6:
				temp = $"Mouse {(int)(keyCode - 325 + 3)}";
				break;
			case KeyCode.LeftControl:
				temp = "LControl";
				break;
			case KeyCode.RightControl:
				temp = "RControl";
				break;
			case KeyCode.LeftShift:
				temp = "LShift";
				break;
			case KeyCode.RightShift:
				temp = "RShift";
				break;
			}
			txtContent.text = temp;
			animContent.ResetAndPlay();
			menu.locked = false;
			if (num >= 0)
			{
				menu.SwitchKey(keyCode, num);
			}
			else
			{
				menu.ChangeKey(keyCode);
			}
			menu.Invoke("UpdateFrame", Time.deltaTime);
			waitForInput = false;
			skip = 3;
		}
	}
}
