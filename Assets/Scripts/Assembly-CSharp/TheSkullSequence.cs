using UnityEngine;

[CreateAssetMenu(fileName = "TheSkullSequence", menuName = "Data/The Skull Sequence", order = 1)]
public class TheSkullSequence : ScriptableObject
{
	public enum SequenceTiming
	{
		BeforeScene = 0,
		AfterScene = 1
	}

	public SequenceTiming moment;

	public SceneData loadAtScene;

	public SkullSpeech[] lines;
}
