using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Atol
{
    public abstract class AAtol
    {
        abstract public KeyValuePair<bool, string> PrintData();

        abstract public KeyValuePair<bool, string> PaymentRun(JToken data);

        abstract public KeyValuePair<bool, string> PrintXReport();

        abstract public KeyValuePair<bool, string> PrintSelectedReport(int selectedIndex);

        abstract public KeyValuePair<bool, string> PrintPeriodicReport(int typeIndex, string beginDate, string endDate);

        abstract public KeyValuePair<bool, string> SmenaStart(JToken data);

        abstract public KeyValuePair<bool, string> SmenaEnd(JToken data);

        abstract public void Destructor();
    }
}