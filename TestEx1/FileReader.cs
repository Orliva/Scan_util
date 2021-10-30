using System;
using System.IO;
using System.Linq;
using System.Threading;

namespace TestEx1
{

    /// <summary>
    /// Реализовать наблюдателя за CountParsingErrors, просматривать в репорте и увеличивать число ошибок сканирования!!!
    /// </summary>
    class FileReader
    {
        private static int countParsingErrors;
        public static int CountParsingErrors 
        {
            get { return countParsingErrors; }
            private set { countParsingErrors = value; }
        }
        public FileReader() { }

        /// <summary>
        /// Читаем весь файл за раз и обрабатываем прочитанное
        /// </summary>
        /// <param name="path">Путь к проверяемому файлу</param>
        /// <param name="mtd">Метод для обработки прочитанного</param>
        /// <returns></returns>
        public bool ApplyToAllFile(string path, Action<string, string> mtd)
        {
            StreamReader sr = null;
            string line = null;
            try
            {
                sr = new StreamReader(path);

                //Читаем файл в одну строку и убираем из нее пробелы и управляющие символы
                line = new string(sr.ReadToEnd().Where(c => !char.IsControl(c) && !char.IsWhiteSpace(c)).ToArray());
                mtd?.Invoke(path, line); //Обрабатываем (передаем в цепочку обработчиков)
            }
            catch (OutOfMemoryException ex) //Если не хватает памяти
            {
                line = null;
                GC.Collect(); //Пробуем освободить память и продолжить работу
                return false;
            }
            catch (Exception ex) //Если не удалось прочитать файл
            {
                Interlocked.Increment(ref countParsingErrors); //Увеличиваем количество ошибок сканирования
                return true;
            }
            finally { sr?.Dispose(); } //Освобождаем ресурсы
            return true;
        }

        /// <summary>
        /// Читаем файл построчно и обрабатываем прочитанное
        /// </summary>
        /// <param name="path">Путь к проверяемому файлу</param>
        /// <param name="mtd">Метод для обработки прочитанного</param>
        /// <returns></returns>
        public bool ApplyToEachLineFile(string path, Action<string, string> mtd)
        {
            StreamReader sr = null;
            string line = null;
            try
            {
                sr = new StreamReader(path);

                while ((line = new string(sr.ReadLine().Where(c => !char.IsControl(c) && !char.IsWhiteSpace(c)).ToArray())) != null)
                {
                    mtd?.Invoke(path, line); //В каждой строке пробуем отыскать "подозрительную" строчку
                }
            }
            catch (OutOfMemoryException ex) //Если не хватает памяти
            {
                line = null;
                GC.Collect(); //Пробуем освободить память и продолжить работу
                Interlocked.Increment(ref countParsingErrors); //Увеличиваем количество ошибок сканирования т.к.
                                                               //способа получить содержимое файла с меньшими 
                                                               //затратами памяти в программе не предусмотрено
                return false;
            }
            catch (Exception ex) //Если не удалось прочитать файл
            {
                Interlocked.Increment(ref countParsingErrors); //Увеличиваем количество ошибок сканирования
            }
            finally { sr?.Dispose(); } //Освобождаем ресурсы
            return true;
        }

        /// <summary>
        /// Обнуляем счетчики
        /// </summary>
        public static void Clear()
        {
            CountParsingErrors = 0;
        }
    }
}
