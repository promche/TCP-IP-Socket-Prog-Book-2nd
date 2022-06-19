using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TCPServer_Variable
{
    class LineRead
    {
        int nbytes = 0;
        byte[] buf = new byte[1024];
        int ptr = -1;

        // 내부 구현용 함수
        int _recv_ahead(Socket s, byte[] p)
        {
            if (nbytes == 0 || nbytes == -1)
            {
                try
                {
                    nbytes = s.Receive(buf);
                    if (nbytes == 0)
                        return 0;
                    ptr = 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                    return -1;
                }
            }

            --nbytes;
            p[0] = buf[ptr++];
            return 1;
        }

        // 사용자 정의 데이터 수신 함수
        public int recvline(Socket s, byte[] buf, int maxlen)
        {
            int n, nbytes;
            byte[] c = new byte[1];
            int ptr = 0;

            for (n = 1; n < maxlen; n++)
            {
                nbytes = _recv_ahead(s, c);
                if (nbytes == 1)
                {
                    buf[ptr++] = c[0];
                    if (c[0] == '\n')
                        break;
                }
                else if (nbytes == 0)
                {
                    return n - 1;
                }
                else
                {
                    return -1;
                }
            }

            return n;
        }
    }

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
            byte[] buf = new byte[BUFSIZE];

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
                    LineRead lineread = new LineRead();
                    while (true)
                    {
                        // 데이터 받기
                        retval = lineread.recvline(client_sock, buf, BUFSIZE + 1);
                        if (retval == -1)
                            break;
                        else if (retval == 0)
                            break;

                        // 받은 데이터 출력
                        Console.Write("[TCP/{0}:{1}] {2}",
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
    }
}
