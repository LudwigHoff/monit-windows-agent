﻿using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace ChMonitoring.Helpers
{
    class ConfigMgr
    {
        public static MonitWindowsAgentConfig Config { get; private set; }

        public static void LoadConfig(string path, string configFileName, string serviceConfigFileNameMasks)
        {
            var configFilePathName = Path.Combine(path, configFileName);
            var ser = new XmlSerializer(typeof(MonitWindowsAgentConfig));

            if (!File.Exists(configFilePathName))
            {
                Config = WriteDefaultConfig(configFilePathName);
                return;
            }

            using (var str = new FileStream(configFilePathName, FileMode.Open, FileAccess.Read))
            {
                Config = ser.Deserialize(str) as MonitWindowsAgentConfig;
            }

            var serviceConfigFilePathNames = Directory.GetFiles(path, serviceConfigFileNameMasks);
            foreach (var serviceConfigFilePathName in serviceConfigFilePathNames)
            {
                using (var str = new FileStream(serviceConfigFilePathName, FileMode.Open, FileAccess.Read))
                {
                    var serviceConfig = ser.Deserialize(str) as MonitWindowsAgentConfig;
                    foreach (var service in serviceConfig.Services)
                    {
                        Config.Services.Add(service);
                    }
                }
            }

            // set period to ms
            if (Config.Period < 1000)
            {
                Config.Period *= 1000;
            }
        }

        public static MonitWindowsAgentConfig WriteDefaultConfig(string configFilePathName)
        {
            var conf = new MonitWindowsAgentConfig();
            var ser = new XmlSerializer(typeof(MonitWindowsAgentConfig));

            using (var str = new FileStream(configFilePathName, FileMode.CreateNew, FileAccess.ReadWrite))
            {
                // standard value 30 sec
                conf.Period = 30;
                conf.MMonitCollectorUrl = "http://monit:monit@localhost:8080/collect";
                conf.MMonitCollectorPassword = "password";
                conf.MMonitCollectorUsername = "username";
                conf.FailedStarts = 5;
                conf.DisplayName = "foo-{COMPUTER_NAME}-bar";
                conf.HttpdPort = 2812;
                conf.HttpdBindIp = "127.0.0.1";
                conf.HttpdPassword = "monit";
                conf.HttpdUsername = "admin";

                conf.Services = new List<Service>();
                conf.Services.Add(new Service()
                {
                    DependentServiceNames = new List<string> {"DEPENDENT_SERVICE_HERE"},
                    ServiceName = "YOUR_SERVICENAME_HERE"
                });
                conf.Services.Add(new Service()
                {
                    DependentServiceNames = new List<string> { "DEPENDENT_SERVICE_HERE" },
                    ServiceName = "YOUR_SERVICENAME_HERE"
                });
                ser.Serialize(str, conf);
            }

            return conf;
        }
    }
}
