using AhoCorasick;
using System.IO;
using System.Linq;
using System.Threading;

namespace TestEx1
{

    /// <summary>
    /// Базовый класс предоставляющий возможность
    /// поиска "подозрительных" файлов 
    /// </summary>
    abstract class DubiousFile
    {
        public string Extension { get; protected set; }   // Расширение файла
        public string Description { get; protected set; } // Описание подозрительного файла
        
        private int countDubiosFile;                      // Количество найденых подозрительных файлов
        public int CountDubiosFile                        // Количество подозрительных файлов
        { 
            get { return countDubiosFile; }
            protected set { countDubiosFile = value; }
        }
        public DubiousFile Successor { get; set; }        // Обработчики
        
        protected DubiousFile()
        {
            Extension = null;
            CountDubiosFile = 0;
            Successor = null;
        }

        /// <summary>
        /// Получить длину цепочки (не включая вызывающего элемента)
        /// Общая длинна цепочки равна len + 1
        /// </summary>
        /// <param name="len">Текущая длинна цепочки</param>
        /// <returns></returns>
        public int GetLengthOfChain(int len = 0)
        {
            if (Successor == null)
                return len;
            return Successor.GetLengthOfChain(++len);
        }

        /// <summary>
        /// Обнулить счетчики экземпляра
        /// </summary>
        public virtual void Clear()
        {
            CountDubiosFile = 0;
        }

        /// <summary>
        /// Получить описание экземпляра в цепочке
        /// </summary>
        /// <param name="index">Индекс искомого экземпляра</param>
        /// <param name="curPos">Текущая позиция в цепочке</param>
        /// <returns></returns>
        public string GetDescriptionOfChain(in int index, int curPos = 0)
        {
            if (index != curPos && Successor != null)
                return Successor.GetDescriptionOfChain(in index, ++curPos);
            else
            {
                if (index == curPos)
                    return Description;
                else if (Successor == null && index != curPos)
                    return null;
            }
            return null;
        }

        /// <summary>
        /// Получить количество "подозрительных" файлов экземпляра в цепочке
        /// </summary>
        /// <param name="index">Индекс искомого экземпляра</param>
        /// <param name="curPos">Текущая позиция в цепочке</param>
        /// <returns></returns>
        public int GetCountDubiousFileOfChain(in int index, int curPos = 0)
        {
            if (index != curPos && Successor != null)
                return Successor.GetCountDubiousFileOfChain(in index, ++curPos);
            else
            {
                if (index == curPos)
                    return CountDubiosFile;
                else if (Successor == null && index != curPos)
                    return -1;
            }
            return -1;
        }

        /// <summary>
        /// Обработчик проверки на "подозрительность"
        /// </summary>
        /// <param name="path">Путь к проверяемому файлу</param>
        /// <param name="line">Содержимое файла</param>
        public virtual void Handle(string pathFile, string line)
        {
            if (Extension != null)                            //Если нам важно конкретное расширение файла (например .js)
            {
                if (Extension == Path.GetExtension(pathFile)) //Проверяем, что расширение файла нам подходит
                {
                    if (IsDubiousStr(line))                          //Если файл "подозрительный",
                        Interlocked.Increment(ref countDubiosFile);  //то прибавь количество "подозрительных" файлов
                }
                else                                          //Если расширение файла нам не подошло, то вызови следующий обработчик
                    Successor?.Handle(pathFile, line);
            }
            else                                              //Если нам не важно расширение файла
            {
                if (IsDubiousStr(line))                             //Если файл "подозрительный",
                    Interlocked.Increment(ref countDubiosFile);     //то прибавь количество "подозрительных" файлов
                else                                          //Если файл НЕ "подозрительный" и есть еще обработчики,
                    Successor?.Handle(pathFile, line);              //то вызови следующий обработчик
            }
        }

        /// <summary>
        /// Проверка, является ли файл "подозрительным"
        /// </summary>
        /// <param name="path">Путь к проверяемому файлу</param>
        /// <returns></returns>
        public abstract bool IsDubiousStr(string line);
    }

    /// <summary>
    /// Класс представляющий возможность
    /// нахождения "подозрительных" файлов по их содержимому
    /// </summary>
    class InnerStrDubiousFile : DubiousFile
    {
        public string DubiousString { get; } //Строка, представляющая "подозрительное" содержимое
        public InnerStrDubiousFile(string description, string dubiousStr, string extension = null) : base()
        {
            Extension = extension;
            Description = description;
            DubiousString = new string(dubiousStr.Where(c => !char.IsControl(c) && !char.IsWhiteSpace(c)).ToArray());
        }

        /// <summary>
        /// Проверка, является ли файл "подозрительным"
        /// </summary>
        /// <param name="path">Путь к проверяемому файлу</param>
        /// <returns></returns>
        public override bool IsDubiousStr(string line)
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
