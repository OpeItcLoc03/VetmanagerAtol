using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using System.Diagnostics;

namespace VersionController
{
    public class VersionChecker
    {
        public const string ACTION_CHECK_VERSION = "checkVersion";
        public const string ACTION_GET_CLIENT = "getClient";
        public const string ONLINE_UPDATE_URL = "https://login.vetmanager.ru/OnlineCassa.php";

        public const string UPDATOR_FILE = "vmUpdator2.exe";
        public const string CUR_EXE_FILE = "Atol.exe";
       
        private static Response CheckVersion(string vmVersionFromClient)
        {
            Response resp = new Response();

            using (WebClient wb = new WebClient())
            {
                var values = new NameValueCollection();

                values["action"] = ACTION_CHECK_VERSION;
                values["vmVersionFromClient"] = vmVersionFromClient;
                values["r"] = (new Random().Next(1000000, 9999999)).ToString();        

                try
                {
                    byte[] response = wb.UploadValues(ONLINE_UPDATE_URL, "POST", values);
                    string data = Encoding.UTF8.GetString(response);

                    if (data == "wrong_params")
                    {
                        resp.message = "Не верные параметры";
                        resp.isError = true;
                    }
                    else if (data.IndexOf('.') != -1)
                    {
                        resp.message = data;
                    }
                    else
                    {
                        resp.isError = true;
                    }
                }
                catch (Exception e)
                {
                    resp.isError = true;
                    resp.message = "Ошибка проверки версии: " + e.Message;
                }
            }

            return resp;
        }

        public static bool IsNeedUpdateLocal(string url, string api)
        {
            url += "?api=" + api + "&action=download_atol_client";
            Process pp = new Process();
            pp.StartInfo.FileName = UPDATOR_FILE;
            pp.StartInfo.Arguments = CUR_EXE_FILE + " " + url;
            pp.Start();

            return true;
        }

        public static bool IsNeedUpdateRemote(string vmVersionFromClient, string clientVersion)
        {
            Response resp = CheckVersion(vmVersionFromClient);

            if (!resp.isError)
            {
                string remoteVersion = resp.message;

                if (isLocalVersionNewer(remoteVersion, clientVersion))
                {
                    string url = "https://login.vetmanager.ru/OnlineCassa.php?clientVersion=" + remoteVersion + "&action=getClient&cassaType=atol";
                    
                    Process pp = new Process();
                    pp.StartInfo.FileName = UPDATOR_FILE;
                    pp.StartInfo.Arguments = CUR_EXE_FILE + " " + url;
                    pp.Start();

                    return true;
                }
            }

            // https://login.vetmanager.ru/OnlineCassa.php?vmVersionFromClient=0.123.47&action=checkVersion
            // https://login.vetmanager.ru/OnlineCassa.php?clientVersion=1.0.9&action=getClient&cassaType=atol


            return false;            
        }

        private static string[] parseVersion(string ver) {
            string[] subres = ver.Split('.');

            string[] res = new string[] {"0", "0", "0"};

            for (int i = 0; i < subres.Length; i++)
			{
                res[i] = subres[i].ToString();
			}

            return res;
        }

        private static bool isLocalVersionNewer(string lVersion, string cVersion) 
        {
            string[] localVersion = parseVersion(lVersion);
            string[] clientVersion = parseVersion(cVersion);

            bool isNewer = false;

            if (int.Parse(localVersion[0]) > int.Parse(clientVersion[0])) {
                isNewer = true;
            }
            else if (int.Parse(localVersion[0]) == int.Parse(clientVersion[0]))
            {
                if (int.Parse(localVersion[1]) > int.Parse(clientVersion[1]))
                {
                    isNewer = true;
                }
                else if (int.Parse(localVersion[1]) == int.Parse(clientVersion[1]))
                {
                    if (int.Parse(localVersion[2]) > int.Parse(clientVersion[2]))
                    {
                        isNewer = true;
                    }
                }
            }
            return isNewer;
        }
    }
}
