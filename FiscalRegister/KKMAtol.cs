using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;
using FprnM1C;

namespace Atol
{
    class KKMAtol: AAtol
    {
        private JToken data = null;
        private string eventName = "";
        private int stringLineLength = 32;
        public FprnM8Class atolDriver = null;

        public KKMAtol(JToken o, ref FprnM8Class f, string e) 
        {
            this.data = o;
            this.eventName = e;
            this.atolDriver = f;
            this.stringLineLength = this.atolDriver.CharLineLength;     
        }

        ~KKMAtol()
        {
            this.Destructor();
        }
        
        public override void Destructor()
        {
            if (this.atolDriver != null)
            {
                this.atolDriver = null;
            }
        }

        private void appendText(ref StringBuilder sp)
        {
            sp.Append("Ошибка выполнения команды\r\n");
            //sp.Append("LastErrorText = " + atolDriver.LastErrorText + "\r\n");
            //if (atolDriver.LastErrorExText.Length > 0)
            //{
            //    sp.Append("LastErrorExText = " + atolDriver.LastErrorExText + "\r\n");
            //}
        }

        public override KeyValuePair<bool, string> PrintData()
        {
            // проверяем на всякий случай ККМ на фискализированность
         /*   if (atolDriver.Fiscal)
                if (System.Windows.Forms.MessageBox.Show("ККМ фискализирована! Вы действительно хотите продолжить?",
                        System.Windows.Forms.Application.ProductName,
                        System.Windows.Forms.MessageBoxButtons.YesNo,
                        System.Windows.Forms.MessageBoxIcon.Question) == System.Windows.Forms.DialogResult.No)
                    return new KeyValuePair<bool, string>(); 
          */ 

            // если есть открытый чек, то отменяем его
            if (atolDriver.CheckState != 0)
                if (atolDriver.CancelCheck() != 0)
                    return new KeyValuePair<bool, string>();

            KeyValuePair<bool, string> result = new KeyValuePair<bool, string>();
                        
            switch (this.eventName)
            {
                case "get_status":
                    result = this.GetPrinterStatus();
                    break;
                case "cashOutcome":
                    result = this.CashOutcome(data);
                    break;
                case "cashIncome":
                    result = this.CashIncome(data);
                    break;
                case "balance":
                    result = this.PrintBalanceOperation(data);
                    break;
                case "paymentRun":
                    result = this.PaymentRun(data);
                    break;
                case "smenaStart":
                    result = this.SmenaStart(data);
                    break;
                case "smenaEnd":
                    result = this.SmenaEnd(data);
                    break;
                case "customReport":
                    switch (data["report_type"].ToString())
                    {
                        case "get_unsended_docs":
                            result = this.PrintUnsendedDocs();
                            break;
                        case "register_settings":
                            result = this.GetRegisterPrintData();
                            break;
                        case "xreport":
                            result = this.PrintXReport();
                            break;
                        case "selected":
                            //{report_type: 'selected', report_id: value}
                            result = this.PrintSelectedReport(int.Parse(data["report_id"].ToString()));
                            break;
                        case "fpreport":
                            /*{
                                report_type: 'fpreport'
                                , report_id: value
                                , date_start: start // ДДММГГ
                                , date_end: end // ДДММГГ
                            }*/
                            //result = this.PrintPeriodicReport(int.Parse(data["report_id"].ToString()), data["date_start"].ToString(), data["date_end"].ToString());
                            break;
                    }
                    break;
            }

            return result;
        }

        private KeyValuePair<bool, string> PrintUnsendedDocs()
        {
            // // // // // // // // // // // // // // // // // // // // // // // // // // // // // //
            // ----------- Получение количества неотправленных документов и --------------//
            //------------------- даты самого старого неотправленного -------------------------//
            // // // // // // // // // // // // // // // // // // // // // // // // // // // // // //

            var textForPrint = "";
            atolDriver.Mode = 0;

            if (!this.SetModeAndStartDocument())
            {
                return returnError();
            }

            // Количество неотправленных документов:
            atolDriver.RegisterNumber = 44;
            if (atolDriver.GetRegister() != 0)
            {
                return returnError();
            }
            textForPrint += "Количество неотправленных документов: " + atolDriver.Count + "\n";

            // Дата самого старого неотправленного документа
            atolDriver.RegisterNumber = 45;
            if (atolDriver.GetRegister() != 0)
            {
                return returnError();
            }

            textForPrint += "Дата самого старого неотправленного документа: " + atolDriver.Day + "." + atolDriver.Month + "." + atolDriver.Year + " " + atolDriver.Hour + ":" + atolDriver.Minute;
            textForPrint += "\n\n\n\n";

            this.printCaption(textForPrint);

            return returnDone();
        }

        private KeyValuePair<bool, string> GetPrinterStatus()
        {
            // // // // // // // // // // // // // // // // // // // // // // // // // // // // // //
            // ------------------------- Отчёт о состоянии расчётов ------------------------------//
            // // // // // // // // // // // // // // // // // // // // // // // // // // // // // //

            // // // // // // // // // // // // // // // // // // // // // // // // // // // // // //
            // ----------- Получение состояния связи фискального накопителя --------------//
            // // // // // // // // // // // // // // // // // // // // // // // // // // // // // //

            var textForPrint = "";
            atolDriver.Mode = 0;
            int sumResult = 0;

            if (!this.SetModeAndStartDocument())
            {
                return returnError();
            }

            // Код ошибки сети
            atolDriver.RegisterNumber = 43;
            atolDriver.OFDLastError = 1;

            if (atolDriver.GetRegister() != 0)
            {
                return returnError();
            }

            sumResult += atolDriver.OFDLastError;
            textForPrint += "Код ошибки сети: " + atolDriver.OFDLastError + "\n";
            // Код ошибки ОФД
            atolDriver.RegisterNumber = 43;
            atolDriver.OFDLastError = 2;

            if (atolDriver.GetRegister() != 0)
            {
                return returnError();
            }

            sumResult += atolDriver.OFDLastError;
            textForPrint += "Код ошибки ОФД: " + atolDriver.OFDLastError + "\n";
            // Код ошибки ФН
            atolDriver.RegisterNumber = 43;
            atolDriver.OFDLastError = 3;

            if (atolDriver.GetRegister() != 0)
            {
                return returnError();
            }

            sumResult += atolDriver.OFDLastError;
            textForPrint += "Код ошибки ФН: " + atolDriver.OFDLastError;
            textForPrint += "\n\n\n\n";

            if (sumResult > 0)
            {
                this.printCaption("Есть ошибки!!!\n\n" + textForPrint);
            }

            return returnDone();
        }

        private KeyValuePair<bool, string> GetRegisterPrintData()
        {

            // // // // // // // // // // // // // // // // // // // // // // // // // // // // // //
            // ----------- Получение регистрационных данных ККТ ----------------------------//
            // // // // // // // // // // // // // // // // // // // // // // // // // // // // // //
            var textForPrint = "";
            atolDriver.Mode = 0;

            if (!this.SetModeAndStartDocument())
            {
                return returnError();
            }

            // Наименование пользователя
            atolDriver.AttrNumber = 1048;

            if (atolDriver.ReadAttribute() != 0)
            {
                return returnError();
            }

            textForPrint += "Наименование пользователя: " + atolDriver.AttrValue + "\n";

            // ИНН пользователя
            atolDriver.AttrNumber = 1018;

            if (atolDriver.ReadAttribute() != 0)
            {
                return returnError();
            }

            textForPrint += "ИНН пользователя: " + atolDriver.AttrValue + "\n";

            // Набор выбранных СНО в виде значения в десятичной системе исчисления, которое в двоичной соответствует битовой маске. Например 41 соответствует маске 101001 (ОСН 1, УСН доход 0, УСН доход-расход 0, ЕНВД 1, ЕСН 0, ПСН 1)
            atolDriver.AttrNumber = 1062;

            if (atolDriver.ReadAttribute() != 0)
            {
                return returnError();
            }

            textForPrint += "Выбранные СНО: " + atolDriver.AttrValue + "\n";

            // Регистрационный номер ККТ
            atolDriver.AttrNumber = 1037;

            if (atolDriver.ReadAttribute() != 0)
            {
                return returnError();
            }

            textForPrint += "Регистрационный номер ККТ: " + atolDriver.AttrValue + "\n";
            // аналогичным образом запрашиваются и остальные атрибуты

            // Номер фискального накопителя
            atolDriver.RegisterNumber = 47;

            if (atolDriver.GetRegister() != 0)
            {
                return returnError();
            }

            textForPrint += "Номер фискального накопителя: " + atolDriver.SerialNumber + "\n";

            // Номер фискального документа последней регистрации/перерегистрации, а также дата и время
            atolDriver.RegisterNumber = 48;

            if (atolDriver.GetRegister() != 0)
            {
                return returnError();
            }

            textForPrint += "Номер ФД последней регистрации/перерегистрации: " + atolDriver.DocNumber + "\n";
            textForPrint += "Дата ФД последней регистрации/перерегистрации: " + atolDriver.Day + "." + atolDriver.Month + "." + atolDriver.Year + " " + atolDriver.Hour + ":" + atolDriver.Minute + "\n";

            // Данные по последнему фискальному документу чека
            atolDriver.RegisterNumber = 51;

            if (atolDriver.GetRegister() != 0)
            {
                return returnError();
            }

            textForPrint += "Номер фискального документа: " + atolDriver.DocNumber + "\n";
            textForPrint += "Тип чека: " + atolDriver.LastCheckType + "\n";
            textForPrint += "Сумма чека: " + atolDriver.Summ + "\n";
            textForPrint += "Дата и время чека: " + atolDriver.Day + "." + atolDriver.Month + "." + atolDriver.Year + " " + atolDriver.Hour + ":" + atolDriver.Minute + "\n";
            textForPrint += "Фискальный признак документа: " + atolDriver.FiscalSign + "\n";

            // Данные по последнему фискальному документу
            atolDriver.RegisterNumber = 52;

            if (atolDriver.GetRegister() != 0)
            {
                return returnError();
            }

            textForPrint += "Номер фискального документа: " + atolDriver.DocNumber + "\n";
            textForPrint += "Тип чека: " + atolDriver.LastCheckType + "\n";
            textForPrint += "Сумма чека: " + atolDriver.Summ + "\n";
            textForPrint += "Дата и время чека: " + atolDriver.Day + "." + atolDriver.Month + "." + atolDriver.Year + " " + atolDriver.Hour + ":" + atolDriver.Minute + "\n";
            textForPrint += "Фискальный признак документа: " + atolDriver.FiscalSign + "\n";
            textForPrint += "\n\n\n\n";

            this.printCaption(textForPrint);

            return returnDone();
        }

        private KeyValuePair<bool, string> CashOutcome(JToken data)
        {
            /*
               // Войти в режим регистрации
 +//        Драйвер.Password = 30;
 +//        Драйвер.Mode = 1;
 +//        Драйвер.SetMode();
 +//        // Выплата
 +//        Драйвер.Summ = 100.00; // Сумма выплаты
 +//        Драйвер.CashOutcome(); // Выполнить выплату
             */
            atolDriver.Password = "30";
            atolDriver.Mode = 1;

            if (!this.SetModeAndStartDocument()) { return returnError(); }

            // снимаем отчет
            atolDriver.Summ = this.ParseEx(data["summ"].ToString());

            if (atolDriver.CashOutcome() != 0)
            {
                return returnError();
            }

            return returnDone();
        }

        private KeyValuePair<bool, string> CashIncome(JToken data)
        {
            /*
         // Войти в режим регистрации
         Драйвер.Password = 30;
         Драйвер.Mode = 1;
         Драйвер.SetMode();
         // Внесение
         Драйвер.Summ = 100.00; // Сумма внесения
         Драйвер.CashIncome(); // Выполнить внесение
 */
            atolDriver.Password = "30";
            atolDriver.Mode = 1;

            if (!this.SetModeAndStartDocument()) { return returnError(); }

            // снимаем отчет
            atolDriver.Summ = this.ParseEx(data["summ"].ToString());

            if (atolDriver.CashIncome() != 0)
            {
                return returnError();
            }

            return returnDone();
        }

        private KeyValuePair<bool, string> PrintBalanceOperation(JToken data)
        {
            /*
            {
              "registerId": 5,
              "taxSystem": 8,
              "taxTypeNumber": 4,
              "clientId": 15,
              "clinicId": 1,
              "clinicTitle": "Ваша клиника",
              "cassaId": 4,
              "org_name": "моя клиника",
              "org_inn": "0123456789",
              "cardType": "visa",
              "org_address": "клиника гдето там",
              "balanceIncomeOutcome": -80,
              "clientFio": "Горбунов Игорь ",
              "cassaUserId": 7,
              "cassaUserFio": "Капитан Виктория Олеговна",
              "cassaTitle": "касса Вика"
            }
            */
            StringBuilder sb = new StringBuilder();

            string orgName = data["org_name"].ToString();
            string clinicTitle = data["clinicTitle"].ToString();
            double balanceIncomeOutcome = this.ParseEx(data["balanceIncomeOutcome"].ToString());
            string taxSystem = data["taxSystem"].ToString();
            int taxTypeNumber = int.Parse(data["taxTypeNumber"].ToString());
            string orgInn = data["org_inn"].ToString();
            string orgAddress = data["org_address"].ToString();
            string clientFio = data["clientFio"].ToString();
            string cassaUserFio = data["cassaUserFio"].ToString();
            string cassaTitle = data["cassaTitle"].ToString();
            string cardType = data["cardType"].ToString();
            int realMoney = int.Parse(data["realMoney"].ToString());
            string goodTitle = "";

            atolDriver.DeviceEnabled = true;
            atolDriver.Mode = 1;

            if (!this.SetModeAndStartDocument())
            {
                return returnError();
            }
            
            atolDriver.AttrNumber = 1021;
            atolDriver.AttrValue = "Кассир: " + cassaUserFio;
            atolDriver.WriteAttribute();

            if (balanceIncomeOutcome > 0)
            {
                atolDriver.CheckType = 1;
                goodTitle = "Пополнение баланса";
            }
            else
            {
                atolDriver.CheckType = 4;
                goodTitle = "Снятие с баланса";
            }
            atolDriver.CheckMode = 1;
            if (atolDriver.OpenCheck() != 0)
            {
                return returnError();
            }

            atolDriver.AttrNumber = 1055;
            atolDriver.AttrValue = taxSystem;
            atolDriver.WriteAttribute();

            atolDriver.INN = orgInn;
             
            // Регистрация товара или услуги
            atolDriver.Name = goodTitle;
            atolDriver.Price = Math.Abs(balanceIncomeOutcome);
            atolDriver.Quantity = 1;
            atolDriver.Department = 0;
            atolDriver.TaxTypeNumber = taxTypeNumber;
            int res = atolDriver.Registration();

            //Всегда до фискализации ККМ и до снятия первого суточного отчета с гашением после фискализации ККМ
            //номер последней закрытой смены равен 0.
            //this.printCaption("Смена № " + atolDriver.Session.ToString());
            //this.printCaption("ИНН пользователя: " + orgInn);
            //this.printCaption("Организация: " + orgName);
           // this.printCaption("Адрес расчета: " + orgAddress);
           // this.printCaption("Чек № " + atolDriver.CheckNumber.ToString());

            if (cardType != "")
            {
                string tmpName = "Другая";

                switch (cardType)
                {
                    case "visa":
                        tmpName = "Виза";
                        break;
                    case "mastercard":
                        tmpName = "Мастеркард";
                        break;
                    default:
                        break;
                }

                this.printCaption("Оплачено картой:  " + tmpName);
            }

            // Оплата и закрытие чека
            // TypeClose - Тип оплаты:
            // 	0 - Наличными
            // 	1 - Электронными средствами платежа

            if (cardType != "")
            {
                atolDriver.TypeClose = 1;
                atolDriver.Summ = Math.Abs(balanceIncomeOutcome);
                res = atolDriver.Payment();
            }
            else
            {
                atolDriver.TypeClose = (realMoney == 1) ? 0 : 1;
                atolDriver.Summ = Math.Abs(balanceIncomeOutcome);
                res = atolDriver.Payment();
            }

            if (atolDriver.CheckPaperPresent == false)
            {

            }

            res = atolDriver.CloseCheck();

            return returnDone();            
        }

        public override KeyValuePair<bool, string> PaymentRun(JToken data)
        {
            /*
            {
              "registerId": 7,
              "invoicesOperations": [
                {
                  "payedCashSumm": 1664.8,
                  "payedCashlessSumm": 0,
                  "invoiceId": 731
                }
              ],
              "taxSystem": 8,
              "taxTypeNumber": 4,
              "clientId": 15,
              "sendByEmailOrPhone": "",
              "cardType": "",
              "clinicId": 1,
              "clinicTitle": "Ваша клиника",
              "org_name": "aefwef",
              "org_inn": "324234",
              "org_address": "wewef",
              "round": "1",
              "cassaId": 4,
              "cassaUserId": 20,
              "cassaUserFio": "Игорь Горбунов Александрович",
              "cassaTitle": "касса Вика",
              "total_amount": 1664.8,
              "isNightInvoice": 0,
              "isCallInvoice": 0,
              "createDate": "08.06.2017 13:25",
              "clientFio": "Горбунов Игорь ",
              "invoiceId": 731,
              "payedCashSumm": 1664.8,
              "payedCashlessSumm": 0,
              "goods": [
                {
                  "good_title": "aaaa (амп)",
                  "quantity": 2,
                  "cost": 20,
                  "price": 10,
                  "discount_cause": "Скидка: 0%, Карта: Скидка на услуги 30%, Тип карты: статичная",
                  "discount": 3,
                  "increase": 7,
                  "percent": 4
                },
                {
                  "good_title": "Биовак DPAL2 (доза)",
                  "quantity": 1,
                  "cost": 240,
                  "price": 240,
                  "discount_cause": "Скидка: 0%, Карта: Скидка на услуги 30%, Тип карты: статичная",
                  "discount": 3,
                  "increase": 7,
                  "percent": 4
                },
                {
                  "good_title": ",Ковенан р-р, 20мл (флакон)",
                  "quantity": 2,
                  "cost": 12,
                  "price": 6,
                  "discount_cause": "Скидка: 0%, Карта: Скидка на услуги 30%, Тип карты: статичная",
                  "discount": 3,
                  "increase": 7,
                  "percent": 4
                },
                {
                  "good_title": "Товар с наценкой 2 (Вагон)",
                  "quantity": 3,
                  "cost": 1332,
                  "price": 444,
                  "discount_cause": "Скидка: 0%, Карта: Скидка на услуги 30%, Тип карты: статичная",
                  "discount": 3,
                  "increase": 7,
                  "percent": 4
                }
              ],
              "description": "Счет №731"
            }
            */
            StringBuilder sb = new StringBuilder();

            string orgName = data["org_name"].ToString();
            int round = int.Parse(data["round"].ToString());
            string clinicTitle = data["clinicTitle"].ToString();
            string taxSystem = data["taxSystem"].ToString();
            int taxTypeNumber = int.Parse(data["taxTypeNumber"].ToString());
            string orgInn = data["org_inn"].ToString();
            string orgAddress = data["org_address"].ToString();
            string clientFio = data["clientFio"].ToString();
            string cassaUserFio = data["cassaUserFio"].ToString();
            string cassaTitle = data["cassaTitle"].ToString();
            string sendByEmailOrPhone = data["sendByEmailOrPhone"].ToString();
            string cardType = data["cardType"].ToString();
            bool isNightInvoice = (int.Parse(data["isNightInvoice"].ToString()) > 0);
            bool isCallInvoice = (int.Parse(data["isCallInvoice"].ToString()) > 0);
            string createDate = data["createDate"].ToString();
            string invoiceId = data["invoiceId"].ToString();
            double payedCashSumm = this.ParseEx(data["payedCashSumm"].ToString());
            double payedCashlessSumm = this.ParseEx(data["payedCashlessSumm"].ToString());
            string description = data["description"].ToString();
            double totalAmount = this.ParseEx(data["total_amount"].ToString());
            totalAmount = Math.Round(totalAmount, round);

            atolDriver.DeviceEnabled = true;
            atolDriver.Mode = 1;
  //atolDriver.TestMode = true;
            if (!this.SetModeAndStartDocument()) { return returnError(); }

            atolDriver.AttrNumber = 1021;
            atolDriver.AttrValue = "Кассир: " + cassaUserFio;
            atolDriver.WriteAttribute();

           // string textForPrint = "";
            // Наименование пользователя
            atolDriver.AttrNumber = 1048;
            atolDriver.AttrValue = orgName;
            atolDriver.WriteAttribute();
            //atolDriver.ReadAttribute();
           // textForPrint += "Наименование пользователя: " + atolDriver.AttrValue + "\n";

            // ИНН пользователя
            atolDriver.AttrNumber = 1018;
            atolDriver.AttrValue = orgInn;
            atolDriver.WriteAttribute();
           // atolDriver.ReadAttribute();
           // textForPrint += "ИНН пользователя: " + atolDriver.AttrValue + "\n";

           // this.printCaption(textForPrint);

            atolDriver.CheckType = 1;
            atolDriver.CheckMode = 1;
            if (atolDriver.OpenCheck() != 0) { return returnError(); }
            atolDriver.AttrNumber = 1055;
            atolDriver.AttrValue = taxSystem;
            atolDriver.WriteAttribute();
            atolDriver.INN = orgInn;

            if (sendByEmailOrPhone != "")
            {
                // Запись контакта покупателя для отправки электронного чека
                atolDriver.AttrNumber = 1008;
                atolDriver.AttrValue = sendByEmailOrPhone;
                atolDriver.WriteAttribute();
            }

            List<KeyValuePair<string, double>> goodResults = new List<KeyValuePair<string, double>>();
            double sumCost = 0;

            foreach (JObject good in data["goods"])
            {
                string goodTitle = good["good_title"].ToString();
                double quantity = this.ParseEx(good["quantity"].ToString());
                double price = this.ParseEx(good["price"].ToString());
                double cost = this.ParseEx(good["cost"].ToString());
                double discount = this.ParseEx(good["discount"].ToString());
                double increase = this.ParseEx(good["increase"].ToString());

                goodTitle += " (" + quantity + " x " + Math.Round(price, round) + ")";

                if (discount != 0 || increase != 0)
                {
                    if (increase > 0)
                    {
                        goodTitle += " надбавка:" + increase.ToString() + "%";
                        price = price + (price * increase / 100);
                        cost = quantity * price;
                    }

                    if (discount > 0)
                    {
                        goodTitle += " скидка:" + discount.ToString() + "%";
                        price = price - (price * discount / 100);
                        cost = quantity * price;
                    }
                }

                cost = Math.Round(cost, round);
                sumCost += cost;
                goodResults.Add(new KeyValuePair<string, double>(goodTitle, cost));
            }

            double diff = 0;

            if (sumCost != totalAmount) // если изза округления не совпала стоимость то пипец
            {
                           //1664.9  1664.8  
                diff = Math.Round((totalAmount - sumCost), round);// 0.1
            }

            for (int i = 0; i < goodResults.Count; i++)
            {
                atolDriver.Name = goodResults[i].Key;
                double price = goodResults[i].Value;

                if (diff > 0)
                {
                    price = Math.Round((price + diff), round);
                    diff = 0;
                }
                else if (diff < 0 && (price > diff || i == goodResults.Count - 1))
                {
                     price = Math.Round((price + diff), round);
                     diff = 0;
                }

                atolDriver.Price = price;
                atolDriver.Quantity = 1;
                atolDriver.Department = 0;
                atolDriver.TaxTypeNumber = taxTypeNumber;
                atolDriver.Registration();
            }

            //Всегда до фискализации ККМ и до снятия первого суточного отчета с гашением после фискализации ККМ
            //номер последней закрытой смены равен 0.
          //  this.printCaption("Смена № " + atolDriver.Session.ToString());
          //  this.printCaption("ИНН пользователя: " + orgInn);
          //  this.printCaption("Организация: " + orgName);
          //  this.printCaption("Адрес расчета: " + orgAddress);
            //   this.printCaption("Чек № " + atolDriver.CheckNumber.ToString());
            this.printCaption("Счет № " + invoiceId);
            this.printCaption("Клиент ФИО:  " + clientFio);

            if (cardType != "")
            {
                string tmpName = "Другая";

                switch (cardType)
                {
                    case "visa":
                        tmpName = "Виза";
                    break;
                    case "mastercard":
                    tmpName = "Мастеркард";
                    break;
                    default:
                        break;
                }

                this.printCaption("Оплачено картой:  " + tmpName);
            }

            if (isNightInvoice) { this.printCaption("Доп. инфо: Ночной счет"); }
            if (isCallInvoice) { this.printCaption("Доп. инфо: Вызов"); }

            // Оплата и закрытие чека
            // TypeClose - Тип оплаты:
            // 	0 - Наличными
            // 	1 - Электронными средствами платежа

            if (payedCashSumm > 0) 
            {
                atolDriver.TypeClose = 0;
                atolDriver.Summ = payedCashSumm;
                atolDriver.Payment();
            }

            if (payedCashlessSumm > 0) 
            {
                atolDriver.TypeClose = 1;
                atolDriver.Summ = payedCashlessSumm;
                atolDriver.Payment();
            }

            if (atolDriver.CheckPaperPresent == false)
            {

            }

            atolDriver.CloseCheck();

            return returnDone();


            ////////////////////////////
            /*
            string invoice_id = invoice["invoice_id"].ToString();
            string payed_user_fio = invoice["payed_user_fio"].ToString();

            string clinic_title = invoice["clinic_title"].ToString();
            double total = this.ParseEx(invoice["total_amount"].ToString());
            double totalPayed = this.ParseEx(invoice["payed_amount"].ToString());
            double diffPercent = this.ParseEx(invoice["diffPercent"].ToString());
            double invoiceIncrease = this.ParseEx(invoice["invoiceIncrease"].ToString());
            double invoiceDiscount = this.ParseEx(invoice["invoiceDiscount"].ToString());
            List<string> discountList = new List<string>();
            List<string> increaseList = new List<string>();

            string createDate = "";

            atolDriver.DeviceEnabled = true;
            atolDriver.Mode = 1;

            if (!this.SetModeAndStartDocument()) 
            {
                return returnError();
            }

            // tests
            //atolDriver.INN = "1234567890";
            int checkNum = atolDriver.CheckNumber;
            int ses = atolDriver.Session;
            string barcode = atolDriver.Barcode;
            bool paperPresent = atolDriver.CheckPaperPresent;
            int curDeviceIndex = atolDriver.CurrentDeviceIndex;
            string curDeviceName = atolDriver.CurrentDeviceName;
            int curDeviceNumber = atolDriver.CurrentDeviceNumber;
            int docNumber = atolDriver.DocNumber;
            bool fiscal = atolDriver.Fiscal;
            string fiscalSign = atolDriver.FiscalSign;
            bool fnFiscal = atolDriver.FNFiscal;
            int fnFlags = atolDriver.FNFlags;
            int barcodeState = atolDriver.GetBarcodeArrayState();
            string inn = atolDriver.INN;
            int ofdLastError = atolDriver.OFDLastError;
            int resPrintBarcode = atolDriver.PrintBarcode();
            //atolDriver.PrintBarcodeByNumber
            int barcodeText = atolDriver.PrintBarcodeText;
            int printBmap = atolDriver.PrintBitMap();
            int testDev = atolDriver.TestDevice();
            bool isSmenaOpened = atolDriver.SessionOpened;
          //  this.printCaption("Кассовый чек №" + atolDriver.CheckNumber.ToString() + " (приход)");

            // Записать должность и ФИО кассира
            atolDriver.AttrNumber = 1021;
            atolDriver.AttrValue = "Кассир: " + payed_user_fio;
            atolDriver.WriteAttribute();
           // atolDriver.OpenSession();

            // Запись контакта покупателя для отправки электронного чека
            atolDriver.AttrNumber = 1008;
           // atolDriver.AttrValue = "+380957939882";
            atolDriver.AttrValue = "mepata@yandex.ru";
            atolDriver.WriteAttribute();

            // CheckType - Тип чека:
            // 	1 - Приход
            // 	2 - Возврат прихода
            // 	4 - Расход
            // 	5 - Возврат расхода
            // 	7 - Коррекция прихода
            // 	9 - Коррекция расхода
            atolDriver.CheckType = 1;

            // CheckMode - Режим формирования чека:
            // 	0 - только в электронном виде без печати на чековой ленте
            // 	1 - печатать на чековой ленте
            atolDriver.CheckMode = 1;  
            if (atolDriver.OpenCheck() != 0)
            {
                return returnError();
            }

            atolDriver.AttrNumber = 1055;
            // Применяемая система налогооблажения в чеке:
            // 	ОСН - 1
            // 	УСН доход - 2
            // 	УСН доход-расход - 4
            // 	ЕНВД - 8
            // 	ЕСН - 16
            // 	ПСН - 32
            atolDriver.AttrValue = "8";
            atolDriver.WriteAttribute();

            this.printCaption("Счет № " + invoice_id);
            this.printCaption("Клиника: " + clinic_title);
            this.printCaption("Касса: " + cassa_title);
 
            this.printCaption("Товары/услуги:");
            
            // Регистрация товара или услуги
            foreach (JObject good in invoice["goods"])
            {
                string invoice_id2 = good["invoice_id"].ToString();
                string good_title = good["good_title"].ToString();

                if (createDate == "")
                {
                    createDate = good["create_date"].ToString();
                }

                string create_date = good["create_date"].ToString();                
                string discount_cause = good["discount_cause"].ToString();
                double quantity = this.ParseEx(good["quantity"].ToString());
                double price = this.ParseEx(good["price"].ToString());
                double calc_disc_incr = this.ParseEx(good["calc_disc_incr"].ToString());
                
                calc_disc_incr = Math.Round(calc_disc_incr, 2);
                quantity = Math.Round(quantity, 2);

                if (calc_disc_incr < 0)
                {
                    price -= Math.Abs(calc_disc_incr) / quantity;
                }
                else
                {
                    price += Math.Abs(calc_disc_incr) / quantity;
                }

                price = Math.Round(price, 2);

                atolDriver.Name = good_title;
                atolDriver.Price = price;
                atolDriver.Quantity = quantity;
                atolDriver.Department = 0;

                // TaxTypeNumber - Номер налога:
                // 	0 - Налог из секции
                // 	1 - НДС 0%
                // 	2 - НДС 10%
                // 	3 - НДС 18%
                // 	4 - НДС не облагается
                // 	5 - НДС с расчётной ставкой 10%
                // 	6 - НДС с расчётной ставкой 18%
                atolDriver.TaxTypeNumber = 4;
                // рекомендуется рассчитывать в кассовом ПО цену со скидкой, а информацию по начисленным скидкам 
                //печатать нефискальной печатью и не передавать скидку в ККМ, поэтому код для начисления скидки
                //закомментирован
                // driver.DiscountValue = 10;
                // // DiscountType - Тип скидки:
                // // 	0 - суммовая
                // // 	1 - процентная
                // driver.DiscountType = 0;

                atolDriver.Registration();
                if (calc_disc_incr < 0 && discountList.IndexOf("Тип скидки: " + discount_cause) == -1)
                {
                    discountList.Add("Тип скидки: " + discount_cause);
                }
                else if (calc_disc_incr > 0 && increaseList.IndexOf("Тип надбавки: " + discount_cause) == -1)
                {
                    increaseList.Add("Тип надбавки: " + discount_cause);
                }
            }
            

            // Отброс копеек (округление чека без распределения по позициям). Скидка на чек доступна только
            //для его округления до рубля. Таким образом недоступны: надбавки, назначение "на позицию"
            //, процентные значения.  SummCharge(), PercentsCharge(), PercentsDiscount () 
            //и ResetChargeDiscount () более недоступны
            // Destination - Назначение скидки:
            // 	0 - на чек
            // 	1 - на позицию (недоступно)
            //atolDriver.Destination = 0;
            //atolDriver.Summ = diffPercent;
            //atolDriver.SummDiscount();
            

            // Нефискальная печать с информацией по скидкам чека

            if (discountList.Count > 0 || increaseList.Count > 0)
            {
                this.printCaption("Скидки/надбавки на отдельные товары:");

                foreach (string discountText in discountList)
                {
                    this.printCaption(discountText);
                }

                foreach (string increaceText in increaseList)
                {
                    this.printCaption(increaceText);
                }
            }

            //Всегда до фискализации ККМ и до снятия первого суточного отчета с гашением после фискализации ККМ
            //номер последней закрытой смены равен 0.
            this.printCaption("Смена № " + atolDriver.Session.ToString());
            this.printCaption("ИНН пользователя: " + "1234567890123");
            this.printCaption("Пользователь: " + "На бабушкина");
            this.printCaption("Адрес расчета: " + "г. Краснодар, ул Трататата 5а");
            this.printCaption("Чек № " + atolDriver.CheckNumber.ToString());
            this.printCaption("ККТ № " + atolDriver.MachineNumber);
           // this.printCaption("ФН № " + atolDriver.SerialNumber);
            this.printCaption("is fiscal: " + atolDriver.Fiscal.ToString());

            if (isNightInvoice)
            {
                this.printCaption("Доп. инфо: Ночной счет");
            }
            if (isCallInvoice)
            {
                this.printCaption("Доп. инфо: Вызов");
            }

            //this.printCaption("Дата чека: " + createDate);

            if (invoiceDiscount > 0)
            {
                this.printCaption("Скидка по чеку: " + invoiceDiscount + " %");
            }
            if (invoiceIncrease > 0)
	        {
                this.printCaption("Надбавка по чеку: " + invoiceIncrease + " %");
	        }

            this.printCaption("Оплачено: " + totalPayed);
            
         
            // Оплата и закрытие чека
            // TypeClose - Тип оплаты:
            // 	0 - Наличными
            // 	1 - Электронными средствами платежа
            // 	3 - предоплата
            atolDriver.TypeClose = 0;
            //atolDriver.Summ = total;
            atolDriver.Summ = total;

            if (atolDriver.CheckPaperPresent == false)
            {

            }

            atolDriver.Payment();


          //  atolDriver.TypeClose = 0;
            //atolDriver.Summ = total;
          //  atolDriver.Summ = total - 50;

          //  atolDriver.Payment();
            
            atolDriver.BarcodeType = 84;
            atolDriver.Barcode = "www.nalog.ru/t=20170510T163800&s=22.00&fn=8710000100396057&i=10&fp=3904538606&n=1";
            atolDriver.Alignment = 1;
            atolDriver.Scale = 800;
           // atolDriver.AutoSize = true;
            int barRes = atolDriver.PrintBarcode();
            atolDriver.Barcode = "";
            
            //atolDriver.GetRegister();
            atolDriver.CloseCheck();
           
            if (barRes != 0)
            {
                return returnError();
            }
            

            return returnDone();
            */
        }

        private void printCaption(string str)
        {
            for (var i = 0; i < str.Length; i += this.stringLineLength)
            {
                string res = str.Substring(i, Math.Min(this.stringLineLength, str.Length - i));

                atolDriver.Caption = res;
                atolDriver.PrintString();
            }
        }

        public double ParseEx(string str)
        {
            double value;

            return
                double.TryParse(str.Replace(",", "."), out value)
                ? value
                : double.Parse(str.Replace(".", ","));
        }

        public override KeyValuePair<bool, string> PrintXReport()
        {
            atolDriver.NewDocument();
            atolDriver.Password = "30";
            // входим в режим отчетов без гашения
            atolDriver.Mode = 2;
            atolDriver.DeviceEnabled = true;

            if (!this.SetModeAndStartDocument())
            {
                return returnError();
            }

            // снимаем отчет
            atolDriver.ReportType = 2;

            if (atolDriver.Report() != 0)
            {
                return returnError();
            }

            return returnDone();
        }

        public override KeyValuePair<bool, string> PrintSelectedReport(int selectedIndex)
        {
            atolDriver.Mode = getModeByReportType(selectedIndex);
            atolDriver.Password = "30";

            if (!this.SetModeAndStartDocument())
            {
                return returnError();
            }

            atolDriver.ReportType = selectedIndex;

            if (atolDriver.Report() != 0)
            {
                return returnError();
            }

            return returnDone();
        }
        
        public override KeyValuePair<bool, string> PrintPeriodicReport(int typeIndex, string beginDate, string endDate)
        {
            // "report_id":3,"date_start":"050417","date_end":"290417"

            atolDriver.Mode = getModeByReportType(typeIndex);
            //atolDriver.Password = "30";

            if (!this.SetModeAndStartDocument())
            {
                return returnError();
            }

            atolDriver.ReportType = typeIndex;

            atolDriver.Day = int.Parse(beginDate.Substring(0, 2));
            atolDriver.Month = int.Parse(beginDate.Substring(2, 2));
            atolDriver.Year = int.Parse("20" + beginDate.Substring(4, 2));

            atolDriver.EndDay = int.Parse(endDate.Substring(0, 2));
            atolDriver.EndMonth = int.Parse(endDate.Substring(2, 2));
            atolDriver.EndYear = int.Parse("20" + endDate.Substring(4, 2));

            if (atolDriver.Report() != 0)
            {
                return returnError();
            }

            return returnDone();
        }

        public override KeyValuePair<bool, string> SmenaStart(JToken data)
        {
            int status = atolDriver.GetStatus();
            if (atolDriver.SessionOpened)
            {
                return returnError();
            }

            atolDriver.Mode = 1;

            if (!this.SetModeAndStartDocument())
            {
                return returnError();
            }
 
            atolDriver.AttrNumber = 1021;
            atolDriver.AttrValue = data["userFIO"].ToString();//"Старший кассир Иванов И.И.";
            atolDriver.WriteAttribute();

            if (atolDriver.OpenSession() != 0)
            {
                return returnError();
            }

            return returnDone();
        }

        public override KeyValuePair<bool, string> SmenaEnd(JToken data)
        {
            atolDriver.Mode = 3;

            if (!this.SetModeAndStartDocument())
            {
                return returnError();
            }

            atolDriver.AttrNumber = 1021;
            atolDriver.AttrValue = data["userFIO"].ToString();//"Старший кассир Иванов И.И.";
            atolDriver.WriteAttribute();
            atolDriver.ReportType = 1;

            if (atolDriver.Report() != 0)
            {
                return returnError();
            }

            return returnDone();
        }

        private KeyValuePair<bool, string> returnDone()
        {
            if (atolDriver.ResetMode() != 0)
            {
                return returnError();
            }

            //atolDriver = null;
            return new KeyValuePair<bool, string>(true, "");
        }

        private bool SetModeAndStartDocument()
        {
            if (atolDriver.NewDocument() != 0)
            {
                return false;
            }
            if (atolDriver.SetMode() != 0)
            {
                return false;
            }

            return true;
        }

        private KeyValuePair<bool, string> returnError()
        {
            string errorText = atolDriver.ResultDescription;

            if (atolDriver.ResultCode == 3822 * (-1))
            {
                throw new Exception("Необходимо сделать Z-отчет, " + errorText);
            }

            int res = atolDriver.ResultCode;

            if ("Ошибок нет" == errorText)
            {
                bool isPaper = atolDriver.CheckPaperPresent;
                int stat = atolDriver.GetStatus();
                bool outOfPaper = atolDriver.OutOfPaper;

            }

            atolDriver.EndDocument();
            atolDriver.ResetMode();
            atolDriver = null;
            return new KeyValuePair<bool, string>(false, errorText);
        }

        private int getModeByReportType(int reportType)
        {
            /*
            2.0 
            Режим снятия отчетов без гашения. 
            ReportType = 2,7,8,9 … 11 

            3.0 
            Режим снятия отчетов с гашением. 
            ReportType = 0,1,34 … 36, 42 

            5.0 
            Режим доступа к ФП. 
            ReportType = 3 … 6 

            6.0 
            Режим доступа к ЭКЛЗ. 
            ReportType = 22 … 33
             
            5 3 26 27 28 29
            7 8 9 10 11 22 37 38 39 40 41 42
            */
            int result = 0;

            switch (reportType)
            {
                case 2:
                case 7:
                case 8:
                case 9:
                case 10:
                case 11:
                    result = 2;
                    break;
                case 0:
                case 1:
                case 34:
                case 35:
                case 36:
                case 42:
                    result = 3;
                    break;
                case 3:
                case 4:
                case 5:
                case 6:
                    result = 5;
                    break;
                case 22:
                case 23:
                case 24:
                case 25:
                case 26:
                case 27:
                case 28:
                case 29:
                case 30:
                case 31:
                case 32:
                case 33:
                    result = 6;
                    break;

                default:
                    throw new Exception("bad report type: cant find mode for this report");
            }

            return result;
        }
    }
}
