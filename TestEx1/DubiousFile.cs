using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace TestEx1
{
    ///Базовый класс предоставляющий возможность поиска "подозрительных" файлов
    abstract class DubiousFile
    {
        public string Description { get; protected set; } // Описание подозрительного файла
        public int CountDubiosFile { get; protected set; } // Количество найденых подозрительных файлов
        public DubiousFile Successor { get; set; } // Обработчики
        public string PathFile { get; set; } // Путь к проверяемому файлу УДАЛИТЬ!!!!!!
        public int CountParsingErrors { get; protected set; } // Количество ошибок анализа файлов
        
        protected DubiousFile(string path)
        {
            CountDubiosFile = 0;
            CountParsingErrors = 0;
            PathFile = path;
            Successor = null;
        }

        /// <summary>
        /// Обработчик проверки на "подозрительность"
        /// </summary>
        /// <param name="path">Путь к проверяемому файлу</param>
        public virtual void Handle(string path)
        {
            if (IsDubiousFile(path).Result) //Если файл "подозрительный",
                CountDubiosFile++;          //то прибавь количество "подозрительных" файлов
            else                            //Если файл НЕ "подозрительный" и есть еще обработчики,
                Successor?.Handle(path);    //то вызови следующий обработчик
        }

        /// <summary>
        /// Проверка, является ли файл "подозрительным"
        /// </summary>
        /// <param name="path">Путь к проверяемому файлу</param>
        /// <returns></returns>
        public abstract Task<bool> IsDubiousFile(string path);
    }

    /// <summary>
    /// Класс представляющий возможность
    /// нахождения "подозрительных" файлов по их содержимому
    /// </summary>
    class InnerStrDubiousFile : DubiousFile
    {
        public string DubiousString { get; } //Строка, представляющая "подозрительное" содержимое
        public InnerStrDubiousFile(string path, string description, string dubiousStr) : base(path)
        {
            Description = description;
            DubiousString = dubiousStr;
        }
        public async override Task<bool> IsDubiousFile(string path) => await EvalFile(path);

        protected async virtual Task<bool> EvalFile(string path)
        {
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(path);
                
                string line;
                while ((line = await sr.ReadLineAsync()) != null)
                {
                    if (line == DubiousString)
                        return true;
                }
            }
            catch 
            {
                CountParsingErrors++;
                CountDubiosFile--;
                return true;
            }
            finally { sr?.Dispose(); }
            return false;
        }
    }

    
    class ExtInnerDubiousFile : InnerStrDubiousFile
    {   
        public string Extension { get; private set; }
        public ExtInnerDubiousFile(string path, string description, string dubiousStr, string extension) 
            : base(path, description, dubiousStr) { Extension = extension; }

        public async override Task<bool> IsDubiousFile(string path)
        {
            if (Path.GetExtension(path).ToLower() != Extension)
                return false;

            return await base.IsDubiousFile(path);
        }
    }
}
