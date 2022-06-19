using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace UDPClient6
{
    class Program
    {
        static string SERVERIP = "::1";
        const int SERVERPORT = 9000;
        const int BUFSIZE = 512;

        static void Main(string[] args)
        {
            int retval;

            // 명령행 인수가 있으면 IP 주소로 사용
            if (args.Length > 0) SERVERIP = args[0];

            Socket sock = null;
            try
            {
                // 소켓 생성
                sock = new Socket(AddressFamily.InterNetworkV6,
                    SocketType.Dgram, ProtocolType.Udp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }

            // 소켓 주소 객체 초기화
            IPEndPoint serveraddr = new IPEndPoint(
                IPAddress.Parse(SERVERIP), SERVERPORT);

            // 데이터 통신에 사용할 변수
            byte[] buf = new byte[BUFSIZE];

            // 서버와 데이터 통신
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
                    Console.WriteLine(
                        "[UDP 클라이언트] {0}바이트를 보냈습니다.", retval);

                    // 데이터 받기
                    IPEndPoint anyaddr = new IPEndPoint(IPAddress.IPv6Any, 0);
                    EndPoint peeraddr = (EndPoint)anyaddr;
                    retval = sock.ReceiveFrom(buf, BUFSIZE,
                        SocketFlags.None, ref peeraddr);

                    // 송신자의 주소 체크
                    if (!((IPEndPoint)peeraddr).Equals(serveraddr))
                    {
                        Console.WriteLine("[오류] 잘못된 데이터입니다!");
                        break;
                    }

                    // 받은 데이터 출력
                    Console.WriteLine(
                        "[UDP 클라이언트] {0}바이트를 받았습니다.", retval);
                    Console.WriteLine("[받은 데이터] {0}",
                        Encoding.Default.GetString(buf, 0, retval));
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
