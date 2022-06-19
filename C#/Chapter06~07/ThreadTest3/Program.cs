using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ThreadTest3
{
    class Program
    {
        static int sum = 0;

        static void MyThread(object arg)
        {
            int num = (int)arg;
            for (int i = 1; i <= num; i++)
                sum += i;
        }

        static void Main(string[] args)
        {
            int num = 100;
            Thread hThread = new Thread(MyThread);

            Console.WriteLine("스레드 실행 전. 계산 결과 = {0}", sum);
            // [참고] hThread.Suspend()나 hThread.Resume()과 같은
            // 기능이 있지만, 이 프로그램의 구현에는 불필요하다.
            hThread.Start(num);
            hThread.Join();
            Console.WriteLine("스레드 실행 후. 계산 결과 = {0}", sum);
        }
    }
}
