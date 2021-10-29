using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.IO.Enumeration;

namespace TestEx1
{
    class Report
    {
        public string Path { get; } // Директория которую будем сканировать
        public int CountFiles { get; private set; } // Количество просканированных файлов
        public int CountError { get; private set; } // Количество возникших ошибок (возможно удалить)
        public TimeSpan ElapsedTime { get; private set; } // Время составления отчета
        public List<string> Result { get; private set; }
        private Stopwatch sw;
        private DubiousFile startChainNode; ///Внедрить через конструктор !!!!!
        public Report(string dirPath)
        {
            sw = new Stopwatch();
            CountFiles = 0;
            CountError = 0;
            if (Directory.Exists(dirPath))
                Path = dirPath;
            else
                Path = null;
        }

        //Получить отчет
        public virtual List<string> GetReport()
        {
            sw.Restart();
            List<string> res = CreateReport();
            sw.Stop();
            ElapsedTime = sw.Elapsed;
            return res;
        }

        public async virtual Task<List<string>> GetReportAsync()
        {
            return await Task.Run(GetReport);
        }

        //Создать отчет
        protected virtual List<string> CreateReport()
        {
            DubiousFile jsFile = new ExtInnerDubiousFile(Path, "JS detects: ", "<script>evil_script()</script>", ".js");
            DubiousFile rmFile = new InnerStrDubiousFile(Path, "rm -rf detects: ", @"rm -rf %userprofile%\Documents");
            DubiousFile runDllFile = new InnerStrDubiousFile(Path, "Rundll32 detects: ", @"Rundll32 sus.dll SusEntry");
            jsFile.Successor = rmFile;
            rmFile.Successor = runDllFile;
            startChainNode = jsFile;

            IEnumerable<string> fileNames = new FileSystemEnumerable<string>(Path, (ref FileSystemEntry entry) => entry.ToFullPath(),
                new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true, AttributesToSkip = 0 })
            {
                ShouldIncludePredicate = (ref FileSystemEntry entry) => !entry.IsDirectory, 
            };

            int cnt = 0;
            foreach (var s in fileNames)
                cnt++;
            Console.WriteLine(cnt);
            //  ParallelOptions opt = new ParallelOptions();

            foreach(var f in fileNames)
            {
                Eval(f);
            }
          //  ParallelLoopResult resParallel = Parallel.ForEach(fileNames, Eval);
            //resParallel.


            return GetResult(jsFile, rmFile, runDllFile);
        }

        private List<string> GetResult(params DubiousFile[] files)
        {
            Result = new List<string>();
            Result.Add("Processed files: " + CountFiles.ToString());

            foreach (DubiousFile file in files)
            {
                Result.Add(file.Description + file.CountDubiosFile.ToString());
                CountError += file.CountParsingErrors;
            }
            Result.Add("Errors: " + CountError.ToString());

            return Result;
        }

        private void Eval(string path)
        {
            //  Console.WriteLine(path);
            startChainNode?.Handle(path);
            CountFiles++;
        }
    }
}
