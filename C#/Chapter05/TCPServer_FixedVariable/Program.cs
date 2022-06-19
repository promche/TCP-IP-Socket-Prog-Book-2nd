using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TCPServer_FixedVariable
{
    class Program
    {
        const int SERVERPORT = 9000;
        const int BUFSIZE = 512;

        static void Main(string[] args)
        {
            int retval;

            Socket listen_sock = null;
            try
            {
                // 소켓 생성
                listen_sock = new Socket(AddressFamily.InterNetwork,
                    SocketType.Stream, ProtocolType.Tcp);

                // Bind()
                listen_sock.Bind(new IPEndPoint(IPAddress.Any, SERVERPORT));

                // Listen()
                listen_sock.Listen(Int32.MaxValue);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Environment.Exit(1);
            }

            // 데이터 통신에 사용할 변수
            Socket client_sock = null;
            IPEndPoint clientaddr = null;
            int len; // 고정 길이 데이터
            byte[] buf = new byte[BUFSIZE]; // 가변 길이 데이터

            while (true)
            {
                try
                {
                    // accept()
                    client_sock = listen_sock.Accept();

                    // 접속한 클라이언트 정보 출력
                    clientaddr = (IPEndPoint)client_sock.RemoteEndPoint;
                    Console.WriteLine(
                        "\n[TCP 서버] 클라이언트 접속: IP 주소={0}, 포트 번호={1}",
                        clientaddr.Address, clientaddr.Port);

                    // 클라이언트와 데이터 통신
                    while (true)
                    {
                        // 데이터 받기(고정 길이)
                        retval = ReceiveN(client_sock, buf, 4, SocketFlags.None);
                        if (retval == 0) break;
                        len = BitConverter.ToInt32(buf, 0);

                        // 데이터 받기(가변 길이)
                        retval = ReceiveN(client_sock, buf, len, SocketFlags.None);
                        if (retval == 0) break;

                        // 받은 데이터 출력
                        Console.WriteLine("[TCP/{0}:{1}] {2}",
                            clientaddr.Address, clientaddr.Port,
                            Encoding.Default.GetString(buf, 0, retval));
                    }

                    // 소켓 닫기
                    client_sock.Close();
                    Console.WriteLine(
                        "[TCP 서버] 클라이언트 종료: IP 주소={0}, 포트 번호={1}",
                        clientaddr.Address, clientaddr.Port);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    break;
                }
            }

            // 소켓 닫기
            listen_sock.Close();
        }

        // C/C++, Python과 달리 데이터 수신 소켓 함수가 MSG_WAITALL
        // 플래그를 지원하지 않으므로 해당 기능을 직접 구현한다.
        static int ReceiveN(Socket sock, byte[] buf, int len, SocketFlags flags)
        {
            int received;
            int offset = 0;
            int left = len;

            while (left > 0)
            {
                try
                {
                    received = sock.Receive(buf, offset, left, flags);
                    if (received == 0) break;
                    left -= received;
                    offset += received;
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return len - left;
        }
    }
}
