using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ConsoleUI : MonoBehaviour
{
	private CanvasGroup canvasGroup;

	private bool showing;

	private InputField field;

	private void Awake()
	{
		canvasGroup = GetComponent<CanvasGroup>();
		canvasGroup.alpha = 0f;
		field = GetComponent<InputField>();
		field.interactable = false;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.BackQuote))
		{
			Show();
		}
	}

	public void Show()
	{
		showing = !showing;
		if (showing)
		{
			field.interactable = true;
			field.text = "";
			Game.instance.Pause();
			EventSystem.current.SetSelectedGameObject(base.gameObject, null);
			GetComponent<Animation>().Play();
		}
		else
		{
			GetComponent<Animation>().Stop();
			canvasGroup.alpha = 0f;
			field.interactable = false;
			Game.instance.Unpause();
		}
	}

	public void DebugEnteredCode(string code)
	{
		string[] array = code.Split(' ');
		field.text = null;
		if (array.Length > 1)
		{
			if (array[0] == "map")
			{
				Game.instance.LoadLevel(array[1]);
				Show();
			}
			else if (array[0] == "spawn")
			{
				QuickPool.instance.Get(array[1], Camera.main.transform.position + Camera.main.transform.forward * 2f);
			}
		}
	}
}
