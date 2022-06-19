using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace NameResolution
{
    class Program
    {
        const string TESTNAME = "www.google.com";

        // 도메인 이름 -> IPv4 주소
        static bool GetIPAddr(string name, ref IPAddress addr)
        {
            IPHostEntry hostent = null;
            try
            {
                hostent = Dns.GetHostEntry(name);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            addr = hostent.AddressList[0];
            return true;
        }

        // IPv4 주소 -> 도메인 이름
        static bool GetDomainName(IPAddress addr, ref string name)
        {
            IPHostEntry hostent = null;
            try
            {
                hostent = Dns.GetHostEntry(addr);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            name = hostent.HostName;
            return true;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("도메인 이름(변환 전) = {0}", TESTNAME);

            // 도메인 이름 -> IP 주소
            IPAddress addr = null;
            if (GetIPAddr(TESTNAME, ref addr))
            {
                // 성공이면 결과 출력
                Console.WriteLine("IP 주소(변환 후) = {0}", addr);

                // IP 주소 -> 도메인 이름
                string name = null;
                if (GetDomainName(addr, ref name))
                {
                    // 성공이면 결과 출력
                    Console.WriteLine("도메인 이름(다시 변환 후) = {0}", name);
                }
            }
        }
    }
}
