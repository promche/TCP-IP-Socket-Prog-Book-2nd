using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace TCPServer
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

                // 수신 버퍼의 크기를 얻는다.
                int optval;
                optval = (int)listen_sock.GetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer);
                Console.WriteLine("수신 버퍼 크기(old) = {0}바이트", optval);
                // 수신 버퍼의 크기를 두 배로 늘린다.
                optval *= 2;
                listen_sock.SetSocketOption(SocketOptionLevel.Socket,
                    SocketOptionName.ReceiveBuffer, optval);
                // 수신 버퍼의 크기를 얻는다.
                optval = (int)listen_sock.GetSocketOption(
                    SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer);
                Console.WriteLine("수신 버퍼 크기(new) = {0}바이트", optval);

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
                    while (true)
                    {
                        // 데이터 받기
                        retval = client_sock.Receive(buf, BUFSIZE, SocketFlags.None);
                        if (retval == 0) break;

                        // 받은 데이터 출력
                        Console.WriteLine("[TCP/{0}:{1}] {2}",
                            clientaddr.Address, clientaddr.Port,
                            Encoding.Default.GetString(buf, 0, retval));

                        // 데이터 보내기
                        client_sock.Send(buf, retval, SocketFlags.None);
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
