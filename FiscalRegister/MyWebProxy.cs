using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;

namespace Atol
{
    public class MyWebProxy
    {
        //public delegate void MyEventHandler(object result, bool isError, string lastErrorMessage);
        //public event MyEventHandler OnResult;
        public MyWebProxy() { }
        public MyWebProxy(string u, string a)
        {
            this.url = u;
            this.api = a;
        }

        public string url = "";
        public string api = "";
        
        private WebResult getDataByAction(string action, List<KeyValuePair<string, string>> subdata)
        {
            WebResult result = new WebResult();
            if (subdata == null)
            {
                subdata = new List<KeyValuePair<string, string>>();
            }

            using (WebClient wb = new WebClient())
            {
                wb.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.1; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36";
 
                result.isError = false;
                result.lastErrorMessage = "";
                var data = new NameValueCollection();
                data["api"] = api.Trim();
                data["action"] = action;
                data["r"] = (new Random().Next(1000000, 9999999)).ToString();

                if ("set_response_data" != action)
                {
                    data["vmVersionFromClient"] = Version.vmVersion;
                    data["client_version"] = Version.version;
                }
                                
                foreach (KeyValuePair<string, string> item in subdata)
                {
                    data[item.Key] = item.Value;
                }

                byte[] response = null;
                try
                {
                        response = wb.UploadValues(url, "POST", data);
                }
                catch (Exception e)
                {
                    result.isError = true;
                    result.lastErrorMessage = e.Message;
                    result.data = "";
                }

                if (!result.isError)
                {
                    result.data = Encoding.UTF8.GetString(response);
                }
            }

            return result;
        }

        public WebResult checkConnection()
        {
            return this.getDataByAction("check_connection", null);
        }

        internal WebResult getUnsendedData()
        {
            return this.getDataByAction("get_unsended_data", null);
        }

        internal WebResult SendResponseData(List<WebResult> r)
        {
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            KeyValuePair<string, List<WebResult>> res = new KeyValuePair<string, List<WebResult>>("data", r); 
            JObject obj = JObject.FromObject(res);
            result.Add(new KeyValuePair<string, string>("results", obj.ToString().Replace("\r\n", "")));
            return this.getDataByAction("set_response_data", result);            
        }
    }
}
