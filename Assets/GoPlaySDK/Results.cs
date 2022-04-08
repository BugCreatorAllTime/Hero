using UnityEngine;
using System;
using System.Collections.Generic;


namespace GoPlaySDK
{
	#region Results 
	public interface IResult
	{
		bool Succeeded { get; set; }
        string Message { get; set; }
		Error ErrorCode { get; set; }  
	}
	
	public class Result : IResult
	{
		public bool Succeeded { get; set; } 
        
        public string Message { get; set; }
		
		public Error ErrorCode { get; set; } 
		
		internal bool TryParse(WWW www)
		{
			Succeeded = false;
			
			if (string.IsNullOrEmpty(www.error))
			{
				// No error, try to read the JSON data //
				try
				{
					Debug.Log(www.text);
					JSONObject json = new JSONObject(www.text);
                    Succeeded = json.GetValue(Constants.RESPONSE_SUCCESS) == Constants.RESPONSE_JSON_TRUE;
                    if (Succeeded)
					{
						OnParse(json);
					}
					else
					{ 
						Message = json[Constants.RESPONSE_MESSAGE].str;
						var errorString = json[Constants.RESPONSE_ERROR_CODE].str;
						Error? errCode = Converter.EnumFromDescription<Error>(errorString);
						
						#if DEBUG
						if (errCode==null)
						{
							Debug.Log("Unable to Parse this enum. Pls check this : " + errorString);
						}
						#endif						
						ErrorCode = errCode ?? Error.None;
					}
				}
				catch (Exception ex)
				{
                    Debug.LogError(ex.Message);
                    Debug.LogError("Unable to parse Json data");
					Message=ex.Message;
				}
			}
			else
			{
				Debug.LogError("Error message: "+www.error);
                ErrorCode = Error.HttpRequestError;
				Message=www.error;
			}
			return Succeeded;
		}

        protected virtual void OnParse(JSONObject json) { }
     
		public void CopyFrom(IResult result)
		{
			Succeeded = result.Succeeded;
			ErrorCode = result.ErrorCode;
		}
	}
	
	public class ProfileResult : Result
	{
		public UserProfile Profile  { get; set; }
		
		#region  Parse
		protected override void OnParse(JSONObject json)
		{
            base.OnParse(json);
			Profile = UserProfile.Create(json);
		}
		#endregion
	}

	public class RegisterResult : ProfileResult 
	{
		public string Session { get; set; }
		#region  Parse
		protected override void OnParse(JSONObject json)
		{
			base.OnParse(json);
			Session = json.GetField(Constants.RESPONSE_SESSION).str;
		}
		#endregion
	}
	public class LoginResult : RegisterResult { }

	public class GetProgressResult : Result
	{
		public string Data { get; set; }
		public string Meta { get; set; }
		
		#region  Parse
		protected override void OnParse(JSONObject json)
		{
			base.OnParse(json);
			Data = json.GetField(Constants.RESPONSE_DATA).str;			
			// Optional //
			var item = json.GetField(Constants.RESPONSE_META);
            Meta = item==null ? null : item.str;
		}
		#endregion
	}
	
	public class GetUnFullFilledExchangesResult : Result 
	{
		public Exchanges Exchanges { get; set; }
		
		#region  Parse
		protected override void OnParse(JSONObject json)
		{
            base.OnParse(json);
			Exchanges = Exchanges.Create(json);
		}
		#endregion
		
	}
	
	public class FullFillExchangeResult : Result
	{
		public Exchange Exchange { get; set; }
		
		#region  Parse
		protected override void OnParse(JSONObject json)
		{
            base.OnParse(json);
			Exchange = Exchange.Create(json);
		}
		#endregion
	}

	public class RejectExchangeResult:FullFillExchangeResult{}
	#endregion

    #region Helpers
    public class WWWParser
    {
        public static T Parse<T>(WWW www) where T : Result, new()
        {
            T result = new T();
            result.TryParse(www);
            return result;
        }
    }
    #endregion

    #region Supporting Data structures
    public class UserProfile
	{
		public int Id { get; set; }
		public string UserName { get; set; } 
		public string NickName { get; set; } 
		public string Email { get; set; } 
		public Gender Gender { get; set; }
		public Vip VipStatus { get; set; }
		public string CountryCode { get; set; }         
		public decimal GoPlayToken{get;set;}
		public decimal FreeBalance { get; set; }
		
		static public UserProfile Create(JSONObject json)
		{
			var doc=json[Constants.RESPONSE_PROFILE];
			var newProfile = new UserProfile
			{
				Id = Convert.ToInt32(doc.GetField(Constants.RESPONSE_USER_ID).ToString()),
				UserName = doc.GetField(Constants.RESPONSE_ACCOUNT).str,
				NickName = doc.GetField(Constants.RESPONSE_NICK_NAME).str,
				Email = doc.GetField(Constants.RESPONSE_EMAIL).str,
				Gender = Converter.EnumFromDescription<Gender>(doc.GetField(Constants.RESPONSE_GENDER).str) ?? Gender.Other,
				VipStatus = Converter.EnumFromDescription<Vip>(doc.GetField(Constants.RESPONSE_VIP).ToString())?? Vip.Null,
				CountryCode = doc.GetField(Constants.RESPONSE_COUNTRY_CODE).str,
				GoPlayToken = Convert.ToDecimal( doc.GetField(Constants.RESPONSE_GOPLAY_TOKEN).ToString()),                
                FreeBalance = Convert.ToDecimal( doc.GetField(Constants.RESPONSE_FREE_TOKEN).ToString())
			};
			
			return newProfile;
		}
	}
	
	public class GameStat
	{
		public string Title { get; set; }
		public string Value { get; set; }
		public bool Public { get; set; }
	}
	
	public class GameStats : List<GameStat> 
    {
        public string ToJson()
        {
            JSONObject arrStats = new JSONObject(JSONObject.Type.ARRAY);
            foreach (var item in this)
            {
                JSONObject j = new JSONObject(JSONObject.Type.OBJECT);
				j.AddField(Constants.FIELD_TITLE, item.Title);
				j.AddField(Constants.FIELD_VALUE, item.Value);
				j.AddField(Constants.FIELD_PUBLIC, item.Public);
                arrStats.Add(j);
            }

            return arrStats.Print();
        }
    }
	
	public class Exchange
	{
		public Guid TransactionId { get; set; } 
		public ExchangeOptionTypes ExchangeType { get; set; }
		public string ExchangeOptionIdentifier { get; set;}
		public decimal GoPlayTokenValue { get; set;}
		public int Quantity { get; set; }
		public bool IsFree { get; set; }

		static public Exchange Create(JSONObject json,bool isList=false)
		{
            var newExchange = new Exchange();
			var data=isList ? json : json[Constants.RESPONSE_EXCHANGE];
            if (data != null)
            {
				newExchange.TransactionId = new Guid(data.GetField(Constants.RESPONSE_TRANSACTION_ID).str);
				newExchange.ExchangeType = Converter.EnumFromDescription<ExchangeOptionTypes>(data.GetField(Constants.RESPONSE_EXCHANGE_OPTION_TYPE).ToString()) ?? ExchangeOptionTypes.Credit;
				newExchange.ExchangeOptionIdentifier = data.GetField(Constants.RESPONSE_EXCHANGE_OPTION_IDENTIFIER).str;
                newExchange.GoPlayTokenValue = Convert.ToDecimal( data.GetField(Constants.RESPONSE_GOPLAY_TOKEN_VALUE).ToString());
                newExchange.Quantity = Convert.ToInt32( data.GetField(Constants.RESPONSE_QUANTITY).ToString());
                newExchange.IsFree = Convert.ToBoolean(data.GetField(Constants.RESPONSE_IS_FREE).str);
			}
			return newExchange;
		}
	}
	
	public class Exchanges : List<Exchange> 
	{
        static public Exchanges Create(JSONObject json)
        {
            var newExchanges = new Exchanges();
            JSONObject data = json.GetField(Constants.RESPONSE_EXCHANGES);
            if (data != null)
            {
                foreach (JSONObject obj in data.list)
                {
                    Exchange ex = Exchange.Create(obj,true);
                    newExchanges.Add(ex);
                }
            }
            return newExchanges;
        }
	}
	
	
	public class OAuthDataObject : Result
	{
		public SocialPlatforms Platform { get; set; }
		public string Token { get; set; }
		public OAuthTypes OAuthType {get; set;}

	}
	
	#endregion
}
