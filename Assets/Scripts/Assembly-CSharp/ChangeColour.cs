using UnityEngine;

public class ChangeColour : MonoBehaviour
{
	public Camera cam;

	public float fadeSpeed = 0.01f;

	private Vector3 currentColour;

	private Color targetColor;

	private void Start()
	{
		currentColour = ColourToVector3(cam.backgroundColor);
		targetColor = RandomColour();
	}

	private void Update()
	{
		if (cam.backgroundColor != targetColor)
		{
			currentColour = Vector3.MoveTowards(currentColour, ColourToVector3(targetColor), fadeSpeed * Time.deltaTime);
			cam.backgroundColor = Vector3ToColour(currentColour);
		}
		else
		{
			targetColor = RandomColour();
		}
	}

	private Vector3 ColourToVector3(Color colour)
	{
		return new Vector3(colour.r, colour.g, colour.b);
	}

	private Color Vector3ToColour(Vector3 vector3)
	{
		return new Color(vector3.x, vector3.y, vector3.z);
	}

	private Color RandomColour()
	{
		float maxInclusive = 0.5f;
		return new Color(Random.Range(0f, maxInclusive), Random.Range(0f, maxInclusive), Random.Range(0f, maxInclusive));
	}
}
