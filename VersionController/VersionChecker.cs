using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Collections.Specialized;
using System.IO;
using Ionic.Zip;
using System.Diagnostics;

namespace VersionController
{
    public class VersionChecker
    {
        public const string ACTION_CHECK_VERSION = "checkVersion";
        public const string ACTION_GET_CLIENT = "getClient";
        public const string ONLINE_UPDATE_URL = "https://login.vetmanager.ru/OnlineCassa.php";
        
        public const string FILENAME = "new version.zip";
        public const string PARAM1 = "from_atol";
        public const string CASSA_TYPE = "atol";
        public const string UPDATEFILE = "vmUpdator.exe"; // do not change !!!
        public const string RELEASEDIR = "_release_temp"; // do not change !!!

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

        private static Response DownloadRemoteClient(string clientVersion)
        {
            Response resp = new Response();

            using (WebClient wb = new WebClient())
            {
                var values = new NameValueCollection();
                values["clientVersion"] = clientVersion;
                values["action"] = ACTION_GET_CLIENT;
                values["cassaType"] = CASSA_TYPE;
                values["r"] = (new Random().Next(1000000, 9999999)).ToString();          

                try
                {
                    byte[] response = wb.UploadValues(ONLINE_UPDATE_URL, "POST", values);
                    string data = Encoding.UTF8.GetString(response);

                    if (data != "")
                    {
                        if ("file_not_found" == data)
                        {
                            resp.isError = true;
                            resp.message = "Файл не найден";
                        }
                        else if ("wrong_params" == data)
                        {
                            resp.isError = true;
                            resp.message = "Не верные параметры";
                        }
                        else
                        {
                            try
                            {
                                if (File.Exists(FILENAME))
                                {
                                    File.Delete(FILENAME);
                                }

                                File.WriteAllBytes(FILENAME, response);
                                resp.isFileLoadeed = true;
                                resp.message = "Файл загружен";
                            }
                            catch (Exception err)
                            {
                                resp.isError = true;
                                resp.message = "Ошибка загрузки файла: " + err.Message;
                            }
                        }
                    }
                    else
                    {
                        resp.isError = true;
                        resp.message = "Нет ответа";
                    }
                }
                catch (Exception e)
                {
                    resp.isError = true;
                    resp.message = "Ошибка загрузки файла: " + e.Message;
                }
            }

            return resp;
        }

        public static bool UnzipProgram()
        {
            if (!File.Exists(FILENAME))
            {
                return false;
            }

            if (!Directory.Exists(RELEASEDIR))
            {
                Directory.CreateDirectory(RELEASEDIR);
            }

            /////

            DirectoryInfo di = new DirectoryInfo(RELEASEDIR);

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }

            ////
            try
            {
                using (var zip = ZipFile.Read(FILENAME))
                {
                    zip.ExtractAll(RELEASEDIR, ExtractExistingFileAction.OverwriteSilently);
                }
            }
            catch (Exception err)
            {
                return false;
            }

            int fileCount = new DirectoryInfo(RELEASEDIR).GetFiles().Length;

            if (fileCount > 5 && File.Exists(FILENAME))
            {
                try
                {
                  //  File.Delete(FILENAME);
                }
                catch { }
            }

            return fileCount > 5;
        }

        public static bool StartUpdator()
        {
            if (File.Exists(RELEASEDIR + "/" + UPDATEFILE))
            {
                Directory.SetCurrentDirectory(RELEASEDIR);

                Process pp = new Process();
                pp.StartInfo.FileName = UPDATEFILE;
                pp.StartInfo.Arguments = PARAM1;
                pp.Start();
                return true;
            }

            return false;
        }

        public static Response UpdateLocal(string url, string api)
        {
            Response resp = new Response();

            using (WebClient wb = new WebClient())
            {
                var values = new NameValueCollection();
                values["action"] = "download_atol_client";
                values["api"] = api;
                values["r"] = (new Random().Next(1000000, 9999999)).ToString();        

                try
                {
                    byte[] response = wb.UploadValues(url, "POST", values);
                    string data = Encoding.UTF8.GetString(response);

                    if (data != "")
                    {
                        try
                        {
                            File.WriteAllBytes(FILENAME, response);
                            resp.isFileLoadeed = true;
                            resp.message = "Файл загружен";
                        }
                        catch (Exception err)
                        {
                            resp.isError = true;
                            resp.message = "Ошибка загрузки файла: " + err.Message;
                        }
                    }
                    else
                    {
                        resp.isError = true;
                    }
                }
                catch (Exception e)
                {
                    resp.isError = true;
                    resp.message = "Ошибка загрузки файла: " + e.Message;
                }
            }

            return resp;
        }

        public static Response UpdateRemote(string vmVersionFromClient, string clientVersion)
        {
            Response resp = CheckVersion(vmVersionFromClient);

            if (!resp.isError)
            {
                string remoteVersion = resp.message;

                if (remoteVersion != clientVersion)
                {
                    resp = null;
                    return DownloadRemoteClient(remoteVersion);
                }
            }

            // https://login.vetmanager.ru/OnlineCassa.php?vmVersionFromClient=0.123.47&action=checkVersion
            // https://login.vetmanager.ru/OnlineCassa.php?clientVersion=1.0.9&action=getClient&cassaType=atol


            return resp;            
        }
    }
}
