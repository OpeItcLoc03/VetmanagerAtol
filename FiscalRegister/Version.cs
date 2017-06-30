using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Atol
{
    public class Version
    {
        private const string VM_VERSION_FILE = "version.vm";

        private static string vm_version = "";

        public static string version
        {
            get { return "1.1.0"; }
        }

        public static string vmVersion
        {
            get
            {
                if (vm_version == "")
                {
                   // vmVersion = "0.123.46"; //TODO: remove
                    vm_version = ReadVmVersionFromFile();
                }

                return vm_version;
            }

            set
            {
                if (value != vm_version)
                {
                    vm_version = value;
                   // vmVersion = "0.123.46"; //TODO: remove
                    File.WriteAllText(VM_VERSION_FILE, vm_version);
                }
            }
        }

        public static string ReadVmVersionFromFile()
        {
            try
            {
                vm_version = File.ReadAllText(VM_VERSION_FILE);
            }
            catch { }

            return vm_version;
        }
    }
}
