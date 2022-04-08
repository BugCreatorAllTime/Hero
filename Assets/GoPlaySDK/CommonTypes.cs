using UnityEngine;
using System;
using System.ComponentModel;
using System.Reflection;



namespace GoPlaySDK
{
    public enum SocialPlatforms
    {
        None,
        FaceBook
    }

    public enum Gender
    {
        [Description("male")]       Male,
        [Description("female")]     Female,
        [Description("other")]      Other
    }

    public enum Vip
    {
        [Description("null")]       Null,
        [Description("standard")]   Standard,
        [Description("gold")]       Gold
    }

	public enum OAuthTypes
	{
		None,
		Login,
		Connect,
		Disconnect,
		CheckOAuth
	}

    public enum Error
    {
        [Description("No Error")]                       None,
        [Description("INVALID_GAME_ID")]                InvalidGameId,
        [Description("MISSING_FIELDS")]                 MissingFields,
        [Description("EXISTING_EMAIL")]                 EmailExist,
        [Description("EXISTING_USERNAME_EMAIL")]        UserNameOrEmailExist,
        [Description("USERNAME_LENGTH")]                UserNameLengthMustBe3To20,
        [Description("INVALID_USERNAME")]               InvalidUserNameCharacters,
        [Description("PASSWORD_LENGTH")]                PasswordLengthMustAtLeast3,
        [Description("INVALID_USN_PWD")]                InvalidUserNameOrPassword,
        [Description("NON_EXISTING_OAUTH")]             OauthUserNotExist,
        [Description("INVALID_SESSION")]                InvalidSession,
        [Description("INVALID_TRANSACTION_ID")]         InvalidTransactionId,
        [Description("TRANSACTION_ALREADY_PROCESSED")]  TransactionAlreadyProcessed,
        [Description("NOT_SUPPORTED_OAUTH_PROVIDER")]   UnSupportedOauthProvider,
        [Description("EXISTING_OAUTH")]                 OauthUserAlreadyExist,
        [Description("OAUTH_ALREADY_CONNECTED")]        OauthAlreadyConnected,
        [Description("OAUTH_USER_NOT_CONNECTED")]       OauthAndUserNotConnected,
        [Description("INVALID_PARTNER_ID")]       		INVALID_PARTNER_ID,
		[Description("NON_EXISTING_REFERRAL_CODE")]     NON_EXISTING_REFERRAL_CODE,

        // Additional Error Codes //
        [Description("USER_ALREADY_LOGGED_IN")]         UserAlreadyLoggedIn = 1000,
        [Description("USER_NOT_LOGGED_IN")]             UserNotLoggedIn,


        [Description("HTTP_REQUEST_ERROR")]             HttpRequestError = 2000,

    }


    public enum ExchangeOptionTypes
    {
		[Description("CreditType")]     Credit,
		[Description("Package")]        Package
    }

    // Enum Helpers //
    static public class Converter
    {
        static public T? EnumFromDescription<T>(string s) where T: struct
        {
            foreach (T value in Enum.GetValues(typeof(T)))
            {
                Type type = value.GetType();
                string name = Enum.GetName(type, value);
                if (string.IsNullOrEmpty(name)==false)
                {
                    FieldInfo field = type.GetField(name);
                    if (field != null)
                    {
                        var attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                        if (attr != null && string.Compare(attr.Description, s, true)==0)
                        {
                            return value;
                        }
                    }
                }
            }
            return null;
        }
    }
}

