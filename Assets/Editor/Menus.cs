/******************************************************************************
 * Spine Runtimes Software License
 * Version 2.1
 * 
 * Copyright (c) 2013, Esoteric Software
 * All rights reserved.
 * 
 * You are granted a perpetual, non-exclusive, non-sublicensable and
 * non-transferable license to install, execute and perform the Spine Runtimes
 * Software (the "Software") solely for internal use. Without the written
 * permission of Esoteric Software (typically granted by licensing Spine), you
 * may not (a) modify, translate, adapt or otherwise create derivative works,
 * improvements of the Software or develop new applications using the Software
 * or (b) remove, delete, alter or obscure any trademarks or any copyright,
 * trademark, patent or other intellectual property or proprietary rights
 * notices on or in the Software, including any copy thereof. Redistributions
 * in binary or source form must include this license and terms.
 * 
 * THIS SOFTWARE IS PROVIDED BY ESOTERIC SOFTWARE "AS IS" AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
 * EVENT SHALL ESOTERIC SOFTARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS;
 * OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 *****************************************************************************/

using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using Spine;

public class Menus {
	[MenuItem("Assets/Create/Spine Atlas")]
	static public void CreateAtlas () {
		CreateAsset<AtlasAsset>("New Atlas");
	}
	
	[MenuItem("Assets/Create/Spine SkeletonData")]
	static public void CreateSkeletonData () {
		CreateAsset<SkeletonDataAsset>("New SkeletonData");
	}
	
	static private T CreateAsset <T> (String name) where T : ScriptableObject {
		var dir = "Assets/";
		var selected = Selection.activeObject;
		if (selected != null) {
			var assetDir = AssetDatabase.GetAssetPath(selected.GetInstanceID());
			if (assetDir.Length > 0 && Directory.Exists(assetDir)) dir = assetDir + "/";
		}
		ScriptableObject asset = ScriptableObject.CreateInstance<T>();

		AssetDatabase.CreateAsset(asset, dir + name + ".asset");
		AssetDatabase.SaveAssets();
		EditorUtility.FocusProjectWindow();
		return asset as T;
	}

	[MenuItem("GameObject/Create Other/Spine SkeletonRenderer")]
	static public void CreateSkeletonRendererGameObject () {
		GameObject gameObject = new GameObject("New SkeletonRenderer", typeof(SkeletonRenderer));
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = gameObject;
	}

	[MenuItem("GameObject/Create Other/Spine SkeletonAnimation")]
	static public void CreateSkeletonAnimationGameObject () {
		GameObject gameObject = new GameObject("New SkeletonAnimation", typeof(SkeletonAnimation));
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = gameObject;
	}

	[MenuItem("Assets/CreateSpineData")]
	static public void CreateSpineData() {
		UnityEngine.Object selected = Selection.activeObject;
		string skeStr = "skeleton";
		string pngExt = ".png";
		string atlasExt = ".atlas.txt";
		string jsonExt = ".json.txt";
		string path = "/";
		if(selected != null)
		{
			string assetDir = AssetDatabase.GetAssetPath(selected.GetInstanceID());
			string[] arr = assetDir.Split('/');
			string name = arr[arr.Length - 1];
			int count = 0;
			string sDataPath  = Application.dataPath;
			string sFolderPath = sDataPath.Substring(0 ,sDataPath.Length-6)+assetDir;           
			string[] aFilePaths = Directory.GetFiles(sFolderPath);
			foreach (string sFilePath in aFilePaths) {
				string sAssetPath = sFilePath.Substring(sDataPath.Length-6);
				if(sAssetPath.IndexOf(pngExt) > -1 && sAssetPath.IndexOf(".meta") == -1){
					++count;
				}
			}

			Material[] materials = new Material[count];
			for(int i = 0; i< count; ++i)
			{
				materials[i] = new Material (Shader.Find("Spine/Skeleton"));
				string realName = name + (i==0?"": (i+1).ToString());
				string realSkeName = skeStr + (i==0?"": (i+1).ToString());
				AssetDatabase.CreateAsset(materials[i], assetDir + path + realName + ".mat");
				materials[i].mainTexture = AssetDatabase.LoadAssetAtPath(assetDir + path + realSkeName + pngExt, typeof(Texture)) as Texture;
			}
			AssetDatabase.SaveAssets();
			AtlasAsset atlasAsset = AtlasAsset.CreateInstance<AtlasAsset>();
			atlasAsset.atlasFile = AssetDatabase.LoadAssetAtPath(assetDir + path + skeStr + atlasExt, typeof(TextAsset)) as TextAsset;
			atlasAsset.materials = new Material[count];
			atlasAsset.materials = materials;
			AssetDatabase.CreateAsset(atlasAsset, assetDir + path + name + ".atlas.asset");
			AssetDatabase.SaveAssets();

			SkeletonDataAsset dataAsset = SkeletonDataAsset.CreateInstance<SkeletonDataAsset>();
			dataAsset.skeletonJSON = AssetDatabase.LoadAssetAtPath(assetDir + path + skeStr + jsonExt,typeof(TextAsset)) as TextAsset;
			dataAsset.atlasAsset= atlasAsset;
			AssetDatabase.CreateAsset(dataAsset, assetDir + path + name + ".ske.asset");
			AssetDatabase.SaveAssets();
			Debug.Log("create spine: " + name);
		}

	}
}
