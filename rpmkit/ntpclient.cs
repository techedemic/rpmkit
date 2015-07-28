using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace rpmkit
{
    class ntpclient
    {
        public static DateTime GetNetworkTime(string ntpServer)
        {
            //default Windows time server
            //const string ntpServer = "10.1.1.10";
            //const string ntpServer = ntpServerIp.to;
            Util.writeLog("ntpclient.GetNetworkTime", "ntpServer: " + ntpServer, true);
            // NTP message size - 16 bytes of the digest (RFC 2030)
            var ntpData = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            //var addresses = Dns.GetHostEntry(ntpServer).AddressList;
            var addresses = Dns.GetHostAddresses(ntpServer);
            Util.writeLog("ntpclient.GetNetworkTime", "addresses: " + addresses.ToString(), true);

            //The UDP port number assigned to NTP is 123
            var ipEndPoint = new IPEndPoint(addresses[0], 123);
            //NTP uses UDP
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Util.writeLog("ntpclient.GetNetworkTime", "socket.Connect(ipEndPoint)", true);
            socket.Connect(ipEndPoint);

            //Stops code hang if NTP is blocked
            socket.ReceiveTimeout = 3000;
            Util.writeLog("ntpclient.GetNetworkTime", "socket.sent(ntpData)", true);
            socket.Send(ntpData);
            Util.writeLog("ntpclient.GetNetworkTime", "socket.Receive(ntpData)", true);
            socket.Receive(ntpData);
            Util.writeLog("ntpclient.GetNetworkTime", "socket.Close()", true);
            socket.Close();

            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
            //departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;

            //Get the seconds part
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
            Util.writeLog("ntpclient.GetNetworkTime", "Get the seconds part: " + intPart.ToString(), true);
            //Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);
            Util.writeLog("ntpclient.GetNetworkTime", "Get the seconds fraction: " + fractPart.ToString(), true);

            //Convert From big-endian to little-endian
            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            Util.writeLog("ntpclient.GetNetworkTime", "Convert From big-endian to little-endian :  [intPart]" + intPart.ToString() + "[fractPart]" + fractPart.ToString(), true);

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            //**UTC** time
            var networkDateTime = (new DateTime(1900, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds((long)milliseconds);
            Util.writeLog("ntpclient.GetNetworkTime", "Return value: " + networkDateTime.ToLocalTime().ToString(), true);
            return networkDateTime.ToLocalTime();
        }

        // stackoverflow.com/a/3294698/162671
        static uint SwapEndianness(ulong x)
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
    }
}
