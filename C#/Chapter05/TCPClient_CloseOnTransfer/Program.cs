using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TCPClient_CloseOnTransfer
{
    class Program
    {
        static string SERVERIP = "127.0.0.1";
        const int SERVERPORT = 9000;
        const int BUFSIZE = 512;

        static void Main(string[] args)
        {
            int retval;

            // 명령행 인수가 있으면 IP 주소로 사용
            if (args.Length > 0) SERVERIP = args[0];

            // 데이터 통신에 사용할 변수
            string[] testdata =
            {
                "안녕하세요",
                "반가워요",
                "오늘따라 할 이야기가 많을 것 같네요",
                "저도 그렇네요",
            };

            // 서버와 데이터 통신
            for (int i = 0; i < 4; i++)
            {
                Socket sock = null;
                try
                {
                    // 소켓 생성
                    sock = new Socket(AddressFamily.InterNetwork,
                        SocketType.Stream, ProtocolType.Tcp);

                    // Connect()
                    sock.Connect(SERVERIP, SERVERPORT);

                    // 데이터 입력(시뮬레이션)
                    byte[] buf = Encoding.Default.GetBytes(testdata[i]);
                    int size = buf.Length;
                    if (size > BUFSIZE) size = BUFSIZE;

                    // 데이터 보내기
                    retval = sock.Send(buf, 0, size, SocketFlags.None);
                    Console.WriteLine(
                        "[TCP 클라이언트] {0}바이트를 보냈습니다.", retval);

                    // 소켓 닫기
                    sock.Close();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    Environment.Exit(1);
                }
            }
        }
    }
}
