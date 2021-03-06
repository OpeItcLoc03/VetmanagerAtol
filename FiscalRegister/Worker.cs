using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.IO.Ports;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FprnM1C;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Atol
{
    public class Worker
    {
        public bool isDone = false;
        public VMSettings settings = new VMSettings();
        public MainForm mainForm = null;
        private int RABBIT_RECCON_COUNT = 0;
        public int PRINTER_CHECK_INFO_COUNT = 180;
        private int checkConnectionInterval = 1000 * 20;   // 20 sec      
        public bool isConnectedToRegister = false;
        public bool isNeedIterrupt = false;
        public bool isNeedCheckRemoteVersion = false;
        public string rabbitExchange = "";
        public IConnection rabbitConnection;
        private string[] errorResponses = { "no_data", "wrong_clinic", "wrong_api", "wrong_params", "no_params", "need_update_local", "need_check_version_remote" };
                            
        public Worker()
        {
            isNeedIterrupt = false;
        }

        public bool CheckVetmanagerConnection()
        {
            string key = DateTime.Now.ToUniversalTime().GetHashCode().ToString();
            MyWebProxy wp = new MyWebProxy(settings.FullUrl + "?key=" + key, settings.ApiKey);
            WebResult res = wp.checkConnection();
            string logText = "Проверка соединения с " + settings.Url;

            wp = null;

            if (!res.isError && res.data != "")
            {
                if (res.data.IndexOf("connection_succesfull") != -1)
                {
                    AddToLog(logText + ": успешно");

                    string[] args = res.data.Split(':');

                    if (args.Length == 2)
                    {
                        Version.vmVersion = args[1].ToString();
                    }
                }
                else if (res.data.IndexOf("need_update_local:") != -1)
                {
                    string[] args = res.data.Split(':');

                    if (args.Length == 2)
                    {
                        Version.vmVersion = args[1].ToString();
                    }

                    res.data = "need_update_local";

                    this.TryLocalUpdate();
                }
                else if (res.data.IndexOf("need_check_version_remote:") != -1)
                {
                    string[] args = res.data.Split(':');

                    if (args.Length == 2)
                    {
                        Version.vmVersion = args[1].ToString();
                    }

                    res.data = "need_check_version_remote";

                    this.TryRemoteCheckUpdate();
                }
                
                return true;
            }

            AddToLog(logText + ": Ошибка соединения");
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

            if (!res.isError)
            {
                if (res.data.IndexOf("no_data:") != -1)
                {
                    AddToLog("Нет данных");

                    string[] args = res.data.Split(':');

                    if (args.Length == 2)
                    {
                        Version.vmVersion = args[1].ToString();
                    }

                    res.data = "no_data";
                }
                else if (res.data.IndexOf("need_update_local:") != -1)
                {
                    string[] args = res.data.Split(':');

                    if (args.Length == 2)
                    {
                        Version.vmVersion = args[1].ToString();
                    }

                    res.data = "need_update_local";

                    this.TryLocalUpdate();
                }
                else if (res.data.IndexOf("need_check_version_remote:") != -1)
                {
                    string[] args = res.data.Split(':');

                    if (args.Length == 2)
                    {
                        Version.vmVersion = args[1].ToString();
                    }

                    res.data = "need_check_version_remote";

                    this.TryRemoteCheckUpdate();
                }
                else 
                {
                    AddToLog("Получен ответ от сервера, размер данных: " + res.data.Length + " байт");
                }
            }
            else
            {
                AddToLog("Ошибка: " + res.lastErrorMessage);
            }

            return res;
        }

        private void TryLocalUpdate()
        {
            AddToLog("Проверка новой версии на локальном сервере");

            bool resp = VersionController.VersionChecker.IsNeedUpdateLocal(this.settings.FullUrl, this.settings.ApiKey);

            if (resp)
            {
                AddToLog("Закрытие програмы для обновления");
                System.Threading.Thread.Sleep(2000);
                this.mainForm.Close();
            }
        }

        private bool TryRemoteCheckUpdate()
        {
            AddToLog("Проверка новой версии на удаленном сервере");

            bool resp = VersionController.VersionChecker.IsNeedUpdateRemote(Version.vmVersion, Version.version);

            if (resp)
            {
                AddToLog("Закрытие програмы для обновления");
                System.Threading.Thread.Sleep(1000);
                this.mainForm.Close();
                return true;
            }

            return false;
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

                    //AddToLog("Создание обьекта драйвера: успешно");

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

            if (this.CheckVetmanagerConnection())
            {
                if (this.IsDeviceWorking())
                {
                    if (rabbitExchange == "")
                    {
                        rabbitExchange = this.getExchange();
                        AddToLog("Exchange получен: " + rabbitExchange);
                    }

                    this.InitRabbitConnection(rabbitExchange);
                    this.DoJob();
                }
                else
                {
                    AddToLog("Устройство не работает");
                    isError = true;
                }
            }
            else
            {
                AddToLog("Нет соединения с ветменеджер");
                isError = true;
            }

            if (isError)
            {
                System.Threading.Thread.Sleep(checkConnectionInterval);
                StartWork();
            }
        }

        private string getExchange()
        {
            string key = DateTime.Now.ToUniversalTime().GetHashCode().ToString();
            MyWebProxy wp = new MyWebProxy(settings.FullUrl + "?key=" + key, settings.ApiKey);
            AddToLog("Получение ключа rabbitMQ");
            WebResult res = wp.getExchange();

            wp = null;

            if (!res.isError && res.data != "wrong_params")
            {
                return res.data + ".onlinecassa";
            }

            return "";
        }

        private void InitRabbitConnection(string exchange)
        {
            if (exchange == "")
            {
                throw new Exception("no exchange set");
            }

            string nameAndPass = Encoding.UTF8.GetString(Convert.FromBase64String("dm1jbGllbnQ ="));
            string host = Encoding.UTF8.GetString(Convert.FromBase64String("NzguNDcuMjQ4LjEyNg =="));

            try
            {
                var factory = new ConnectionFactory()
                {
                    UserName = nameAndPass,
                    Password = nameAndPass,
                    VirtualHost = "/vm",
                    HostName = host,
                    Port = 5672,
                    RequestedHeartbeat = 20
                };

                rabbitConnection = factory.CreateConnection();
                var channel = rabbitConnection.CreateModel();
                var consumer = new EventingBasicConsumer(channel);

                consumer.Received += new BasicDeliverEventHandler(consumer_Received);
                channel.BasicConsume(exchange, true, consumer);
                //channel.BasicConsume("vm.gen.1d0258c2440a8d19e716292b231e3190:1.onlinecassa", true, consumer);
                AddToLog("Подключение к rabbit успешно");
            }
            catch (Exception err)
            {
                AddToLog("Ошибка создания подключения: " + err.Message);
            }            
        }

        void consumer_Received(IBasicConsumer sender, BasicDeliverEventArgs args)
        {
            var body = args.Body;
            var message = Encoding.UTF8.GetString(body);

            AddToLog("From rabbit: " + message);
            
            try
            {
                List<WebResult> results = new List<WebResult>();
                string jsonString = message.Replace('"', '\'');
                JObject dataString = JObject.Parse(jsonString);

                if (dataString != null)
                {
                    foreach (JObject item in dataString["data"])
                    {
                        if (settings.ApiKey == item["api_key"].ToString())
                        {
                            WebResult result = new WebResult();
                            result.data = null;
                            int runCount = 0;

                            while (true)
                            {
                                result = this.PrintDataByParams(
                                    item["id"].ToString()
                                    , int.Parse(this.settings.Device.Model)
                                    , item["data"]
                                    , item["event_name"].ToString()
                                    , runCount);

                                runCount++;

                                if (!result.isError)
                                {
                                    break;
                                }
                                else
                                {
                                    if (result.lastErrorMessage.IndexOf("Нет бумаги") != -1)
                                    {
                                        System.Threading.Thread.Sleep(10 * 1000);
                                        continue;
                                    }
                                    else
                                    {
                                        if (runCount < 3 && result.lastErrorMessage.IndexOf("Открыт чек продажи/покупки - операция невозможна") != -1)
                                        {
                                            continue;
                                        }
                                        else if (runCount < 3 && result.lastErrorMessage.IndexOf("Ошибка обработки данных: В экземпляре объекта не задана ссылка на объект") != -1)
                                        {
                                            continue;
                                        }
                                        else if (runCount > -1 && result.lastErrorMessage.IndexOf("Ошибка обработки данных: Необходимо сделать Z-отчет") != -1)
                                        {
                                            JObject subData = JObject.Parse("{userFIO: \"кассир\"}");
                                            WebResult subRes = this.PrintDataByParams("0", int.Parse(this.settings.Device.Model), subData, "smenaEnd", -2);

                                            if (subRes.lastErrorMessage != null && subRes.data != null && subRes.data == "" && subRes.isError == false)
                                            {
                                                continue;
                                            }
                                        }
                                    }

                                    break;
                                }
                            }

                            results.Add(result);
                        }
                        else
                        {
                            AddToLog("Ошибка: не совпадает API KEY !!!");
                        }
                    }
                }

                dataString = null;
                jsonString = null;

                if (results.Count > 0)
                {
                    this.SendResponseData(results);
                }
            }
            catch (Exception err)
            {
                AddToLog("Ошибка обработки данных: " + err.Message);
            }
        }

        private bool IsDeviceWorking()
        {
            if (this.settings.Device.Model == null)
            {
                AddToLog("Не указаны настройки принтера");
                return false;
            }

            if (PRINTER_CHECK_INFO_COUNT++ >= 180)  // раз в час
            {
                AddToLog("Проверка работы принтера");
                PRINTER_CHECK_INFO_COUNT = 0;

                WebResult res = this.PrintDataByParams("0", int.Parse(this.settings.Device.Model), null, "get_status", 0);

                return !res.isError;
            }

            return true;
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

                if (isNeedCheckRemoteVersion)
                {
                    AddToLog("Проверка новой версии по таймеру");

                    if (TryRemoteCheckUpdate())
                    {
                        return;
                    }

                    isNeedCheckRemoteVersion = false;
                }

                if (!this.IsDeviceWorking())
                {
                    System.Threading.Thread.Sleep(checkConnectionInterval); // 20 sek
                    DoJob();
                    return;
                }

                if (this.CheckVetmanagerConnection())
                {
                    if (rabbitExchange == "")
                    {
                        rabbitExchange = this.getExchange();
                        AddToLog("Exchange получен: " + rabbitExchange); 
                        this.InitRabbitConnection(rabbitExchange);
                    }
                }

                if (rabbitConnection == null || (rabbitConnection != null && !rabbitConnection.IsOpen))
                {
                    AddToLog("Заново стартуем подключение к rabbit");
                    if (rabbitExchange == "")
                    {
                        rabbitExchange = this.getExchange();
                        AddToLog("Exchange получен: " + rabbitExchange);
                    }

                    this.InitRabbitConnection(rabbitExchange);
                }

                if (RABBIT_RECCON_COUNT++ >= 180) // раз в час
                {
                    AddToLog("Переподключение к rabbit, раз в час");
                    RABBIT_RECCON_COUNT = 0;

                    if (rabbitConnection != null && rabbitConnection.IsOpen)
                    {
                        rabbitConnection.Close();
                    }

                    this.InitRabbitConnection(rabbitExchange);
                }

                System.Threading.Thread.Sleep(checkConnectionInterval); // 20 sek 
            }
        }

        private WebResult PrintDataByParams(string registerDataId, int registerModel, JToken itemData, string eventName, int runCount)
        {
            runCount++;
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

                AddToLog(result.lastErrorMessage + " stack: " + err.StackTrace.ToString());
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
            /*
            if (result.isError)
            {
                if (result.lastErrorMessage.IndexOf("Нет бумаги") != -1)
                {
                    System.Threading.Thread.Sleep(10 * 1000);
                    result = PrintDataByParams(registerDataId, registerModel, itemData, eventName, 0);
                } 
                else 
                {
                    WebResult subRes = new WebResult();
                    subRes.data = null;

                    if (runCount < 3 && result.lastErrorMessage.IndexOf("Открыт чек продажи/покупки - операция невозможна") != -1)
                    {
                        subRes = PrintDataByParams(registerDataId, registerModel, itemData, eventName, runCount);
                    }
                    else if (runCount < 3 && result.lastErrorMessage.IndexOf("Ошибка обработки данных: В экземпляре объекта не задана ссылка на объект") != -1)
                    {
                        subRes = PrintDataByParams(registerDataId, registerModel, itemData, eventName, runCount);
                    }
                    else if (runCount > -1 && result.lastErrorMessage.IndexOf("Ошибка обработки данных: Необходимо сделать Z-отчет") != -1)
                    {
                        JObject subData = JObject.Parse("{userFIO: \"кассир\"}");
                        subRes = this.PrintDataByParams("0", int.Parse(this.settings.Device.Model), subData, "smenaEnd", -2);
                    }

                    if (subRes.lastErrorMessage != null && subRes.data != null && subRes.data == "" && subRes.isError == false) 
                    {
                        result = PrintDataByParams(registerDataId, registerModel, itemData, eventName, 0);
                    }
                }                
            }
            */
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
