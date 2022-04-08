
using UnityEngine;
using System;
using System.Collections;
using GoPlaySDK;

namespace GoPlaySDK
{
	public sealed class GoPlaySdk : Singleton<GoPlaySdk>
    {
		private GoPlaySdk() 
        {
            UserSession = new Session();
        }
        public bool UseLiveServer { get; set; }
		public Session UserSession { get; set; }

        #region Events 
        public delegate void ResponseHandler(IResult r);
        // Events //
        public event ResponseHandler OnLogin;
		public event ResponseHandler OnLogOut;
        public event ResponseHandler OnRegister;
        public event ResponseHandler OnGetProfile;
        public event ResponseHandler OnEditProfile;
        public event ResponseHandler OnGetProgress;
        public event ResponseHandler OnSaveProgress;
        public event ResponseHandler OnUpdateGameStats;
        public event ResponseHandler OnGetUnFullFilledExchanges;
        public event ResponseHandler OnFullFillExchange;
		public event ResponseHandler OnUpdateExternalExchange;
		public event ResponseHandler OnRejectExchange;

        #endregion

        #region Supported Operations

        #region Register
        public void Register(string userName,
                             string password,	
		                     string email = null,
                             string nickName = null,
                             Gender gender = Gender.Other,
                             string referal = null)
        {
            if (OnRegister == null)
                return;

            if(UserSession.HasLoggedIn )
			{   // Already logged in //
                var result = new RegisterResult { ErrorCode = Error.UserAlreadyLoggedIn, Succeeded = false };
				OnRegister(result);
			}
            else 
            {
				StartCoroutine(RegisterAsync(userName, password,  email, nickName, gender, referal));
            }
        }


        private IEnumerator RegisterAsync(string userName,
                                            string password,		                                    
                                            string email = null,
                                            string nickName = null,
                                            Gender gender = Gender.Other,
                                            string referal = null)
        {
			WWWForm f = new WWWForm();
            f.AddFieldLowerCase(Constants.FIELD_USERNAME, userName);
            f.AddField(Constants.FIELD_PASSWORD, password);
            f.AddField(Constants.FIELD_GAME_ID, UserSession.GameId);
            f.AddField(Constants.FIELD_GENDER, gender.ToString());
            f.AddFieldIfValid(Constants.FIELD_EMAIL, email);
            f.AddFieldIfValid(Constants.FIELD_NICKNAME, nickName);
            f.AddFieldIfValid(Constants.FIELD_REFERRAL_CODE, referal);

            string url = URLs.ACTION_REGISTER.ToURL(UseLiveServer);
            WWW www = new WWW(url, f);
            yield return www;


            // Compose the Result //
            var result = WWWParser.Parse<RegisterResult>(www);

            // Need to connect this user with Social platform ? //
            if (result.Succeeded && this.RequireOAuthData != null)
            {			
                // Perform Oauth-Connect //
				yield return StartCoroutine(BindOauthAsync(result.Session, RequireOAuthData));
                result.CopyFrom(RequireOAuthData);
                this.RequireOAuthData = null; // Reset it //
            }

            // Trigger Event //
            OnRegister(result);
        }
        #endregion

        #region Login
		public void Login(string userName, string password)
        {
            if (OnLogin != null && !UserSession.HasLoggedIn)
            {
				StartCoroutine(LoginAsync(userName, password));
			}
		}

		private IEnumerator LoginAsync(string userName, string password)
        {

            WWWForm f = new WWWForm();
            f.AddFieldLowerCase(Constants.FIELD_USERNAME, userName); 
            f.AddField(Constants.FIELD_PASSWORD, password);
			f.AddField(Constants.FIELD_GAME_ID,UserSession.GameId);

            string url = URLs.ACTION_LOGIN.ToURL(UseLiveServer);
			WWW www = new WWW(url, f);
			yield return www;

            // Compose the Result //
            var result = WWWParser.Parse<LoginResult>(www);
            // Need to connect this user with Social platform ? //
			if (result.Succeeded && RequireOAuthData != null)
			{

				// Perform Oauth-Connect //
				yield return StartCoroutine(BindOauthAsync(result.Session,RequireOAuthData));
				result.CopyFrom(RequireOAuthData);
				this.RequireOAuthData = null; // Reset it //
			}
			// check again for keep store session
			if(result.Succeeded){
				UserSession.SessionId = result.Session;
				UserSession.CurrentUser = result.Profile;
				//Cache session
				PlayerPrefs.SetString(Constants.GOPLAY_SESSION, result.Session);
			}
			// Trigger Event //
			OnLogin(result);
            // After Login, get any UnfullfilledExchanges //
            if (result.Succeeded && OnGetUnFullFilledExchanges != null)
                StartCoroutine(GetUnFullFilledExchangesAsync()); 


        }
        #endregion

        #region Login with third party such as FaceBook
        public void Login(SocialPlatforms platform, string token)
        {
            if (OnLogin != null)
            {
				StartCoroutine(LoginAsync(platform, token));
			}
		}

		private IEnumerator LoginAsync(SocialPlatforms platform, string token)
        {
            WWWForm f = new WWWForm();
			f.AddField(Constants.FIELD_SERVICE, Constants.VALUE_FACEBOOK); 
            f.AddField(Constants.FIELD_TOKEN, token);
            f.AddField(Constants.FIELD_GAME_ID, UserSession.GameId);

            string url = URLs.ACTION_LOGIN_OAUTH.ToURL(UseLiveServer);
            WWW www = new WWW(url, f);
            yield return www;

            // Compose the Result //
            var result = WWWParser.Parse<LoginResult>(www);

            if (result.Succeeded)
            {
                UserSession.SessionId = result.Session;
                UserSession.CurrentUser = result.Profile;
                //Cache session
                PlayerPrefs.SetString(Constants.GOPLAY_SESSION, result.Session);
                RequireOAuthData = new OAuthDataObject { Platform = platform, Token = token }; // Set //
            }
            else
            {
                if ( result.ErrorCode == Error.OauthUserNotExist)
                {
                    RequireOAuthData = new OAuthDataObject { Platform = platform, Token = token }; // Set //
                }
            }
           
            // Trigger Event //
            OnLogin(result);
        }
        #endregion

        #region Logout
        public void LogOut()
        {
            if (OnLogOut != null)
            {
                LogOutSync();
            }
        }
        private void LogOutSync()
        {
            PlayerPrefs.DeleteKey(Constants.GOPLAY_SESSION);
            UserSession.SessionId = string.Empty;

            var result = new Result { Succeeded = true, ErrorCode = Error.None };

            OnLogOut(result);
        }
        #endregion

        #region BindOAuth
		private IEnumerator BindOauthAsync(string session, OAuthDataObject oData)
        {
            WWWForm f = new WWWForm();
			f.AddField(Constants.FIELD_SESSION, session);
            f.AddField(Constants.FIELD_GAME_ID, UserSession.GameId);
			f.AddField(Constants.FIELD_SERVICE, Constants.VALUE_FACEBOOK);
            f.AddFieldIfValid(Constants.FIELD_TOKEN, oData.Token);
			//get url
            string url = URLs.ACTION_BIND_OAUTH.ToURL(UseLiveServer);
            WWW www = new WWW(url, f);
            yield return www;
            // Check out the result //
            oData.TryParse(www);
        }

		private IEnumerator LoginOAuthAsync(OAuthDataObject oData)
		{
			WWWForm f = new WWWForm();
			f.AddField(Constants.FIELD_SESSION, UserSession.SessionId);
			f.AddField(Constants.FIELD_GAME_ID, UserSession.GameId);
			f.AddField(Constants.FIELD_SERVICE, Constants.VALUE_FACEBOOK);
			f.AddFieldIfValid(Constants.FIELD_TOKEN, oData.Token);
			//get url
			string url = URLs.ACTION_LOGIN_OAUTH.ToURL(UseLiveServer);
			WWW www = new WWW(url, f);
			yield return www;
			// Check out the result //
			oData.TryParse(www);
		}
        #endregion

		#region UnBindOAuth 
		private void UnBindOAuth(string token)
		{

			if(UserSession.HasLoggedIn)
			{
				StartCoroutine(UnBindOAuthAsysnc(token));
			}
		}
		
        private IEnumerator UnBindOAuthAsysnc(string token)
		{
			WWWForm f = new WWWForm();
			f.AddField(Constants.FIELD_SESSION, UserSession.SessionId);
			f.AddField(Constants.FIELD_GAME_ID, UserSession.GameId);
			f.AddField(Constants.FIELD_SERVICE, Constants.VALUE_FACEBOOK);
			f.AddFieldIfValid(Constants.FIELD_TOKEN, token);
			//get url
			string url = URLs.ACTION_UNBIND_OAUTH.ToURL(UseLiveServer);
			WWW www = new WWW(url, f);
			yield return www;
            // Compose the Result //
            var result = WWWParser.Parse<Result>(www);
			if(result.Succeeded)
			{
				PlayerPrefs.DeleteKey(Constants.GOPLAY_SESSION);
				UserSession.SessionId=string.Empty;
			}
		}
		#endregion
		
		#region Check- OAuth - BInding
		
		public bool IsOAuthBinded(OAuthDataObject oData)
		{
            bool binded = false;
			if(oData != null && UserSession.HasLoggedIn)
                StartCoroutine(IsOAuthBindedAsysnc(oData, binded));
            return binded;

		}
		private IEnumerator IsOAuthBindedAsysnc(OAuthDataObject oData,  object binded)
		{
			WWWForm f = new WWWForm();
			f.AddField(Constants.FIELD_SESSION, UserSession.SessionId);
			f.AddField(Constants.FIELD_GAME_ID, UserSession.GameId);
			f.AddField(Constants.FIELD_SERVICE, Constants.VALUE_FACEBOOK);
			f.AddFieldIfValid(Constants.FIELD_TOKEN, oData.Token);
			//get url
			string url = URLs.ACTION_CHECK_OAUTH_BINDING.ToURL(UseLiveServer);
			WWW www = new WWW(url, f);
			yield return www;
			// Check out the result //
            var result = WWWParser.Parse<OAuthDataObject>(www);
            binded = result.Succeeded && result.ErrorCode == Error.OauthAlreadyConnected;
		}
		#endregion

        #region GetProfile
		public void GetProfile()
        {
            if (OnGetProfile != null && UserSession.HasLoggedIn)
            {
				StartCoroutine(GetProfileAsync());
			}
        }

		private IEnumerator GetProfileAsync()
        {
            WWWForm f = new WWWForm();
			f.AddField(Constants.FIELD_SESSION, UserSession.SessionId);
			f.AddField(Constants.FIELD_GAME_ID, UserSession.GameId);
			//get url
            string url = URLs.ACTION_GET_PROFILE.ToURL(UseLiveServer); 
            WWW www = new WWW(url, f);
            yield return www;
            // Compose the Result //
            var result = WWWParser.Parse<ProfileResult>(www);
            // Trigger Event //
            OnGetProfile(result);
        }
        #endregion

        #region EditProfile
		public void EditProfile(string email=null, 
                                string nickName=null, 
                                Gender? gender=null )
        {
            if (OnEditProfile != null && UserSession.HasLoggedIn)
            {
				StartCoroutine(EditProfileAsync(email, nickName, gender));
            }
        }

		private IEnumerator EditProfileAsync(   string email = null, 
                                                string nickName = null, 
                                                Gender? gender = null)
        {
			WWWForm f = new WWWForm();
			f.AddField(Constants.FIELD_SESSION, UserSession.SessionId);
            f.AddField(Constants.FIELD_GAME_ID, UserSession.GameId);

            f.AddFieldIfValid(Constants.FIELD_EMAIL, email);
            f.AddFieldIfValid(Constants.FIELD_NICKNAME, nickName);
            if (gender.HasValue) f.AddField(Constants.FIELD_GENDER, gender.ToString());

            string url = URLs.ACTION_EDIT_PROFILE.ToURL(UseLiveServer); 
            WWW www = new WWW(url, f);
            yield return www;

            // Compose the Result //
            var result = WWWParser.Parse<ProfileResult>(www);

            // Trigger Event //
            OnEditProfile(result);
        }
        #endregion

        #region GetProgress
		public void GetProgress(bool sendData)
        {
            if (OnGetProgress != null && UserSession.HasLoggedIn)
			{
				StartCoroutine(GetProgressAsync(UserSession.GameId, sendData));
            }
        }

		private IEnumerator GetProgressAsync(string gameId, bool sendData)
        {
            WWWForm f = new WWWForm();
			f.AddField(Constants.FIELD_SESSION, UserSession.SessionId);
            f.AddField(Constants.FIELD_GAME_ID, gameId);
            f.AddField(Constants.FIELD_SEND_DATA, sendData.ToString()); 
			//get url
            string url = URLs.ACTION_GET_PROGRESS.ToURL(UseLiveServer);  
            WWW www = new WWW(url, f);
            yield return www;
            // Compose the Result //
            var result = WWWParser.Parse<GetProgressResult>(www);
            // Trigger Event //
            OnGetProgress(result);
        }
        #endregion

        #region SaveProgress
        public void SaveProgress(string data, string meta=null)
        {
            if (OnSaveProgress != null && UserSession.HasLoggedIn)
			{
				StartCoroutine(SaveProgressAsync(data, meta));
            }
        }

        private IEnumerator SaveProgressAsync(string data, string meta = null)
        {
            WWWForm f = new WWWForm();
			f.AddField(Constants.FIELD_SESSION, UserSession.SessionId);
            f.AddField(Constants.FIELD_GAME_ID, UserSession.GameId);
            f.AddField(Constants.FIELD_DATA, data);
            f.AddFieldIfValid(Constants.FIELD_META, meta);
			//get url
            string url = URLs.ACTION_SAVE_PROGRESS.ToURL(UseLiveServer); 
            WWW www = new WWW(url, f);
            yield return www;
            // Compose the Result //
            var result = WWWParser.Parse<Result>(www);
            // Trigger Event //
            OnSaveProgress(result);
        }
        #endregion

        #region Update Game Stats
        public void UpdateGameStats(GameStats stats)
        {
            if (OnUpdateGameStats != null && UserSession.HasLoggedIn)
			{
				StartCoroutine(UpdateGameStatsAsync(stats));
            }
        }

        private IEnumerator UpdateGameStatsAsync(GameStats stats)
        {
            WWWForm f = new WWWForm();
			f.AddField(Constants.FIELD_SESSION, UserSession.SessionId);
            f.AddField(Constants.FIELD_GAME_ID, UserSession.GameId);
           	f.AddField(Constants.FIELD_STATS, stats.ToJson()); 
			//get url
            string url = URLs.ACTION_UPDATE_GAME_STATS.ToURL(UseLiveServer);
            WWW www = new WWW(url, f);
            yield return www;
            // Compose the Result //
            var result = WWWParser.Parse<Result>(www);
            // Trigger Event //
            OnUpdateGameStats(result);
        }
        #endregion

		#region Update-external-exchange
		public void UpdateExternalExchange(string transactionId, string exchangeOptionIdentifier )
		{
			if(OnUpdateExternalExchange != null && UserSession.HasLoggedIn)
			{
				StartCoroutine(UpdateExternalExchangeAsync(transactionId, exchangeOptionIdentifier));
			}
		}
		private IEnumerator UpdateExternalExchangeAsync(string transactionId, string exchangeOptionIdentifier)
		{
			WWWForm f = new WWWForm();
			f.AddField(Constants.FIELD_SESSION, UserSession.SessionId);
			f.AddField(Constants.FIELD_GAME_ID, UserSession.GameId);
			f.AddField(Constants.FIELD_TRANSACTION_ID, transactionId);
			f.AddField(Constants.FIELD_EXCHANGE_OPTION_IDENTIFIER, exchangeOptionIdentifier);

			string url = URLs.ACTION_UPDATE_EXTERNAL_EXCHANGE.ToURL(UseLiveServer);
			WWW www = new WWW(url, f);			
			yield return www;

            // Compose the Result //
            var result = WWWParser.Parse<Result>(www);
			//Trigger Even
			OnUpdateExternalExchange(result);
		}
		#endregion

        #region Get Unfullfilled Exchanges
        public void GetUnFullFilledExchanges()
        {
            if (OnGetUnFullFilledExchanges != null && UserSession.HasLoggedIn)
			{
				StartCoroutine(GetUnFullFilledExchangesAsync());
            }
        }

        private IEnumerator GetUnFullFilledExchangesAsync()
        {
            WWWForm f = new WWWForm();
			f.AddField(Constants.FIELD_SESSION, UserSession.SessionId);
            f.AddField(Constants.FIELD_GAME_ID, UserSession.GameId);
			//get url
            string url = URLs.ACTION_GET_UNFULLFILLED_EXCHANGE.ToURL(UseLiveServer);
            WWW www = new WWW(url, f);
            yield return www;
            // Compose the Result //
            var result = WWWParser.Parse<GetUnFullFilledExchangesResult>(www);
            // Trigger Event //
            OnGetUnFullFilledExchanges(result);
        }
        #endregion

        #region Fullfilled Exchanges
        public void FullFillExchange(string transactionId)
        {
            if (OnFullFillExchange != null && UserSession.HasLoggedIn)
			{
				StartCoroutine(FullFillExchangeAsync(transactionId));
            }
        }

        private IEnumerator FullFillExchangeAsync(string transactionId)
        {
            WWWForm f = new WWWForm();
			f.AddField(Constants.FIELD_SESSION, UserSession.SessionId);
            f.AddField(Constants.FIELD_GAME_ID, UserSession.GameId);
            f.AddField(Constants.FIELD_TRANSACTION_ID, transactionId);

            string url = URLs.ACTION_FULLFILL_EXCHANGE.ToURL(UseLiveServer);
            WWW www = new WWW(url, f);
            yield return www;

            // Compose the Result //
            var result = WWWParser.Parse<FullFillExchangeResult>(www);

            // Trigger Event //
            OnFullFillExchange(result);
        }

        #endregion

		#region RejectExchange
		public void RejectExchange(string transactionId)
		{
			if(OnRejectExchange != null && UserSession.HasLoggedIn)
			{
				StartCoroutine(RejectExchangeAsync(transactionId));
			}
		}
		private IEnumerator RejectExchangeAsync(string transactionId)
		{
			WWWForm f = new WWWForm();
			f.AddField(Constants.FIELD_SESSION, UserSession.SessionId);
			f.AddField(Constants.FIELD_GAME_ID, UserSession.GameId);
			f.AddField(Constants.FIELD_TRANSACTION_ID, transactionId);
			
			string url = URLs.ACTION_REJECT_EXCHANGE.ToURL(UseLiveServer);
			WWW www = new WWW(url, f);
			yield return www;
			
			// Compose the Result //
			var result = WWWParser.Parse<RejectExchangeResult>(www);
			
			// Trigger Event //
			OnRejectExchange(result);
		}

		#endregion

        #endregion

        #region Privates
        // Private transcient data //
        private OAuthDataObject RequireOAuthData { get; set; }

        #endregion

		public void ClearEventDelegate(){
			OnLogin = null;
			OnLogOut = null;
			OnRegister = null;
			OnGetProfile = null;
			OnEditProfile = null;
			OnGetProgress = null;
			OnSaveProgress = null;
			OnUpdateGameStats = null;
			OnGetUnFullFilledExchanges = null;
			OnFullFillExchange = null;
			OnUpdateExternalExchange = null;
			OnRejectExchange = null;
		}
		
		public int GetOnLogoutLength(){
			if(OnLogOut == null)
				return 0;
			return OnLogOut.GetInvocationList ().Length;
		}
    }
}


