using System;
using System.Collections;
using System.Text;
using LitJson;
using phongvan.common.utils.security;
using strange.examples.strangerocks;
using UnityEngine;
using Object = System.Object;


namespace common.network
{
    public class HttpConnection
    {
        public static String AUTH_TOKEN_HEADER = "TOKEN";

		private String _server = "http://192.168.0.191:9000/";
		private Boolean _encrypted = true;

        private String _uId = "0";
        private String _token = "9fury";

        private Aes _aes = new Aes();


        [Inject]
        public IRoutineRunner routineRunner { get; set; }

        public string Server
        {
            get { return _server; }
            set { _server = value; }
        }

        public string UId
        {
            get { return _uId; }
            set { _uId = value; }
        }

        public string Token
        {
            get { return _token; }
            set { _token = value; }
        }

        public delegate  void  OnResponse(Object req, String data ,String error);

        public void SendRequest(BaseRequest request, String api, OnResponse callBack)
        {
            request.uId = _uId;
            request.token = _token;

            routineRunner.StartCoroutine(executeRequest(request, api, callBack));
        }

        public void SendRequest(Object request, String api, OnResponse callBack)
        {

            routineRunner.StartCoroutine(executeRequest(request, api, callBack));
        }

        private IEnumerator executeRequest(Object request, String api , OnResponse callBack)
        {
            string json = JsonMapper.ToJson(request);

            Logger.Info("Request API ", api, json );

            if (_encrypted) json = _aes.Encrypt(json);

            string objStr = WWW.EscapeURL(json);

            WWWForm form = new WWWForm();
            form.AddField("data", json);
            //form.headers.Add(AUTH_TOKEN_HEADER, _token);

            HTTP.Request httpRequest = new HTTP.Request("POST", _server + api, form);
            httpRequest.AddHeader(AUTH_TOKEN_HEADER, _token);
            httpRequest.Send();

            while (!httpRequest.isDone)
            {
                yield return null;
            }

            if (httpRequest.exception != null)
            {
                callBack(request, null, httpRequest.exception.ToString());

                yield return null;
            }
            else
            {
                objStr = httpRequest.response.Text;

                if (_encrypted) objStr = _aes.Decrypt(objStr);

                Logger.Info("Response API ", api, objStr);

                callBack(request, objStr, null);
                
            }

           
        }
    }
}
