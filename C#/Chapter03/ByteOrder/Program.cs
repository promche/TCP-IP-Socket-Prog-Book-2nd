using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ByteOrder
{
    class Program
    {
        static void Main(string[] args)
        {
            short x1 = 0x1234;
            int y1 = 0x12345678;
            short x2;
            int y2;

            // 호스트 바이트 -> 네트워크 바이트
            Console.WriteLine("[호스트 바이트 -> 네트워크 바이트]");
            x2 = IPAddress.HostToNetworkOrder(x1);
            Console.WriteLine("0x{0:x} -> 0x{1:x}", x1, x2);
            y2 = IPAddress.HostToNetworkOrder(y1);
            Console.WriteLine("0x{0:x} -> 0x{1:x}", y1, y2);

            // 네트워크 바이트 -> 호스트 바이트
            Console.WriteLine("\n[네트워크 바이트 -> 호스트 바이트]");
            Console.WriteLine("0x{0:x} -> 0x{1:x}", x2,
                IPAddress.NetworkToHostOrder(x2));
            Console.WriteLine("0x{0:x} -> 0x{1:x}", y2,
                IPAddress.NetworkToHostOrder(y2));

            // 잘못된 사용 예
            Console.WriteLine("\n[잘못된 사용 예]");
            Console.WriteLine("0x{0:x} -> 0x{1:x}", x1,
                IPAddress.NetworkToHostOrder((int)x1));
        }
    }
}
