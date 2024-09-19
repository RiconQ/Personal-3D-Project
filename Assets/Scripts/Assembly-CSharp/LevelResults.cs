using System;

[Serializable]
public class LevelResults
{
	public string name;

	public int noDanage;

	public int noFalls;

	public int noMercy;

	public int rank;

	public int points;

	public int combo;

	public float time;

	public int secret;

	public int reached;

	public LevelResults(string name)
	{
		this.name = name;
	}
}
