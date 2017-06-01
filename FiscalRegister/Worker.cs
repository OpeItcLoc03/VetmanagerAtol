using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.IO.Ports;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FprnM1C;

namespace Atol
{
    public class Worker
    {
        public bool isDone = false;
        public VMSettings settings = new VMSettings();
        public MainForm mainForm = null;
        private int sleepInterval = 1000 * 15 ;
        private int longPoolingSleepInterval = 1000 * 5;        
        public bool isConnectedToRegister = false;
        public bool isNeedIterrupt = false;
        private string[] errorResponses = { "no_data", "wrong_clinic", "wrong_api", "wrong_params", "no_params" };
                            
        public Worker()
        {
            isNeedIterrupt = false;
        }

        public bool CheckVetmanagerConnection()
        {
            string key = DateTime.Now.ToUniversalTime().GetHashCode().ToString();
            MyWebProxy wp = new MyWebProxy(settings.FullUrl + "?key=" + key, settings.ApiKey);
            AddToLog("Проверка соединения с "+ settings.Url);
            WebResult res = wp.checkConnection();

            wp = null;

            if (!res.isError && res.data == "connection_succesfull")
            {
                AddToLog("Соединено успешно");
                return true;
            }

            AddToLog("Ошибка соединения");
            return false;
        }

        public WebResult SendResponseData(List<WebResult> result)
        {
            AddToLog("Отправка отчета о выполнении на сервер");

            MyWebProxy wp = new MyWebProxy(settings.FullUrl, settings.ApiKey);
            WebResult res = wp.SendResponseData(result);

            wp = null;

            if (!res.isError)
            {
                AddToLog("Отчет отправлен");
            }
            else
            {
                AddToLog("Ошибка: " + res.lastErrorMessage);
            }

            return res;
        }

        public WebResult GetUnsendedData()
        {
            AddToLog("Проверка ожидающих на печать документов");

            MyWebProxy wp = new MyWebProxy(settings.FullUrl, settings.ApiKey);
            WebResult res = wp.getUnsendedData();

            wp = null;

            if (!res.isError && res.data.Length > 0)
            {
                AddToLog("Получен ответ от сервера, размер данных: " + res.data.Length + " байт");
            }
            else if (!res.isError && res.data.Length == 0)
            {
                AddToLog("Нет данных");
            }
            else
            {
                AddToLog("Ошибка: " + res.lastErrorMessage);
            }

            return res;
        }

        private FprnM8Class getDevice()
        {
            FprnM8Class atolDriver = null;

            try
            {
                atolDriver = new FprnM8Class();

                if (this.settings.Device.MachineName != null)
                {
                    atolDriver.AddDevice();
                    atolDriver.Model = int.Parse(this.settings.Device.Model);
                    atolDriver.MachineName = this.settings.Device.MachineName;
                    atolDriver.CurrentDeviceName = this.settings.Device.DeviceName;
                    atolDriver.UseAccessPassword = (int.Parse(this.settings.Device.UseAccessPassword) > 0);
                    atolDriver.DefaultPassword = this.settings.Device.DefaultPassword;
                    atolDriver.PortNumber = int.Parse(this.settings.Device.PortNumber);
                    atolDriver.BaudRate = int.Parse(this.settings.Device.BaudRate);
                    atolDriver.AccessPassword = this.settings.Device.AccessPassword;
                    atolDriver.WriteLogFile = int.Parse(this.settings.Device.WriteLogFile);
                    atolDriver.HostAddress = this.settings.Device.HostAddress;
                    atolDriver.DeviceEnabled = true;
                    atolDriver.SetSettings();

                    AddToLog("Создание обьекта драйвера: успешно");

                    return atolDriver;
                }
                else
                {
                    AddToLog("Создание обьекта драйвера: не указано имя в настройках");
                    return atolDriver;
                }
            }
            catch (Exception err)
            {
                atolDriver = null;
                AddToLog("Ошибка создания обьекта драйвера: " + err.Message);
                return null;
            }
        }

        public string ShowProperties()
        {
            string res = "";
            FprnM8Class atolDriver = this.getDevice();

            if (atolDriver == null)
            {
                return res;
            }

            if (atolDriver.ShowProperties() == 0)
            {
                res = atolDriver.DeviceSettings;
            }

            atolDriver = null;

            return res;
        }

        public void StartWork()
        {
            AddToLog("StartWork");

            bool isError = false;
            isNeedIterrupt = false;
            this.isConnectedToRegister = false;

            if (this.IsDeviceWorking())
            {
                    if (this.CheckVetmanagerConnection())
                    {
                        this.DoJob();
                    }
                    else
                    {
                        AddToLog("Нет соединения с ветменеджер");
                        isError = true;
                    }
            }
            else
            {
                AddToLog("Устройство не работает");
                isError = true;
            }

            if (isError)
            {
                System.Threading.Thread.Sleep(sleepInterval);
                StartWork();
            }
        }

        private bool IsDeviceWorking()
        {
            AddToLog("Проверка работы принтера");

            if (this.settings.Device.Model == null) 
            {
                AddToLog("Не указаны настройки принтера");
                return false;
            }

            WebResult res = this.PrintDataByParams("0", int.Parse(this.settings.Device.Model), null, "get_status");

            return !res.isError;
        }

        private void DoJob()
        {
            AddToLog("Старт рабочего потока");

            while (true)
            {
                if (isNeedIterrupt) 
                {
                    AddToLog("Остановка рабочего потока");
                    return;
                }

                if (!this.IsDeviceWorking())
                {
                    System.Threading.Thread.Sleep(sleepInterval); // 15 sek
                    DoJob();
                    return;
                }

                try
                {    
                    AddToLog("Попытка получить данные для печати");

                    List<WebResult> results = new List<WebResult>();
                    WebResult wr = this.GetUnsendedData(); // 20 sek max long pooling

                    if (!wr.isError && wr.data.Length > 0)
                    {
                        if (isErrorResponse(wr.data.ToString()))
                        {
                            AddToLog("Нет данных или ошибка ответа: " + wr.data.ToString());
                        }
                        else
                        {
                            try
                            {
                                string jsonString = @wr.data.Replace('"', '\'');
                                JObject dataString = JObject.Parse(jsonString);

                                if (dataString != null)
                                {
                                    foreach (JObject item in dataString["data"])
                                    {
                                        if (settings.ApiKey == item["api_key"].ToString())
                                        {
                                            results.Add(
                                                this.PrintDataByParams(
                                                    item["id"].ToString()
                                                    , int.Parse(this.settings.Device.Model)
                                                    , item["data"]
                                                    , item["event_name"].ToString())
                                            );
                                        }
                                        else
                                        {
                                            AddToLog("Ошибка: не совпадает API KEY !!!");
                                        }
                                    }
                                }

                                dataString = null;
                                jsonString = null;
                            }
                            catch (Exception err)
                            {
                                AddToLog("Ошибка обработки данных: " + err.Message);
                            }
                        }
                    } 
                    
                    if (results.Count > 0)
                    {
                        this.SendResponseData(results);
                    }

                    results = null;
                }
                catch (Exception err)
                {
                    AddToLog("Ошибка: " + err.Message);
                }

                System.Threading.Thread.Sleep(longPoolingSleepInterval); // 5 sek 
            }
        }

        private WebResult PrintDataByParams(string registerDataId, int registerModel, JToken itemData, string eventName)
        {
            AAtol printer = null;
            FprnM8Class atolDriver = this.getDevice();
            WebResult result = new WebResult();

            if (atolDriver == null)
            {
                result.isError = true;
                result.lastErrorMessage = "Ошибка создания обьекта драйвера";

                return result;
            }

            if (registerModel > 0)
            {
                printer = new KKMAtol(itemData, ref atolDriver, eventName);
            }
            else
            {
                throw new Exception("bad register model");
            }

            if (printer == null)
            {
                result.isError = true;
                result.lastErrorMessage = "Ошибка создания обьекта принтера";

                return result;
            }

            try
            {
                KeyValuePair<bool, string> subres = printer.PrintData();

                result.data = registerDataId;
                result.isError = !subres.Key;
                result.lastErrorMessage = subres.Value;

                if (!result.isError && registerDataId != "0")
                {
                    AddToLog("Выполнено, ID = " + registerDataId);
                }
                else if (registerDataId != "0")
                {
                    AddToLog("Ошибка выполнения: " + result.lastErrorMessage);
                }
            }
            catch (Exception err)
            {
                result.data = registerDataId;
                result.isError = true;
                result.lastErrorMessage = "Ошибка обработки данных: " + err.Message;
            }
            finally
            {
                printer.Destructor();
                GC.SuppressFinalize(printer);
                GC.SuppressFinalize(atolDriver);

                printer = null;
                atolDriver = null;

                GC.Collect();
            }

            return result;
        }

        public void AddToLog(string t)
        {
            mainForm.AddToLog(t);
        }

        private bool isErrorResponse(string response)
        {
            return (Array.IndexOf(this.errorResponses, response) != -1);
        }
    }
}
