using System.Collections.Generic;
using System.IO;

namespace TestEx1
{
    class Program
    {
        static void Main(string[] args)
        {
            IReportShow reportShow = new ConsoleReportShow();
            Queue<Report> queueDir = new Queue<Report>();

            //Создаем различные виды "подозрительных" файлов
            DubiousFile jsFile = new InnerStrDubiousFile("JS detects: ", "<script>evil_script()</script>", ".js");
            DubiousFile rmFile = new InnerStrDubiousFile("rm -rf detects: ", @"rm -rf %userprofile%\Documents");
            DubiousFile runDllFile = new InnerStrDubiousFile("Rundll32 detects: ", @"Rundll32 sus.dll SusEntry");
            //Устанавливаем цепочку обработчиков
            jsFile.Successor = rmFile;
            rmFile.Successor = runDllFile;

            foreach(string s in args)
            {
                if (Directory.Exists(s))
                    queueDir.Enqueue(new Report(dirPath:s, startChainNode:jsFile)); 
            }
            while (queueDir.Count > 0)
            {
                reportShow.Show(queueDir.Dequeue());
                jsFile.Clear();
                rmFile.Clear();
                runDllFile.Clear();
                FileReader.Clear();
            }
        }
    }
}
