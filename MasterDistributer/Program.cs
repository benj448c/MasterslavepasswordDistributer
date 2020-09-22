using System;

namespace MasterDistributer
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            worker worker1 = new worker();
            worker1.readfromfile();
            worker1.Start(7007);
        }
    }
}
