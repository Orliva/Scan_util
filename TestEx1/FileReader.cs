using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace TestEx1
{
    class FileReader
    {
        private static int countParsingErrors;
        public static int CountParsingErrors {
            get { return countParsingErrors; }
            private set { countParsingErrors = value; }
        }
        public bool ApplyToAllFile(string path, Action<string, string> mtd)
        {
            StreamReader sr = null;
            string line = null;
            try
            {
                sr = new StreamReader(path);
                //throw new OutOfMemoryException();
                line = new string(sr.ReadToEnd().Where(c => !char.IsControl(c) && !char.IsWhiteSpace(c)).ToArray());
                mtd?.Invoke(path, line);
            }
            catch (OutOfMemoryException ex)
            {
                line = null;
                GC.Collect();
                return false;
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref countParsingErrors);
                return true;
            }
            finally { sr?.Dispose(); }
            return true;
        }

        public bool ApplyToEachLineFile(string path, Action<string, string> mtd)
        {
            StreamReader sr = null;
            string line = null;
            try
            {
                sr = new StreamReader(path);

                while ((line = new string(sr.ReadLine().Where(c => !char.IsControl(c) && !char.IsWhiteSpace(c)).ToArray())) != null)
                {
                    mtd?.Invoke(path, line);
                }
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref countParsingErrors);

                //CountParsingErrors++;
            }
            finally { sr?.Dispose(); }
            return true;
        }
    }
}
