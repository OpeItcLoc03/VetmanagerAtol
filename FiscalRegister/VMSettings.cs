using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Atol
{
    public class VMSettings
    {
        private string apiKey = "";
        private string url = "";
        private bool startAfterRun = false;
        private AtolSettings device = new AtolSettings();
        
        public string FullUrl
        {
            get
            {
                return this.GetFullUrl(this.url);
            }
        }

        public string GetFullUrl(string u)
        {
            return u + "/fiscal_register.php"; 
        }

        [XmlElement]
        public string ApiKey
        {
            get { return apiKey; }
            set { apiKey = value; }
        }

        [XmlElement]
        public string Url
        {
            get { return url; }
            set { url = value; }
        }

        [XmlElement]
        public bool StartAfterRun
        {
            get { return startAfterRun; }
            set { startAfterRun = value; }
        }

        public AtolSettings Device
        {
            get { return device; }
            set { device = value; }
        }
    }
}
