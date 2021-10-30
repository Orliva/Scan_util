using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Enumeration;
using System.Threading;
using System.Threading.Tasks;


namespace TestEx1
{
    /// <summary>
    /// Класс отчета о сканировании
    /// </summary>
    class Report
    {
        public string Path { get; }                      // Директория которую будем сканировать
        
        private int countFiles;                          // Количество просканированных файлов
        public int CountFiles 
        { 
            get { return countFiles; }
            private set { countFiles = value; } 
        }

        private int countError;                          // Количество возникших ошибок
        public int CountError 
        { 
            get { return countError; }
            private set { countError = value; }
        }

        public TimeSpan ElapsedTime { get; private set; } // Время составления отчета
        public List<string> Result { get; private set; }  //Отчет сканирования
        
        private readonly Stopwatch sw;
        protected DubiousFile startChainNode;             //Первый обработчик в цепочке обработчиков

        public Report(string dirPath, DubiousFile startChainNode)
        {
            sw = new Stopwatch();
            this.startChainNode = startChainNode;
            CountFiles = 0;
            CountError = 0;
            if (Directory.Exists(dirPath))
                Path = dirPath;
            else
                Path = null;
        }

        /// <summary>
        /// Получить отчет
        /// </summary>
        /// <returns>Список представляющий готовый к выводу отчет</returns>
        public virtual List<string> GetReport()
        {
            sw.Restart();
            List<string> res = CreateReport();
            sw.Stop();
            ElapsedTime = sw.Elapsed;
            return res;
        }

        /// <summary>
        /// Создать отчет
        /// </summary>
        /// <returns></returns>
        protected virtual List<string> CreateReport()
        {
            //Получаем все файлы по указанному пути
            IEnumerable<string> fileNames = new FileSystemEnumerable<string>(Path, (ref FileSystemEntry entry) => entry.ToFullPath(),
                new EnumerationOptions { IgnoreInaccessible = false, RecurseSubdirectories = true, AttributesToSkip = 0 })
            {
                ShouldIncludePredicate = (ref FileSystemEntry entry) => !entry.IsDirectory, 
            };

            //Обрабатываем все найденные файлы
            Parallel.ForEach(fileNames, (string file) => 
            {
                FileReader fileReader = new FileReader();
                if (fileReader.ApplyToAllFile(file, startChainNode.Handle))
                    Interlocked.Increment(ref countFiles);
                else
                {
                    fileReader.ApplyToEachLineFile(file, startChainNode.Handle);
                    Interlocked.Increment(ref countFiles);
                }
            });

            return GetResult();
        }

        /// <summary>
        /// Получаем тело отчета о сканировании
        /// </summary>
        /// <returns></returns>
        private List<string> GetResult()
        {
            Result = new List<string>();
            Result.Add("Processed files: " + CountFiles.ToString());

            int len = startChainNode.GetLengthOfChain() + 1;
            for (int i = 0; i < len; i++)
            {
                Result.Add(startChainNode.GetDescriptionOfChain(i) 
                    + startChainNode.GetCountDubiousFileOfChain(i).ToString());
            }

            CountError = FileReader.CountParsingErrors;
            Result.Add("Errors: " + FileReader.CountParsingErrors.ToString());

            string str = string.Format("{0:00}:{1:00}:{2:00}",
                ElapsedTime.Hours, ElapsedTime.Minutes, ElapsedTime.Seconds);
            Result.Add("Exection time: " + str);

            return Result;
        }   
    }
}
