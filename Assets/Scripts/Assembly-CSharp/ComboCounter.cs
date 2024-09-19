using UnityEngine;
using UnityEngine.UI;

public class ComboCounter : MonoBehaviour
{
	public float timer;

	private RectTransform t;

	private CanvasGroup cg;

	private Vector3 aScale = Vector3.one;

	private Vector3 bScale = new Vector3(0.75f, 0.75f, 0.75f);

	public GameObject[] allCombos;

	public int combo { get; private set; }

	public int maxCombo { get; private set; }

	[Button]
	public void CreateAllChilds()
	{
		if (allCombos.Length != 0)
		{
			for (int i = 0; i < allCombos.Length; i++)
			{
				Object.DestroyImmediate(allCombos[i]);
			}
		}
		allCombos = new GameObject[99];
		for (int j = 2; j <= 100; j++)
		{
			string text = $"X{j}";
			GameObject gameObject = new GameObject(text);
			gameObject.transform.SetParent(base.transform);
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.AddComponent<Text>().text = text;
			allCombos[j - 2] = gameObject;
		}
	}

	public void Setup()
	{
		t = GetComponent<RectTransform>();
		cg = GetComponent<CanvasGroup>();
		cg.alpha = 0f;
		int num2 = (combo = 0);
		maxCombo = num2;
		for (int i = 0; i < allCombos.Length; i++)
		{
			allCombos[i].SetActive(value: false);
		}
	}

	public void ResetCombo()
	{
		if (maxCombo < combo)
		{
			maxCombo = combo;
		}
		if (combo - 2 >= 0)
		{
			allCombos[combo - 2].SetActive(value: false);
		}
		combo = 0;
		timer = 0f;
		cg.alpha = 0f;
	}

	public void AddCombo()
	{
		combo++;
		cg.alpha = ((combo >= 2) ? 1 : 0);
		if (combo >= 2)
		{
			allCombos[combo - 2].SetActive(value: true);
			if (combo - 3 >= 0)
			{
				allCombos[combo - 3].SetActive(value: false);
			}
		}
		if (maxCombo < combo)
		{
			maxCombo = combo;
		}
		timer = 1f;
	}

	public void Tick()
	{
		if (timer != 0f)
		{
			timer = Mathf.MoveTowards(timer, 0f, Time.deltaTime * 0.1f);
			t.localScale = Vector3.Lerp(bScale, aScale, timer);
			cg.alpha = Mathf.Lerp(0f, (combo >= 2) ? 1 : 0, timer * 2f);
			if (timer == 0f)
			{
				ResetCombo();
			}
		}
	}
}
