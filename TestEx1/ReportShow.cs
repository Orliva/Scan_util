using System;
using System.Collections.Generic;

namespace TestEx1
{
    interface IReportShow
    {
        public void Show(Report report);
    }

    class ConsoleReportShow : IReportShow
    {
        private readonly string header;
        private readonly string footer;
        private readonly char separator;
        public ConsoleReportShow()
        {
            separator = '=';
            header = new string(separator, 15) + " Scan result " + new string(separator, 15);
            footer = new string(separator, 43);
        }

        public virtual void Show(Report report)
        {
            ConsoleOutput(report.GetReport(), report.ElapsedTime);
        }

        protected virtual void ConsoleOutput(List<string> report, TimeSpan elapsedTime)
        {
            Console.WriteLine(header);
            foreach (string s in report)
                Console.WriteLine(s);
            string str = string.Format("{0:00}:{1:00}:{2:00}",
            elapsedTime.Hours, elapsedTime.Minutes, elapsedTime.Seconds);
            Console.WriteLine("Exection time: " + str);
            Console.WriteLine(footer);
        }
    }
}
