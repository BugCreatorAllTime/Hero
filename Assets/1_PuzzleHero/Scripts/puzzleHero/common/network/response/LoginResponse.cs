using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace common.network.request
{
    public class LoginResponse
    {
        public HTTP_ERROR_CODE error;
        public string errorMsg = string.Empty;
        public List<int> listUserServer = new List<int>();
        public List<ServerInfo> servers = new List<ServerInfo>();
        public string token = string.Empty;

        public string urlIphoneAppstore = string.Empty;
        public string urlIphoneJb = string.Empty;
        public string urlUpdateAndroid = string.Empty;
    }

    public class ServerInfo
    {
        private string name = string.Empty;
        private int serverID;
        private string status = string.Empty;
        private string url = string.Empty;

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }

        public int ServerID
        {
            get
            {
                return this.serverID;
            }
            set
            {
                this.serverID = value;
            }
        }

        public string Status
        {
            get
            {
                return this.status;
            }
            set
            {
                this.status = value;
            }
        }

        public string Url
        {
            get
            {
                return this.url;
            }
            set
            {
                this.url = value;
            }
        }
    }
}
