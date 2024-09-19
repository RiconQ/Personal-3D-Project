using System;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class QuickUI : MonoBehaviour
{
	public static QuickUI lastActive;

	public QuickUI prevUI;

	[Header("Audio")]
	public AudioSource source;

	public QuickMenuSounds sounds;

	private bool activating;

	public RectTransform t { get; private set; }

	public CanvasGroup cg { get; private set; }

	public bool active { get; private set; }

	public virtual void Awake()
	{
		source = GetComponentInParent<AudioSource>();
		t = GetComponent<RectTransform>();
		cg = GetComponent<CanvasGroup>();
		if (cg.alpha == 0f)
		{
			Deactivate();
		}
		else
		{
			active = true;
			Activate();
		}
		InputsManager.OnBack = (Action)Delegate.Combine(InputsManager.OnBack, new Action(Back));
	}

	public virtual void OnDisable()
	{
		if (cg.interactable)
		{
			Deactivate();
		}
		InputsManager.OnBack = (Action)Delegate.Remove(InputsManager.OnBack, new Action(Back));
	}

	public virtual void Activate()
	{
		lastActive = this;
		cg.interactable = true;
		cg.blocksRaycasts = true;
		activating = true;
	}

	public virtual void Deactivate()
	{
		cg.alpha = 0f;
		cg.interactable = false;
		cg.blocksRaycasts = false;
		if (active)
		{
			active = false;
		}
		activating = false;
	}

	public virtual void Back()
	{
		if ((bool)prevUI && active)
		{
			prevUI.Activate();
			OnBack();
			Deactivate();
		}
	}

	public virtual void OnBack()
	{
		if (active && (bool)sounds && (bool)source)
		{
			Game.sounds.PlaySound(sounds.back, 2);
		}
	}

	protected virtual void Update()
	{
		if (activating && cg.alpha != 1f)
		{
			cg.alpha = Mathf.MoveTowards(cg.alpha, 1f, Time.unscaledDeltaTime * 2f);
			if (cg.alpha > 0.1f && !active)
			{
				active = true;
			}
		}
	}
}
