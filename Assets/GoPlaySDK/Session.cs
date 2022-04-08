using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using GoPlaySDK;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GoPlaySDK
{

    [Serializable]
    public class GTGameID
    {

        private GTGameID()
        {
        }

        public void OnGUI()
        {
#if UNITY_EDITOR
            GameID = EditorGUILayout.TextField("Game ID", GameID);
#endif
        }


        public static GTGameID LoadSetting()
        {
            GTGameID data = null;
			TextAsset binarydata = Resources.Load<TextAsset>("GoPlaySDKSetting");
            if (binarydata == null)
            {
                // setting is not set
                return new GTGameID();
            }

            try
            {
                Stream ms = new MemoryStream(binarydata.bytes);
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    data = (GTGameID)formatter.Deserialize(ms);
                }
                catch (SerializationException e)
                {
                    Debug.LogError(e.Message);
                }
                finally
                {
                    ms.Close();
                }
            }
            catch (FileNotFoundException e)
            {
                Debug.Log("Load Gtoken Setting: " + e.Message);
                data = new GTGameID();
            }
            return data;
        }

        // Game ID
        [SerializeField]
        public string GameID;
    }

	public class Session 
	{
		//class Game Id
		private GTGameID gameId;
		//GameID guid type
		public string GameId { get {return gameId.GameID;}}	
		// Whether the user already loginned
		public bool HasLoggedIn { get { return !string.IsNullOrEmpty(SessionId); } }
		// The current loginned user
		public UserProfile CurrentUser { get; set; }
		public string SessionId { get; set; }
		// store cache key for session
		private readonly string GOPLAY_Session = Constants.GOPLAY_SESSION;

		// Start Session
		public Session()
		{
			gameId = GTGameID.LoadSetting();			
			// Look for cache session
			SessionId = PlayerPrefs.GetString(GOPLAY_Session, string.Empty);
		}
	}
}
