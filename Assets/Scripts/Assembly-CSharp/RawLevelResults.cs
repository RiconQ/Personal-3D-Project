using System;

[Serializable]
public class RawLevelResults
{
	public bool noDamage;

	public bool noFalls;

	public bool noMercy;

	public bool secret;

	public int points;

	public int combo;

	public int moves;

	public int rank;

	public bool pbNoDamage;

	public bool pbNoFalls;

	public bool pbPoints;

	public bool pbCombo;

	public bool pbMoves;

	public bool pbRank;

	public bool pbTime;

	public float time;

	public float groundedTime;

	public float airTime;

	public float slideTime;

	public float parkourTime;

	public int slidePercent => (int)(slideTime / groundedTime * 100f);

	public int parkourPercent => (int)(parkourTime / airTime * 100f);

	public void Clear()
	{
		noDamage = true;
		noFalls = true;
		noMercy = false;
		secret = false;
		points = (combo = (moves = (rank = 0)));
		time = (groundedTime = (airTime = (slideTime = (parkourTime = 0f))));
		pbNoDamage = (pbNoFalls = (pbPoints = (pbCombo = (pbMoves = (pbRank = (pbTime = false))))));
	}
}
