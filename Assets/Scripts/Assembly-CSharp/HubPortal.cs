using System.Collections;
using UnityEngine;

[SelectionBase]
public class HubPortal : MonoBehaviour
{
	public SceneData data;

	[HideInInspector]
	public HubPortalUI ui;

	[HideInInspector]
	public bool isLocked;

	[HideInInspector]
	public bool isOpened;

	[HideInInspector]
	public bool isFocused;

	public bool quickLoad;

	public Transform t { get; private set; }

	public Transform tMesh { get; private set; }

	public virtual void Setup()
	{
		t = base.transform;
		tMesh = t.Find("Mesh");
	}

	public virtual void Check()
	{
	}

	public virtual void OnTriggerEnter(Collider other)
	{
		if (Game.player.inputActive && !isLocked)
		{
			Hub.lastPortal = this;
			Game.fading.InstantFade(1f);
			if (GetType() == typeof(HubPortalToHub))
			{
				StartCoroutine(FXBeforeLoading());
			}
			else
			{
				Game.instance.LoadLevel(data.sceneReference.ScenePath, quickLoad);
			}
		}
	}

	private IEnumerator FXBeforeLoading()
	{
		Game.player.SetKinematic(value: true);
		Game.player.Deactivate();
		Game.player.t.position = BlackBox.instance.tPlayerPosition.position - Game.player.tHead.localPosition;
		Game.player.mouseLook.SetRotation(BlackBox.instance.tPlayerPosition.rotation);
		Game.player.weapons.gameObject.SetActive(value: false);
		yield return new WaitForEndOfFrame();
		Game.fading.InstantFade(0f);
		CameraController.shake.Shake(2);
		Toggle(value: false);
		BlackBox.instance.gameObject.SetActive(value: true);
		BlackBox.instance.source.Play();
		BlackBox.instance.particle.Play();
		while (BlackBox.instance.particle.isPlaying)
		{
			yield return null;
		}
		Game.instance.LoadLevel(data.sceneReference.ScenePath);
	}

	public virtual void Toggle(bool value)
	{
		if ((bool)ui)
		{
			if (value)
			{
				ui.Show();
			}
			else
			{
				ui.Hide();
			}
		}
		if (isFocused != value)
		{
			isFocused = value;
		}
	}

	public virtual void SpawnPlayer()
	{
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawRay(base.transform.position, base.transform.forward * 4f);
	}
}
