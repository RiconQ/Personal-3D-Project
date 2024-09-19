using System;
using UnityEngine;
using UnityEngine.UI;

public class WorldTextTrigger : MonoBehaviour
{
	public enum PlayerActionInput
	{
		None = 0,
		Kick = 1,
		Slide = 2,
		Jump = 3,
		Restart = 4,
		Attack = 5,
		AltAttack = 6,
		Pick = 7
	}

	[HideInInspector]
	public RectTransform tCanvas;

	[HideInInspector]
	public RectTransform tText;

	[HideInInspector]
	public CanvasGroup cg;

	[HideInInspector]
	public Text txt;

	[TextArea]
	public string text = "TEST";

	public bool ifNoActiveEnemies;

	public PlayerActionInput tipAction;

	private bool isActivated;

	private float alpha;

	private string hashedText;

	public void Setup()
	{
		base.gameObject.layer = 12;
		BoxCollider boxCollider = base.gameObject.AddComponent<BoxCollider>();
		boxCollider.size = new Vector3(6f, 6f, 6f);
		boxCollider.isTrigger = true;
		GameObject gameObject = new GameObject("Canvas", typeof(RectTransform));
		gameObject.AddComponent<Canvas>();
		tCanvas = gameObject.GetComponent<RectTransform>();
		tCanvas.SetParent(base.gameObject.transform, worldPositionStays: false);
		tCanvas.localScale = new Vector3(0.02f, 0.02f, 0.02f);
		tCanvas.sizeDelta = new Vector2(256f, 256f);
		gameObject = new GameObject("Text", typeof(RectTransform));
		tText = gameObject.GetComponent<RectTransform>();
		tText.SetParent(tCanvas, worldPositionStays: false);
		tText.anchorMin = new Vector2(0f, 0f);
		tText.anchorMax = new Vector2(1f, 1f);
		tText.offsetMin = Vector3.zero;
		tText.offsetMax = Vector3.zero;
		tText.anchoredPosition3D = new Vector3(0f, 0f, -1f);
		cg = gameObject.AddComponent<CanvasGroup>();
		txt = gameObject.AddComponent<Text>();
		txt.text = "TEST";
		txt.alignment = TextAnchor.MiddleCenter;
		txt.lineSpacing = 0.8f;
		txt.fontSize = 32;
	}

	private void Awake()
	{
		cg.alpha = 0f;
		PlayerHead.OnGameQuickReset = (Action)Delegate.Combine(PlayerHead.OnGameQuickReset, new Action(Reset));
		GamePrefs.OnValueUpdated = (Action<string>)Delegate.Combine(GamePrefs.OnValueUpdated, new Action<string>(CheckSettings));
		QuickRebindMenu.OnRebinded = (Action)Delegate.Combine(QuickRebindMenu.OnRebinded, new Action(UpdateActionText));
		PlayerController.OnInputTypeChange = (Action)Delegate.Combine(PlayerController.OnInputTypeChange, new Action(UpdateActionText));
		base.gameObject.SetActive(Game.gamePrefs.GetValue("Tips") == 1);
	}

	private void OnDestroy()
	{
		PlayerHead.OnGameQuickReset = (Action)Delegate.Remove(PlayerHead.OnGameQuickReset, new Action(Reset));
		GamePrefs.OnValueUpdated = (Action<string>)Delegate.Remove(GamePrefs.OnValueUpdated, new Action<string>(CheckSettings));
		QuickRebindMenu.OnRebinded = (Action)Delegate.Remove(QuickRebindMenu.OnRebinded, new Action(UpdateActionText));
		PlayerController.OnInputTypeChange = (Action)Delegate.Remove(PlayerController.OnInputTypeChange, new Action(UpdateActionText));
	}

	private void Start()
	{
		UpdateActionText();
	}

	private void UpdateActionText()
	{
		if (tipAction != 0)
		{
			if (PlayerController.gamepad)
			{
				txt.text = string.Format(text, PlayerController.instance.inputs.playerKeys[(int)(tipAction + 3)].joy);
				return;
			}
			KeyCode key = PlayerController.instance.inputs.playerKeys[(int)(tipAction + 3)].key;
			string arg = key.ToString();
			switch (key)
			{
			case KeyCode.Mouse0:
				arg = "LMB";
				break;
			case KeyCode.Mouse1:
				arg = "RMB";
				break;
			case KeyCode.Mouse2:
				arg = "Middle Mouse";
				break;
			case KeyCode.Mouse3:
			case KeyCode.Mouse4:
			case KeyCode.Mouse5:
			case KeyCode.Mouse6:
				arg = $"Mouse {(int)(key - 325 + 3)}";
				break;
			}
			txt.text = string.Format(text, arg);
		}
		else
		{
			txt.text = text;
		}
	}

	private void CheckSettings(string prefs)
	{
		if (prefs == "Tips")
		{
			base.gameObject.SetActive(Game.gamePrefs.GetValue(prefs) == 1);
		}
	}

	private void Reset()
	{
		if ((bool)cg)
		{
			cg.alpha = (alpha = 0f);
			isActivated = false;
		}
	}

	private void Update()
	{
		if (alpha != (float)(isActivated ? 1 : 0))
		{
			alpha = Mathf.MoveTowards(alpha, (isActivated & (!ifNoActiveEnemies | (ifNoActiveEnemies & (CrowdControl.instance.activeEnemies == 0)))) ? 1 : 0, Time.deltaTime * 4f);
			cg.alpha = alpha;
		}
	}

	private void OnTriggerStay(Collider other)
	{
		isActivated = true;
	}

	private void FixedUpdate()
	{
		if (isActivated)
		{
			isActivated = false;
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.magenta;
		Gizmos.DrawSphere(base.transform.position, 0.5f);
	}
}
