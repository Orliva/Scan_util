using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using AhoCorasick;

namespace TestEx1
{
    ///Базовый класс предоставляющий возможность поиска "подозрительных" файлов
    abstract class DubiousFile
    {
        public string Extension { get; protected set; }
        public string Description { get; protected set; } // Описание подозрительного файла
        private int countDubiosFile;
        public int CountDubiosFile { 
            get { return countDubiosFile; }
            protected set { countDubiosFile = value; }
        } // Количество найденых подозрительных файлов
        public DubiousFile Successor { get; set; } // Обработчики
        public string PathFile { get; set; } // Путь к проверяемому файлу УДАЛИТЬ!!!!!!
        public int CountParsingErrors { get; protected set; } // Количество ошибок анализа файлов
        
        protected DubiousFile(string path)
        {
            Extension = null;
            CountDubiosFile = 0;
            CountParsingErrors = 0;
            PathFile = path;
            Successor = null;
        }

        /// <summary>
        /// Обработчик проверки на "подозрительность"
        /// </summary>
        /// <param name="path">Путь к проверяемому файлу</param>
        public virtual void Handle(string pathFile, string line)
        {
            if (Extension != null) //Если на важно конкретное расширение файла (например .js)
            {
                if (Extension == Path.GetExtension(pathFile)) //Проверяем, что расширение файла нам подходит
                {
                    if (IsDubiousStr(line)) //Если файл "подозрительный",
                        Interlocked.Increment(ref countDubiosFile);  //то прибавь количество "подозрительных" файлов
                }
                else                                          //Если расширение файла нам не подошло, то вызови следующий обработчик
                    Successor?.Handle(pathFile, line);
            }
            else                   //Если нам не важно расширение файла
            {
                if (IsDubiousStr(line))                //Если файл "подозрительный",
                    Interlocked.Increment(ref countDubiosFile);                 //то прибавь количество "подозрительных" файлов
                else                                   //Если файл НЕ "подозрительный" и есть еще обработчики,
                    Successor?.Handle(pathFile, line); //то вызови следующий обработчик
            }
        }

        /// <summary>
        /// Проверка, является ли файл "подозрительным"
        /// </summary>
        /// <param name="path">Путь к проверяемому файлу</param>
        /// <returns></returns>
        public virtual bool IsDubiousStr(string line)
        {
            return Eval(line);
        }
        protected abstract bool Eval(string line);
    }

    /// <summary>
    /// Класс представляющий возможность
    /// нахождения "подозрительных" файлов по их содержимому
    /// </summary>
    class InnerStrDubiousFile : DubiousFile
    {
        public string DubiousString { get; } //Строка, представляющая "подозрительное" содержимое
        //Trie trie;
        public InnerStrDubiousFile(string path, string description, string dubiousStr, string extension = null) : base(path)
        {
            Extension = extension;
            Description = description;
            DubiousString = new string(dubiousStr.Where(c => !char.IsControl(c) && !char.IsWhiteSpace(c)).ToArray());
            //trie = new Trie();
            //trie.Add(DubiousString);
            //trie.Build();
        }
        public override bool IsDubiousStr(string line) => Eval(line);

        protected override bool Eval(string line)
        {
            Trie trie = new Trie();
            trie.Add(DubiousString);
            trie.Build();
            if (trie.Find(line).Any())
                return true;
            return false;
        }
    }
}
