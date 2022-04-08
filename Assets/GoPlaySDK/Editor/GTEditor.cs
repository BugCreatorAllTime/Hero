using UnityEngine;
using UnityEditor;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

namespace GoPlaySDK
{
	public class GTEditor : EditorWindow {

		public static readonly string GOPLAYSDK_SETTING_DIR = "/GoPlaySDK/Resources/GoPlaySDKSetting.txt";
		
		private static GTGameID data = null;

		[MenuItem("GoPlaySDK/Setting")]
		static void Init()
		{
			GetWindowWithRect<GTEditor>(new Rect(0, 0, 500, 200));
		}
		
		void OnEnable()
		{
			hideFlags = HideFlags.HideAndDontSave;
			if (data == null)
			{
				data = GTGameID.LoadSetting();
			}
		}
		
		void OnGUI()
		{
			if (data != null)
			{
				GUILayout.Label("GoPlaySDK Settings", EditorStyles.boldLabel);
				data.OnGUI();
				
				if (GUILayout.Button("Save"))
				{
					SaveSetting();
				}
			}
		}
		
		void SaveSetting()
		{
			string path = Application.dataPath + GOPLAYSDK_SETTING_DIR;
			using (FileStream fs = new FileStream(path, FileMode.Create))
			{
				BinaryFormatter formatter = new BinaryFormatter();
				try
				{
					formatter.Serialize(fs, data);
				}
				catch (SerializationException e)
				{
					Debug.LogError(e.Message);
				}
				fs.Close();
			}
			AssetDatabase.Refresh();
		}
}
}