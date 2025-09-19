using System.Net;
using System.Net.Sockets;

namespace commonlib
{
    public static class IPAddressExtensions
    {
        public static IPAddress GetLocalIPAddress()
        {
            IPAddress local = null;
            IPAddress[] ips = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
            foreach (IPAddress ip in ips)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    local = ip;
                    break;
                }
            }
            return local;
        }
    }
}
