using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ImageAnimator : MonoBehaviour
{
	public Sprite[] sprites;

	private Image image;

	private WaitForSeconds wait = new WaitForSeconds(0.08f);

	private void Awake()
	{
		image = GetComponent<Image>();
		StartCoroutine(Playing());
	}

	private IEnumerator Playing()
	{
		while (true)
		{
			int i = 0;
			while (i < sprites.Length)
			{
				image.sprite = sprites[i];
				yield return wait;
				int num = i + 1;
				i = num;
			}
		}
	}
}
