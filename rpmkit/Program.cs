using System;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Configuration;

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
                    case "randomNumber":
                        func_randomNumber(attributeNumber, int.Parse(args[2]), int.Parse(args[3]));
                        break;
                    case "compareTime":
                        func_compareTime(attributeNumber, args[2]);
                        break;
                    default:
                        Console.WriteLine("No 'functioname' provided or unable to locate function requested");
                        break;
                }
            }
            catch (Exception exc)
            {
                Util.writeLog("main", "EXCEPTION : " + exc.Message.ToString(), false);
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

        static void func_SqlDbaseTime(int attributeNumber)
        {
            try
            {
                Util.writeRPMDatFile(attributeNumber, "1");
            }
            catch (Exception exc)
            {
                Util.writeLog("func_SqlDbaseTime", "EXCEPTION : " + exc.Message.ToString(), false);
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
    }

}

