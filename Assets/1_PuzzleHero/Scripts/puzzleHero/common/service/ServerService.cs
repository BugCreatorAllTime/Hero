
using System;
using LitJson;
using common.network;
using common.network.request;


namespace common.service
{
	public class ServerService
	{
		[Inject]
		public HttpConnection connection { get; set; }

		[Inject]
		public OnLoginSignal onLoginSignal { get; set; }

		public void Login (string username, string password, string version)
		{
			LoginRequest request = new LoginRequest ();
			request.username = username;
			request.password = password;
			request.version = version;
			connection.SendRequest (request, "server/login", OnLogin);
		}

		private void OnLogin (object req, string data, string error)
		{
			if (error != null) {
					Logger.Info ("Network cannot connect ", error);
					return;
			}

			LoginResponse loginData = JsonMapper.ToObject<LoginResponse> (data);


			Logger.Info ("LoginData OK ", JsonMapper.ToJson (loginData));


			//------------------- test getUserInfo

			connection.Token = loginData.token;

			GetUserInfo ();

		}

		private void GetUserInfo ()
		{
			connection.SendRequest (new GetUserInfoRequest (), "user/info", OnUserInfo);
		}

		private void OnUserInfo (object req, string data, string error)
		{
			if (error != null) {
				Logger.Info ("Network cannot connect ", error);
				return;
			}

			Logger.Info ("UserInfo Data OK ", data);
		}

		public void ValidateTransaction(string transactionId, string original_tranId, common.network.HttpConnection.OnResponse callback )
		{
			PaymentRequest req = new PaymentRequest();
			req.transactionId = transactionId;
			req.original_tranId = original_tranId;
			connection.SendRequest(req ,"payment",callback);
		}
	}
}
