using System.IO;
using UnityEngine;

public class Quickmap
{
	public static string customMapName = "";

	public static string pathMyMaps = "";

	public static string pathMyDownloads = "";

	public static int previewPictureWidth = 256;

	public static int previewPictureHeight = 256;

	public static string PreviewPictureName(string name)
	{
		return $"{GetPathToMyLevels()}/{name}-preview.png";
	}

	public static string GetPathToMyLevels()
	{
		if (pathMyMaps.Length == 0)
		{
			pathMyMaps = $"{Application.persistentDataPath}/Quickmap/MyLevels";
			if (!Directory.Exists(pathMyMaps))
			{
				Directory.CreateDirectory(pathMyMaps);
			}
		}
		return pathMyMaps;
	}

	public static bool DeleteQuickmap()
	{
		bool result = false;
		string path = $"{GetPathToMyLevels()}/{customMapName}.quickmap";
		string path2 = $"{GetPathToMyLevels()}/{customMapName}-preview.png";
		if (File.Exists(path))
		{
			File.Delete(path);
			result = true;
		}
		if (File.Exists(path2))
		{
			File.Delete(path2);
			result = true;
		}
		return result;
	}

	public static void TakePreviewOLD(Camera cam)
	{
		RenderTexture active = (cam.targetTexture = new RenderTexture(256, 256, 24));
		cam.clearFlags = CameraClearFlags.Skybox;
		cam.Render();
		RenderTexture.active = active;
		Texture2D texture2D = new Texture2D(256, 256, TextureFormat.RGB24, mipChain: false);
		texture2D.ReadPixels(new Rect(0f, 0f, 256f, 256f), 0, 0);
		texture2D.Apply();
		byte[] bytes = texture2D.EncodeToPNG();
		File.WriteAllBytes(PreviewPictureName(customMapName), bytes);
		cam.targetTexture = null;
		cam.clearFlags = CameraClearFlags.Depth;
		RenderTexture.active = null;
	}

	public static void TakePreview(Camera cam)
	{
		previewPictureHeight = Screen.height;
		previewPictureWidth = Screen.width;
		RenderTexture active = (cam.targetTexture = new RenderTexture(previewPictureWidth, previewPictureHeight, 24));
		cam.clearFlags = CameraClearFlags.Skybox;
		cam.Render();
		RenderTexture.active = active;
		Texture2D texture2D = new Texture2D(previewPictureHeight, previewPictureHeight, TextureFormat.RGB24, mipChain: false);
		texture2D.ReadPixels(new Rect((previewPictureWidth - previewPictureHeight) / 2, 0f, previewPictureWidth, previewPictureHeight), 0, 0);
		texture2D.Apply();
		cam.targetTexture = null;
		cam.clearFlags = CameraClearFlags.Depth;
		RenderTexture.active = null;
		byte[] bytes = texture2D.EncodeToPNG();
		File.WriteAllBytes(PreviewPictureName(customMapName), bytes);
	}

	public static void SaveQuickmap(QuickmapScene scene)
	{
		using (BinaryWriter binaryWriter = new BinaryWriter(File.Open($"{GetPathToMyLevels()}/{customMapName}.quickmap", FileMode.Create)))
		{
			if ((bool)scene.megaCubeWorld)
			{
				binaryWriter.Write(scene.megaCubeWorld.regions.Count);
				foreach (MegaCubeRegion region in scene.megaCubeWorld.regions)
				{
					binaryWriter.Write(region.origin.x);
					binaryWriter.Write(region.origin.y);
					binaryWriter.Write(region.origin.z);
					binaryWriter.Write(region.points.Count);
					foreach (Vector3Int point in region.points)
					{
						binaryWriter.Write(point.x);
						binaryWriter.Write(point.y);
						binaryWriter.Write(point.z);
					}
				}
			}
			binaryWriter.Write(scene.editorCamera.t.position.x);
			binaryWriter.Write(scene.editorCamera.t.position.y);
			binaryWriter.Write(scene.editorCamera.t.position.z);
			binaryWriter.Write(scene.editorCamera.t.rotation.x);
			binaryWriter.Write(scene.editorCamera.t.rotation.y);
			binaryWriter.Write(scene.editorCamera.t.rotation.z);
			binaryWriter.Write(scene.editorCamera.t.rotation.w);
			if (scene.objectsToSave.Count > 0)
			{
				Vector3 vector = default(Vector3);
				binaryWriter.Write(scene.objectsToSave.Count);
				for (int i = 0; i < scene.objectsToSave.Count; i++)
				{
					scene.objectsToSave[i].GetComponent<QuickmapObject>();
					binaryWriter.Write(scene.objectsToSave[i].name);
					binaryWriter.Write(scene.objectsToSave[i].transform.position.x);
					binaryWriter.Write(scene.objectsToSave[i].transform.position.y);
					binaryWriter.Write(scene.objectsToSave[i].transform.position.z);
					binaryWriter.Write(scene.objectsToSave[i].transform.rotation.x);
					binaryWriter.Write(scene.objectsToSave[i].transform.rotation.y);
					binaryWriter.Write(scene.objectsToSave[i].transform.rotation.z);
					binaryWriter.Write(scene.objectsToSave[i].transform.rotation.w);
					SetupableMonobehavior component = scene.objectsToSave[i].GetComponent<SetupableMonobehavior>();
					if ((bool)component)
					{
						vector = component.GetWorldTargetPosition();
					}
					binaryWriter.Write(vector.x);
					binaryWriter.Write(vector.y);
					binaryWriter.Write(vector.z);
				}
			}
			else
			{
				binaryWriter.Write(0);
			}
		}
		TakePreview(scene.editorCamera.cam);
	}

	public static void LoadQuickmap(QuickmapScene scene)
	{
		string path = ((pathMyDownloads.Length > 0) ? pathMyDownloads : $"{GetPathToMyLevels()}/{customMapName}.quickmap");
		if (!File.Exists(path))
		{
			return;
		}
		using (BinaryReader binaryReader = new BinaryReader(File.Open(path, FileMode.Open)))
		{
			if ((bool)scene.megaCubeWorld)
			{
				scene.megaCubeWorld.ClearAll();
				int num = binaryReader.ReadInt32();
				for (int i = 0; i < num; i++)
				{
					Vector3Int vector3Int = default(Vector3Int);
					vector3Int.x = binaryReader.ReadInt32();
					vector3Int.y = binaryReader.ReadInt32();
					vector3Int.z = binaryReader.ReadInt32();
					scene.megaCubeWorld.origin = vector3Int;
					scene.megaCubeWorld.AddNewRegion();
					int num2 = binaryReader.ReadInt32();
					for (int j = 0; j < num2; j++)
					{
						vector3Int.x = binaryReader.ReadInt32();
						vector3Int.y = binaryReader.ReadInt32();
						vector3Int.z = binaryReader.ReadInt32();
						scene.megaCubeWorld.AddToRegion(vector3Int);
					}
				}
				scene.megaCubeWorld.CreateMeshRuntime();
			}
			Vector3 position = default(Vector3);
			position.x = binaryReader.ReadSingle();
			position.y = binaryReader.ReadSingle();
			position.z = binaryReader.ReadSingle();
			Quaternion rotation = default(Quaternion);
			rotation.x = binaryReader.ReadSingle();
			rotation.y = binaryReader.ReadSingle();
			rotation.z = binaryReader.ReadSingle();
			rotation.w = binaryReader.ReadSingle();
			scene.editorCamera.t.position = position;
			scene.editorCamera.mouseLook.SetRotation(rotation);
			int num3 = binaryReader.ReadInt32();
			Vector3 pos = default(Vector3);
			Vector3 target = new Vector3(0f, 0f, 0f);
			Quaternion rot = default(Quaternion);
			for (int k = 0; k < num3; k++)
			{
				string name = binaryReader.ReadString();
				pos.x = binaryReader.ReadSingle();
				pos.y = binaryReader.ReadSingle();
				pos.z = binaryReader.ReadSingle();
				rot.x = binaryReader.ReadSingle();
				rot.y = binaryReader.ReadSingle();
				rot.z = binaryReader.ReadSingle();
				rot.w = binaryReader.ReadSingle();
				target.x = binaryReader.ReadSingle();
				target.y = binaryReader.ReadSingle();
				target.z = binaryReader.ReadSingle();
				scene.InstantiatePrefab(name, pos, rot, target);
			}
		}
		pathMyDownloads = "";
	}
}
