using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Reflection;

namespace Atol
{
    public class AtolSettings
    {
        //private string deviceNumber = "";
        //private string deviceName = "";
        //private string machineName = "";
        //private string accessPassword = "";
        //private string writeLogFile = "";
        //private string hostAddress = "";
        //private string invertDrawerState = "";
        //private string model = "";
        //private string currentDeviceName = "";
        //private string useAccessPassword = "";
        //private string defaultPassword = "";
        //private string portNumber = "";
        //private string baudRate = "";

        //DeviceNumber0=1
        //DeviceName0=АТОЛ 11Ф, №00106707168906
        //MachineName0=
        //PortNumber0=1006
        //BaudRate0=3
        //Model0=67
        //AccessPassword0=
        //UseAccessPassword0=0
        //WriteLogFile0=0
        //DefaultPassword0=30
        //HostAddress0=192.168.10.1:5555
        //InvertDrawerState0=0


        public string DeviceNumber { get; set; }
        public string DeviceName { get; set; }
        public string MachineName { get; set; }
        public string AccessPassword { get; set; }
        public string WriteLogFile { get; set; }
        public string HostAddress { get; set; }
        public string InvertDrawerState { get; set; }
        public string Model { get; set; }
        public string CurrentDeviceName { get { return DeviceName; } }
        public string UseAccessPassword { get; set; }
        public string DefaultPassword { get; set; }
        public string PortNumber { get; set; }
        public string BaudRate { get; set; }
        public object this[string propertyName]
        {
            get
            {
                PropertyInfo property = GetType().GetProperty(propertyName);
                return property.GetValue(this, null);
            }
            set
            {
                PropertyInfo property = GetType().GetProperty(propertyName);
                property.SetValue(this, value, null);
            }
        }
    }
}
