using System;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Configuration;

namespace rpmkit
{
    class Util
    {
        public static void writeRPMDatFile(int attr, string result)
        {
            // need to write to rpm_[attributenumber].dat
            // contents should be
            //    [attributenumber][char(1)][outputvalue]
            char delimiter = (char)1;
            // Read configuration from app settings file
            string fileLoc = ConfigurationManager.AppSettings.Get("PathToScript");
            writeLog("writeRPMDatFile", "Write to file '" + fileLoc + "rpm_" + attr.ToString() + ".dat' : " + result, false);
            // Write to file
            File.WriteAllText(fileLoc + "rpm_" + attr.ToString() + ".dat", attr.ToString() + delimiter.ToString() + result);
        }

        public static void writeLog(string funcName, string logMessage, bool isDebug)
        {
            string procName = Process.GetCurrentProcess().ProcessName;
            string fileLoc = ConfigurationManager.AppSettings.Get("PathToScript");
            string logLevel = ConfigurationManager.AppSettings.Get("LogLevel");
            DateTime logTime = DateTime.Now;
            string logLevelLogged;
            logLevelLogged = (isDebug == true) ? " [DEBUG] " : " [INFO] ";
            string convLogMessage = "\r\n" + logTime.ToString() + logLevelLogged + "Function Name: " + funcName + " - " + logMessage;
            if (logLevel.ToUpper() == "DEBUG")
            {
                // Write ANY log when log level is 'DEBUG'
                File.AppendAllText(fileLoc + "rpm_" + Program.g_AttrNum.ToString() + "_" + procName.ToString() + ".log", convLogMessage);
            }
            else if ((logLevel.ToUpper() == "INFO") && (isDebug == false))
            {
                // Write only 'INFO' logs when log level is 'INFO' 
                File.AppendAllText(fileLoc + "rpm_" + Program.g_AttrNum.ToString() + "_" + procName.ToString() + ".log", convLogMessage);
            }
            else
            // Would be the same as 
            // if ((logLevel.ToUpper() == "INFO") && (isDebug == true))
            {
                //do nothing
                return;
            }
        }
    }

}
