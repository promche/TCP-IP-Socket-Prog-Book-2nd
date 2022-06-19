using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace MulticastReceiver
{
    class Program
    {
        const string MULTICASTIP = "235.7.8.9";
        const int LOCALPORT = 9000;
        const int BUFSIZE = 512;

        static void Main(string[] args)
        {
            int retval;

            Socket sock = null;
            MulticastOption mcastOption = null;
            try
            {
                // 소켓 생성
                sock = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);

                // SO_REUSEADDR 옵션 설정
                sock.SetSocketOption(SocketOptionLevel.Socket,
                    SocketOptionName.ReuseAddress, true);

                // Bind()
                sock.Bind(new IPEndPoint(IPAddress.Any, LOCALPORT));

                // 멀티캐스트 그룹 가입
                mcastOption = new MulticastOption(
                    IPAddress.Parse(MULTICASTIP), IPAddress.Any);
                sock.SetSocketOption(SocketOptionLevel.IP,
                    SocketOptionName.AddMembership, mcastOption);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }

            // 데이터 통신에 사용할 변수
            byte[] buf = new byte[BUFSIZE];

            // 멀티캐스트 데이터 받기
            while (true)
            {
                try
                {
                    // 데이터 받기
                    IPEndPoint anyaddr = new IPEndPoint(IPAddress.Any, 0);
                    EndPoint peeraddr = (EndPoint)anyaddr;
                    retval = sock.ReceiveFrom(buf, BUFSIZE,
                        SocketFlags.None, ref peeraddr);

                    // 받은 데이터 출력
                    Console.WriteLine("[UDP/{0}:{1}] {2}",
                        ((IPEndPoint)peeraddr).Address,
                        ((IPEndPoint)peeraddr).Port,
                        Encoding.Default.GetString(buf, 0, retval));
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }

            // 멀티캐스트 그룹 탈퇴
            sock.SetSocketOption(SocketOptionLevel.IP,
                SocketOptionName.DropMembership, mcastOption);

            // 소켓 닫기
            sock.Close();
        }
    }
}
