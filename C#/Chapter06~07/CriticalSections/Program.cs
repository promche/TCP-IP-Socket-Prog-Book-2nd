using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CriticalSections
{
    class Program
    {
        const int MAXCNT = 100000000;
        static int count = 0;

        static void MyThread1(object arg)
        {
            for (int i = 0; i < MAXCNT; i++)
            {
                Monitor.Enter(arg);
                count += 2;
                Monitor.Exit(arg);
            }
        }

        static void MyThread2(object arg)
        {
            for (int i = 0; i < MAXCNT; i++)
            {
                Monitor.Enter(arg);
                count -= 2;
                Monitor.Exit(arg);
            }
        }

        static void Main(string[] args)
        {
            // 동기화용 오브젝트 생성
            object obj = new object();

            // 스레드 두 개 생성
            Thread[] hThread = new Thread[2];
            hThread[0] = new Thread(MyThread1);
            hThread[1] = new Thread(MyThread2);
            hThread[0].Start(obj);
            hThread[1].Start(obj);

            // 스레드 두 개 종료 대기
            hThread[0].Join();
            hThread[1].Join();

            // 결과 출력
            Console.WriteLine("count = {0}", count);
        }
    }
}
