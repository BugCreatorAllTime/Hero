using UnityEngine;
using UnityEditor;

public class AllAssetPostProcessor : AssetPostprocessor
{
	void OnPreprocessTexture()
	{
		Object asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Texture2D));
		if (asset)
			return; //set default values only for new textures;
		
		TextureImporter importer = assetImporter as TextureImporter;
		//set your default settings to the importer here
		importer.SetPlatformTextureSettings ("iPhone",1024,TextureImporterFormat.ARGB16);
		importer.SetPlatformTextureSettings ("Android",1024,TextureImporterFormat.ARGB16);
	}

	void OnPreprocessAudio()
	{
		Object asset = AssetDatabase.LoadAssetAtPath(assetPath, typeof(AudioClip));
		if (asset)
			return; //set default values only for new textures;
		
		AudioImporter importer = assetImporter as AudioImporter;
		importer.compressionBitrate = 32000;
		importer.loadType = AudioImporterLoadType.CompressedInMemory;
		importer.threeD = false;
		importer.hardware = true;

		Debug.Log(assetPath + " - " + importer.compressionBitrate + " bits/second");
	}
}