namespace GoPlaySDK
{
    static public class Constants
    {
        // Parameter Fields //
        public const string FIELD_USERNAME                      = "username";
        public const string FIELD_PASSWORD                      = "password";
        public const string FIELD_EMAIL                         = "email";
        public const string FIELD_NICKNAME                      = "nickname";
        public const string FIELD_GENDER                        = "gender";
        public const string FIELD_GAME_ID                       = "game_id";
        public const string FIELD_REFERRAL_CODE                 = "referral_code";
        public const string FIELD_SESSION                       = "session";
        public const string FIELD_SEND_DATA                     = "send_data";
        public const string FIELD_DATA                          = "data";
        public const string FIELD_META                          = "meta";
        public const string FIELD_STATS                         = "stats";
        public const string FIELD_TRANSACTION_ID                = "transaction_id";
        public const string FIELD_SERVICE                       = "service";
        public const string FIELD_TOKEN                         = "token";
		public const string GOPLAY_SESSION                      = "Gtoken_Session";
		public const string FIELD_IP_ADDRESS                    = "ip_address";
		public const string FIELD_EXCHANGE_OPTION_IDENTIFIER 	= "exchange_option_identifier";
		public const string FIELD_TITLE							= "title";
		public const string FIELD_VALUE							= "value";
		public const string FIELD_PUBLIC						= "public";
        // Responses //
        public const string RESPONSE_SUCCESS                    = "success";
        public const string RESPONSE_MESSAGE                    = "message";
        public const string RESPONSE_ERROR_CODE                 = "error_code";
        public const string RESPONSE_SESSION                    = "session";
        public const string RESPONSE_PROFILE                    = "profile";
        public const string RESPONSE_DATA                       = "data";
        public const string RESPONSE_META                       = "meta";
		public const string RESPONSE_USER_ID                    = "uid";
		public const string RESPONSE_COUNTRY_CODE               = "country_code";
		public const string RESPONSE_GOPLAY_TOKEN               = "goplay_token";
		public const string RESPONSE_FREE_GOPLAY_TOKEN          = "free_goplay_token";
		public const string RESPONSE_GTOKEN                     = "gtoken";
		public const string RESPONSE_FREE_TOKEN                 = "free_gtoken";
		public const string RESPONSE_AVATAR                     = "avatar";
		public const string RESPONSE_ACCOUNT                    = "account";
		public const string RESPONSE_NICK_NAME                  = "nickname";
		public const string RESPONSE_GENDER                     = "gender";
		public const string RESPONSE_EMAIL                      = "email";
        public const string RESPONSE_JSON_TRUE                  = "true";
		public const string RESPONSE_VIP                        = "vip";
		public const string RESPONSE_EXCHANGES                   = "exchanges";
		public const string RESPONSE_EXCHANGE                   = "exchange";
		public const string RESPONSE_TRANSACTION_ID             = "transaction_id";
		public const string RESPONSE_EXCHANGE_OPTION_TYPE       = "exchange_option_type";
		public const string RESPONSE_EXCHANGE_OPTION_IDENTIFIER = "exchange_option_identifier";
		public const string RESPONSE_GOPLAY_TOKEN_VALUE         = "goplay_token_value";
		public const string RESPONSE_GTOKEN_VALUE               = "gtoken_value";
		public const string RESPONSE_QUANTITY                   = "quantity";
		public const string RESPONSE_IS_FREE                    = "is_free";
        // Values //
        public const string VALUE_FACEBOOK                      = "facebook";
    }

 
    #region URLS
    static public class URLs
    {
		public const string BASE_DEV_SERVER                     = @"https://dev.goplay.la/api/1/";
		public const string BASE_LIVE_SERVER                    = @"https://goplay.gtoken.com/api/1/";

        public const string ACTION_REGISTER                     = @"account/register";
        public const string ACTION_LOGIN                        = @"account/login-password";
        public const string ACTION_LOGIN_OAUTH                  = @"account/login-oauth";
		public const string ACTION_BIND_OAUTH                   = @"account/connect-oauth";
		public const string ACTION_UNBIND_OAUTH                 = @"account/disconnect-oauth";
		public const string ACTION_CHECK_OAUTH_BINDING          = @"account/check-oauth-connection";
        public const string ACTION_GET_PROFILE                  = @"account/profile";
        public const string ACTION_EDIT_PROFILE                 = @"account/profile-edit";
        public const string ACTION_GET_PROGRESS                 = @"game/get-progress";
        public const string ACTION_SAVE_PROGRESS                = @"game/save-progress";
        public const string ACTION_UPDATE_GAME_STATS            = @"game/update-game-stats";
        public const string ACTION_FULLFILL_EXCHANGE            = @"transaction/fulfill-exchange";
        public const string ACTION_GET_UNFULLFILLED_EXCHANGE    = @"transaction/get-unfulfilled-exchanges";   
		public const string ACTION_UPDATE_EXTERNAL_EXCHANGE    	= @"transaction/update-external-exchange";
		public const string ACTION_REJECT_EXCHANGE				= @"transaction/reject-exchange";

    }

    #endregion
       
  




}

