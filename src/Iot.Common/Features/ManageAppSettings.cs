using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;


namespace Iot.Common
{
    public class ManageAppSettings
    {
        public static NameValueCollection ReadAllSettings()
        {
            return ConfigurationManager.AppSettings;
        }

        public static string ReadSetting(string key)
        {
            string strRet = null;
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                
                strRet = appSettings[key];
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error reading app settings for key=" + key);
            }
            return strRet;
        }

        public static void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Console.WriteLine("Error writing app settings for key=[" + key + "] value=[" + value + "]" );
            }
        }
    }
}
