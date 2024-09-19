using UnityEngine;

[CreateAssetMenu(fileName = "Keyboard Inputs", menuName = "New Keyboard Inputs", order = 1)]
public class KeyboardInputs : ScriptableObject
{
	public PlayerKey[] playerKeys;

	public void Reset()
	{
		playerKeys[0].key = KeyCode.W;
		playerKeys[1].key = KeyCode.A;
		playerKeys[2].key = KeyCode.S;
		playerKeys[3].key = KeyCode.D;
		playerKeys[4].key = KeyCode.E;
		playerKeys[5].key = KeyCode.LeftControl;
		playerKeys[6].key = KeyCode.Space;
		playerKeys[7].key = KeyCode.T;
		playerKeys[8].key = KeyCode.Mouse0;
		playerKeys[9].key = KeyCode.Mouse1;
		playerKeys[10].key = KeyCode.LeftShift;
		playerKeys[11].key = KeyCode.Z;
	}
}
