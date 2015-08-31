using System;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace rpmkit
{
    class Program
    {
        public static int g_AttrNum = 0;

        static void Main(string[] args)
        {
            try
            {


                if (args.Length == 0)
                {
                    string procName = Process.GetCurrentProcess().ProcessName;
                    Console.WriteLine("No arguments were provided");
                    Console.WriteLine("Usage: ");
                    Console.WriteLine(procName + " [attribute number] [functionName] [function parameter 1] [function parameter 2] ... ");
                    Console.WriteLine("Example:");
                    Console.WriteLine(procName + " 12345 randomNumber 1 100");
                    Console.WriteLine(" *** This will generate a random number between 1 and 100 and write it to 14504.dat");
                    return;
                }

                int attributeNumber = int.Parse(args[0]);
                Program.g_AttrNum = attributeNumber;
                string rpmFunction = args[1];
                Util.writeLog("main", "rpmFunction : " + rpmFunction, false);

                switch (rpmFunction)
                {
                    case "readFile":
                        func_readFile(attributeNumber, args[2]);
                        break;
                    case "randomNumber":
                        func_randomNumber(attributeNumber, int.Parse(args[2]), int.Parse(args[3]));
                        break;
                    case "compareTime":
                        func_compareTime(attributeNumber, args[2]);
                        break;
                    //case "BCX_PS_MailFlow":
                    //    func_BCX_PS_MailFlow(attributeNumber, args[2], args[3], args[4], args[5]);
                    //    break;
                    default:
                        Console.WriteLine("No 'functioname' provided or unable to locate function requested");
                        break;
                }
            }
            catch (Exception exc)
            {
                Util.writeLog("main", "EXCEPTION : " + exc.Message.ToString(), false);
                Util.writeRPMDatFile(int.Parse(args[0]), "EXCEPTION : " + exc.Message.ToString());
                Console.WriteLine(exc.Message.ToString());
            }
        }

        /// <summary>
        /// The func_readFile() function will take two arguments
        /// attributeNumber (int) - The value of the attribute number which should be passed through by the main method
        /// fileName (string) - The name of the file, normally in 'C:\sintelligent\RPMScripts\Results\', that will hold the value to be plotted. 
        /// </summary>

        static void func_readFile(int attributeNumber, string fileName)
        {
            try
            {
                Util.writeLog("func_readFile", "Read from file : " + fileName, true);
                string text = File.ReadAllText(@"results/" + fileName); // Read value from results folder - fileName is the file to use. It should contain nothing but the value that should be returned. 
                Util.writeRPMDatFile(attributeNumber, text.Trim());
                Util.writeLog("func_readFile", "Wrote to .dat. Value : " + text.Trim(), true);
            }
            catch (FileNotFoundException exc)           // If the file provided as per the parameter is not found
            {
                Util.writeLog("func_readFile", "EXCEPTION : File not found - Ensure you are referencing the correct file", false);
                Util.writeLog("func_readFile", "EXCEPTION : " + exc.Message.ToString(), false);
                Console.WriteLine(exc.Message.ToString());
            }
            catch (DirectoryNotFoundException exc)      // Normally occurs if the 'results' directory does not exist.
            {
                Util.writeLog("func_readFile", "EXCEPTION : Directory not found - Ensure you have a directory names 'results' in your RPM script location.", false);
                Util.writeLog("func_readFile", "EXCEPTION : " + exc.Message.ToString(), false);
            }
            catch (Exception exc)                       // Any other exceptions
            {
                Util.writeLog("func_readFile", "EXCEPTION : " + exc.Message.ToString(), false);
                Console.WriteLine(exc.Message.ToString());
            }
        }

        

        /// <summary>
        /// The func_compareTime() function will take two arguments
        /// attributeNumber (int) - The value of the attribute number which should be passed through by the main method
        /// ntpServer (string) - Either an IP Address or Hostname of an NTP server which is accessible from the host where the script is running
        /// </summary>

        static void func_compareTime(int attributeNumber, string ntpServer)
        {
            try
            {
                // Get NTP server time                
                var ntpTime = ntpclient.GetNetworkTime(ntpServer).ToUniversalTime();

                // Get the local time of the host
                var localTime = DateTime.UtcNow;
                Util.writeLog("func_compareTime", "ntpTime:   " + ntpTime.Hour + ":" + ntpTime.Minute + ":" + ntpTime.Minute + ":" + ntpTime.Second + ":" + ntpTime.Millisecond, true);
                Util.writeLog("func_compareTime", "localTime: " + localTime.Hour + ":" + localTime.Minute + ":" + localTime.Minute + ":" + localTime.Second + ":" + localTime.Millisecond, true);

                // Calculate the difference between the two times
                var difference = (localTime - ntpTime);
                // Convert to type 'long' in order to remove 'micro' seconds 
                long ms = (long)(difference.TotalMilliseconds);
                // Convert negative values to positive values. We don't care about the '-' because we just need to know the difference between the two values
                ms = System.Math.Abs(ms);
                Util.writeLog("func_compareTime", "difference: " + ms.ToString(), false);
                // Write to .dat file
                Util.writeRPMDatFile(attributeNumber, ms.ToString());

            }
            catch (Exception exc)
            {
                Util.writeLog("func_compareTime", "EXCEPTION : " + exc.Message.ToString(), false);
                Console.WriteLine(exc.Message.ToString());
            }
        }

        static void func_randomNumber(int attributeNumber, int param1, int param2)
        {
            int startNumber = param1;
            Util.writeLog("func_randomNumber", "startNumber = " + param1, true);
            int endNumber = param2;
            Util.writeLog("func_randomNumber", "endNumber = " + param2, true);
            Random random = new Random();
            int randomNumber = random.Next(startNumber, endNumber);
            Util.writeLog("func_randomNumber", "randomNumber = " + randomNumber, true);
            Util.writeRPMDatFile(attributeNumber, randomNumber.ToString());
        }

       /* static void func_BCX_PS_MailFlow(int attributeNumber, string serverName, string TargetEmail, string ScriptName, string fieldCheck)
        {
            try
            {
                RunspaceConfiguration runspaceConfiguration = RunspaceConfiguration.Create();

                Runspace runspace = RunspaceFactory.CreateRunspace(runspaceConfiguration);
                runspace.Open();

                RunspaceInvoke scriptInvoker = new RunspaceInvoke(runspace);
                scriptInvoker.Invoke("Set-ExecutionPolicy Unrestricted");

                Pipeline pipeline = runspace.CreatePipeline();

                //Here's how you add a new script with arguments
                Command myCommand = new Command(ScriptName);
                CommandParameter servername = new CommandParameter(serverName);
                myCommand.Parameters.Add(servername);
                CommandParameter targetEmail = new CommandParameter("TargetEmailAddress", TargetEmail);
                myCommand.Parameters.Add(targetEmail);

                pipeline.Commands.Add(myCommand);

                // Execute PowerShell script
                Util.writeLog("func_BCX_PS_MailFlow", "Run Script : " + ScriptName, false);
                var results = pipeline.Invoke();
                foreach (var item in results)
                {
                    string value = item.ToString();
                    if (value.Contains(fieldCheck))
                    {
                        Util.writeLog("func_BCX_PS_MailFlow", "Found field : " + fieldCheck, true);
                        string[] arrValue = value.Split(':');
                        Util.writeLog("func_BCX_PS_MailFlow", "Field value : " + arrValue[1].Trim(), true);
                        Util.writeRPMDatFile(attributeNumber, arrValue[1].Trim());
                    }
                }
                runspace.Close();
            }
            catch (Exception exc)
            {
                Util.writeLog("func_BCX_PS_MailFlow", "EXCEPTION : " + exc.Message.ToString(), false);
                Console.WriteLine(exc.Message.ToString());
            }
        }*/
    }

}

