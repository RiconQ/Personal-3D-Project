using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
	public Image strengh;

	public Image stamina;

	public AudioClip sfxCharged;

	private PlayerController player;

	private CanvasGroup cg;

	private bool charged;

	public GameObject skullEyes;

	public Transform tSkull;

	private MeshFilter filter;

	private MeshRenderer rend;

	private MaterialPropertyBlock block;

	private void Start()
	{
		player = Game.player;
		cg = GetComponent<CanvasGroup>();
		filter = tSkull.GetComponentInChildren<MeshFilter>();
		rend = tSkull.GetComponentInChildren<MeshRenderer>();
		block = new MaterialPropertyBlock();
		rend.GetPropertyBlock(block);
	}

	private void Update()
	{
		cg.alpha = (Game.paused ? 0f : (1f - Game.wideMode.cg.alpha));
		if (player.weapons.isActiveAndEnabled)
		{
			strengh.fillAmount = player.weapons.Holding();
			stamina.fillAmount = player.weapons.kickController.timer;
		}
		else
		{
			strengh.fillAmount = 0f;
			stamina.fillAmount = 0f;
		}
		block.SetFloat("_Fill", -0.15f + player.weapons.kickController.timer * 0.5f);
		block.SetFloat("_Blink", player.weapons.Holding());
		rend.SetPropertyBlock(block);
		if (strengh.fillAmount == 1f)
		{
			if (!charged)
			{
				charged = true;
				Game.sounds.PlayClip(sfxCharged);
			}
		}
		else
		{
			charged = false;
		}
	}
}
