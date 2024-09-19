using Steamworks;
using UnityEngine;

public class MySteam : MonoBehaviour
{
	private void Start()
	{
		if (SteamManager.Initialized)
		{
			Debug.Log(SteamFriends.GetPersonaName());
		}
	}
}
