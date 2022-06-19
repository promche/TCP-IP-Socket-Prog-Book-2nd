using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace MulticastSender
{
    class Program
    {
        static string MULTICASTIP = "235.7.8.9";
        const int REMOTEPORT = 9000;
        const int BUFSIZE = 512;

        static void Main(string[] args)
        {
            int retval;

            // 명령행 인수가 있으면 IP 주소로 사용
            if (args.Length > 0) MULTICASTIP = args[0];

            Socket sock = null;
            try
            {
                // 소켓 생성
                sock = new Socket(AddressFamily.InterNetwork,
                    SocketType.Dgram, ProtocolType.Udp);

                // 멀티캐스트 TTL 설정
                int ttl = 2;
                sock.SetSocketOption(SocketOptionLevel.IP,
                    SocketOptionName.MulticastTimeToLive, ttl);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }

            // 소켓 주소 객체 초기화
            IPEndPoint serveraddr = new IPEndPoint(
                IPAddress.Parse(MULTICASTIP), REMOTEPORT);

            // 데이터 통신에 사용할 변수
            byte[] buf = new byte[BUFSIZE];

            // 멀티캐스트 데이터 보내기
            while (true)
            {
                // 데이터 입력
                Console.Write("\n[보낼 데이터] ");
                string data = Console.ReadLine();
                if (data.Length == 0) break;

                try
                {
                    // 데이터 보내기 (최대 길이를 BUFSIZE로 제한)
                    byte[] senddata = Encoding.Default.GetBytes(data);
                    int size = senddata.Length;
                    if (size > BUFSIZE) size = BUFSIZE;
                    retval = sock.SendTo(senddata, 0, size,
                        SocketFlags.None, serveraddr);
                    Console.WriteLine("[UDP] {0}바이트를 보냈습니다.", retval);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }

            // 소켓 닫기
            sock.Close();
        }
    }
}
