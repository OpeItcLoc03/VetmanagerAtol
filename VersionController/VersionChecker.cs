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
        public const string FILENAME = "new version.zip";
        public const string PARAM1 = "from_atol";
        public const string UPDATEFILE = "vmUpdator.exe"; // do not change !!!
        public const string RELEASEDIR = "_release_temp"; // do not change !!!

        public Response CheckVersion(string url, string txtClientVersion)
        {
            Response resp = new Response();

            using (WebClient wb = new WebClient())
            {
                var values = new NameValueCollection();

                values["cmd"] = "check_version";
                values["client_version"] = txtClientVersion;

                try
                {
                    byte[] response = wb.UploadValues(url, "POST", values);

                    string data = Encoding.UTF8.GetString(response);

                    if (data == "version_up_to_date")
                    {
                        resp.message = "У вас актуальная версия клиента";
                        resp.isUpToDate = true;
                    }
                    else if (data == "file_not_exists")
                    {
                        resp.message = "Необходимого файла нет";
                        resp.isError = true;
                    }
                    else
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
                }
                catch (Exception e)
                {
                    resp.isError = true;
                    resp.message = "Ошибка загрузки файла: " + e.Message;
                }
            }

            return resp;
        }

        public bool UnzipProgram()
        {
            if (!File.Exists(FILENAME))
            {
               // MessageBox.Show("no file");
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
            using (var zip = ZipFile.Read(FILENAME))
            {
                zip.ExtractAll(RELEASEDIR, ExtractExistingFileAction.OverwriteSilently);
            }

            int fileCount = new DirectoryInfo(RELEASEDIR).GetFiles().Length;

            if (fileCount > 5 && File.Exists(FILENAME))
            {
                try
                {
                    File.Delete(FILENAME);
                }
                catch { }
            }

            return fileCount > 5;
        }

        public bool StartUpdator()
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
    }
}
