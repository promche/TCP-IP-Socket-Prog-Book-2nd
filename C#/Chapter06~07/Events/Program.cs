using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Events
{
    class Program
    {
        const int BUFSIZE = 10;
        static AutoResetEvent hWriteEvent;
        static AutoResetEvent hReadEvent;
        static int[] buf = new int[BUFSIZE];

        static void WriteThread(object arg)
        {
            for (int k = 1; k <= 500; k++)
            {
                // 읽기 완료 대기
                hReadEvent.WaitOne();

                // 공유 버퍼에 데이터 저장
                for (int i = 0; i < BUFSIZE; i++)
                    buf[i] = k;

                // 쓰기 완료 알림
                hWriteEvent.Set();
            }
        }

        static void ReadThread(object arg)
        {
            while (true)
            {
                // 쓰기 완료 대기
                hWriteEvent.WaitOne();

                // 읽은 데이터 출력 후 버퍼를 0으로 초기화
                Console.Write("Thread {0:D4}:\t",
                    Thread.CurrentThread.ManagedThreadId);
                for (int i = 0; i < BUFSIZE; i++)
                    Console.Write("{0:D3} ", buf[i]);
                Console.WriteLine();
                Array.Clear(buf, 0, buf.Length);

                // 읽기 완료 알림
                hReadEvent.Set();
            }
        }

        static void Main(string[] args)
        {
            // 이벤트 생성
            hWriteEvent = new AutoResetEvent(false);
            hReadEvent = new AutoResetEvent(false);

            // 스레드 세 개 생성
            Thread[] hThread = new Thread[3];
            hThread[0] = new Thread(WriteThread);
            hThread[1] = new Thread(ReadThread);
            hThread[2] = new Thread(ReadThread);
            hThread[0].Start();
            hThread[1].Start();
            hThread[2].Start();

            // 읽기 완료 알림
            hReadEvent.Set();

            // 스레드 세 개 종료 대기
            hThread[0].Join();
            hThread[1].Join();
            hThread[2].Join();

            // 이벤트 제거
            hWriteEvent.Close();
            hReadEvent.Close();
        }
    }
}
