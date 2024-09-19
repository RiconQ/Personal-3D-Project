using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(Text))]
public class TextAnimator : BaseMeshEffect
{
	public bool playOnEnable;

	public bool looped;

	public bool unscaledTime;

	public TextAnimationPreset preset;

	private int charsCount;

	private Vector3 tempCharCenter;

	private UIVertex uiVertex;

	private List<int> activeAnimations = new List<int>(10);

	private List<int> lines = new List<int>(20);

	private List<int> words = new List<int>(100);

	private CharAnimation[] chars = new CharAnimation[0];

	private CharAnimation activeChar = new CharAnimation();

	private int charIndex;

	private int lineIndex;

	private int wordIndex;

	private int nextLineStartsAt;

	private int nextWordStartsAt;

	public Text text { get; private set; }

	public RectTransform t { get; private set; }

	public bool isPlaying { get; private set; }

	protected override void Awake()
	{
		base.Awake();
		t = GetComponent<RectTransform>();
		text = GetComponent<Text>();
		ResetChars();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (playOnEnable)
		{
			Play();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		isPlaying = false;
	}

	public void ResetChars(string newText)
	{
		text.text = newText;
		ResetChars();
	}

	public void ResetChars()
	{
		lines.Clear();
		words.Clear();
		charsCount = text.text.Length;
		chars = new CharAnimation[charsCount];
		int num = 0;
		for (int i = 0; i < text.text.Length; i++)
		{
			chars[i] = new CharAnimation();
			if (text.text[i] == ' ')
			{
				charsCount--;
				num++;
				words.Add(i - num + 1);
			}
			else if (char.IsControl(text.text[i]))
			{
				charsCount--;
				num++;
				words.Add(i - num + 1);
				lines.Add(i - num + 1);
			}
		}
	}

	public bool LastCharReached()
	{
		if (words.Count == 0)
		{
			return true;
		}
		return chars[words[words.Count - 1]].progress > 0f;
	}

	public void ResetAndPlay(string newText)
	{
		text.text = newText;
		ResetAndPlay();
	}

	public void ResetAndPlay()
	{
		ResetChars();
		Play();
	}

	public void Play()
	{
		isPlaying = true;
		activeAnimations.Clear();
		for (int i = 0; i < preset.animations.Count; i++)
		{
			activeAnimations.Add(i);
		}
		CharAnimation[] array = chars;
		for (int j = 0; j < array.Length; j++)
		{
			array[j].Reset();
		}
	}

	public void StopAt(float value = 0f)
	{
		isPlaying = false;
		activeAnimations.Clear();
		CharAnimation[] array = chars;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Reset(value);
		}
		base.graphic.SetVerticesDirty();
	}

	private void Update()
	{
		if (isPlaying)
		{
			base.graphic.SetVerticesDirty();
		}
	}

	public override void ModifyMesh(VertexHelper vh)
	{
		if (!IsActive() || !Application.isPlaying || !preset)
		{
			return;
		}
		lineIndex = (wordIndex = 0);
		nextLineStartsAt = ((lines.Count > 0) ? lines[lineIndex] : (-1));
		nextWordStartsAt = ((words.Count > 0) ? words[wordIndex] : (-1));
		for (int i = 0; i < vh.currentVertCount; i += 4)
		{
			charIndex = i / 4;
			if (charIndex >= chars.Length)
			{
				continue;
			}
			tempCharCenter.x = (tempCharCenter.y = (tempCharCenter.z = 0f));
			vh.PopulateUIVertex(ref uiVertex, i);
			tempCharCenter += uiVertex.position;
			vh.PopulateUIVertex(ref uiVertex, i + 2);
			tempCharCenter += uiVertex.position;
			tempCharCenter /= 2f;
			if (preset.perWordDelay > 0f && charIndex == nextWordStartsAt)
			{
				wordIndex++;
				if (wordIndex < words.Count)
				{
					nextWordStartsAt = words[wordIndex];
				}
			}
			if (preset.perLineDelay > 0f && charIndex == nextLineStartsAt)
			{
				lineIndex++;
				if (lineIndex < lines.Count)
				{
					nextLineStartsAt = lines[lineIndex];
				}
			}
			activeChar = chars[charIndex];
			if (activeAnimations.Count > 0 && isPlaying)
			{
				activeChar.time += (unscaledTime ? Time.unscaledDeltaTime : Time.deltaTime) * preset.speed;
				activeChar.progress = Mathf.Clamp01(activeChar.time - (float)charIndex * preset.perCharDelay - (float)lineIndex * preset.perLineDelay - (float)wordIndex * preset.perWordDelay);
			}
			for (int j = 0; j < 4; j++)
			{
				vh.PopulateUIVertex(ref uiVertex, j + i);
				for (int k = 0; k < preset.animations.Count; k++)
				{
					preset.animations[k].UpdateAnimation(ref uiVertex, ref activeChar, tempCharCenter);
				}
				vh.SetUIVertex(uiVertex, j + i);
			}
			if (chars[charsCount - 1].progress == 1f)
			{
				for (int num = activeAnimations.Count - 1; num >= 0; num--)
				{
					activeAnimations.RemoveAt(num);
				}
			}
			if (activeAnimations.Count != 0)
			{
				continue;
			}
			if (!looped)
			{
				isPlaying = false;
				continue;
			}
			for (int l = 0; l < preset.animations.Count; l++)
			{
				activeAnimations.Add(l);
			}
			CharAnimation[] array = chars;
			for (int m = 0; m < array.Length; m++)
			{
				array[m].Reset();
			}
		}
	}
}
