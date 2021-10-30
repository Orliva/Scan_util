using System;
using System.Collections.Generic;

namespace TestEx1
{
    /// <summary>
    /// Интерфейс вывода отчета
    /// </summary>
    interface IReportShow
    {
        public void Show(Report report);
    }

    /// <summary>
    /// Класс вывода отчета на консоль
    /// </summary>
    class ConsoleReportShow : IReportShow
    {
        private readonly string header;
        private readonly string footer;
        private readonly char separator;
        public ConsoleReportShow()
        {
            separator = '=';
            header = new string(separator, 12) + " Scan result " + new string(separator, 12);
            footer = new string(separator, 37);
        }

        /// <summary>
        /// Вывод отчета
        /// </summary>
        /// <param name="report">Отчет, который требуется вывести на консоль</param>
        public virtual void Show(Report report)
        {
            ConsoleOutput(report.GetReport());
        }

        /// <summary>
        /// Вывод на консоль
        /// </summary>
        /// <param name="report">Тело отчета</param>
        private void ConsoleOutput(List<string> report)
        {
            Console.WriteLine(header);
            foreach (string s in report)
                Console.WriteLine(s);
            Console.WriteLine(footer);
        }
    }
}
