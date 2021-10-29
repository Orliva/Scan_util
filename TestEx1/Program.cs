using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Security;
using System.Security.Permissions;
using System.IO.Enumeration;

namespace TestEx1
{
    class Program
    {
        static void Main(string[] args)
        {
            IReportShow reportShow = new ConsoleReportShow();
            Queue<Report> queueDir = new Queue<Report>();
            foreach(string s in args)
            {
                if (Directory.Exists(s))
                    queueDir.Enqueue(new Report(s)); 
            }
            //for (int i = 0; i < queueDir.Count; i++)
                reportShow.Show(queueDir.Dequeue());
         //   Console.WriteLine(args[0]);
            //DirectoryInfo dirInfo = new DirectoryInfo(args[0]);
            //FileInfo fileInfo = new FileInfo(args[0]);
            //Directory.
        }
    }
}
