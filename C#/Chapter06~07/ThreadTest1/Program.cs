using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ThreadTest1
{
    class Point3D
    {
        public int x, y, z;
        public Point3D(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    class Program
    {
        static void MyThread(object arg)
        {
            Thread.Sleep(1000);
            Point3D pt = (Point3D)arg;
            Console.WriteLine("Running MyThread() {0}: {1}, {2}, {3}",
                Thread.CurrentThread.ManagedThreadId,
                pt.x, pt.y, pt.z);
        }

        static void Main(string[] args)
        {
            // 첫 번째 스레드 생성
            Point3D pt1 = new Point3D(10, 20, 30);
            Thread hThread1 = new Thread(MyThread);
            hThread1.Start(pt1);

            // 두 번째 스레드 생성
            Point3D pt2 = new Point3D(40, 50, 60);
            Thread hThread2 = new Thread(MyThread);
            hThread2.Start(pt2);

            Console.WriteLine("Running main() {0}",
                Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(2000);
        }
    }
}
