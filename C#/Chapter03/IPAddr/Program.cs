using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace IPAddr
{
    class Program
    {
        static void Main(string[] args)
        {
            /*----------------*/
            /* IPv4 변환 연습 */
            /*----------------*/
            // 원래의 IPv4 주소 출력
            string ipv4test = "147.46.114.70";
            Console.WriteLine("IPv4 주소(변환 전) = {0}", ipv4test);

            // Parse() 함수 연습
            IPAddress ipv4num = IPAddress.Parse(ipv4test);
            byte[] bytes1 = ipv4num.GetAddressBytes();
            Console.WriteLine("IPv4 주소(변환 후) = 0x{0:x}",
                BitConverter.ToInt32(bytes1, 0));

            // Implicit ToString() 함수 연습
            Console.WriteLine("IPv4 주소(다시 변환 후) = {0}\n", ipv4num);

            /*----------------*/
            /* IPv6 변환 연습 */
            /*----------------*/
            // 원래의 IPv6 주소 출력
            string ipv6test = "2001:0230:abcd:ffab:0023:eb00:ffff:1111";
            Console.WriteLine("IPv6 주소(변환 전) = {0}", ipv6test);

            // Parse() 함수 연습
            IPAddress ipv6num = IPAddress.Parse(ipv6test);
            Console.Write("IPv6 주소(변환 후) = 0x");
            byte[] bytes2 = ipv6num.GetAddressBytes();
            for (int i = 0; i < bytes2.Length; i++)
                Console.Write("{0:x}", bytes2[i]);
            Console.WriteLine();

            // Implicit ToString() 함수 연습
            Console.WriteLine("IPv6 주소(다시 변환 후) = {0}\n", ipv6num);
        }
    }
}
