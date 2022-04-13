using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Logging
{
    class TLog
    {
        private static Object _lockMe = new Object();
        private static int _logCnt = 0;
        private static int _logInd = 0;
        private static string _FileDir = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\";

        private static string _FilePath = _CreateFilePath();

        private static string _CreateFilePath()
        {
            return _FileDir
                + _logInd.ToString() + "_"
                + System.IO.Path.GetFileNameWithoutExtension(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName)
                + "_" + DateTime.Now.ToString(new CultureInfo("ru-RU")).Replace(".", "_").Replace(" ", "__").Replace(":", "_") + ".log";
        }

        private static void _IncLogCnt()
        {
            _logCnt++;

            if (_logCnt > 100)
            {
                if (new FileInfo(_FilePath).Length > 1 * 1000000)
                {
                    _logInd++;
                    _FilePath = _CreateFilePath();
                }
            }
        }

        public static string GetFilePath()
        {
            return _FilePath;
        }

        public static void ModifyFilePath(string FileDir)
        {
            _FileDir = FileDir;
            _FilePath = _CreateFilePath();
        }

        public static void ToLog(string line)
        {
            FileStream fs = null;

            try
            {
                fs = File.Open(_FilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

                using (StreamWriter writer = new StreamWriter(fs))
                {
                    string s = DateTime.Now.ToString() + "  [" + new StackFrame(1).GetMethod().Name + "]  " + line;
                    writer.WriteLine(s);
                    Console.WriteLine(s);
                }

                _IncLogCnt();
            }
            catch (Exception logexcept)
            {
                Console.WriteLine("EXCEPTION ToLog("
                    + ((logexcept != null) ? logexcept.Message : "")
                    + (((logexcept != null) && (logexcept.InnerException != null)) ? logexcept.InnerException.Message : "") + ")");
            }
        }

        public static void ToLogAsync(string line)
        {
            lock (_lockMe)
            {
                try
                {
                    FileStream fs = null;

                    fs = File.Open(_FilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

                    using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
                    {
                        string s = DateTime.Now.ToString() + "  [" + new StackFrame(1).GetMethod().Name + "]  " + line;
                        writer.WriteLine(s);
                        Console.WriteLine(s);
                    }

                    _IncLogCnt();
                }
                catch (Exception logexcept)
                {
                    Console.WriteLine("EXCEPTION ToLogAsync("
                        + ((logexcept != null) ? logexcept.Message : "")
                        + (((logexcept != null) && (logexcept.InnerException != null)) ? logexcept.InnerException.Message : "") + ")");
                }
            }
        }

        public static void ToLogExceptAsync(string line, Exception except)
        {
            lock (_lockMe)
            {
                try
                {
                    FileStream fs = null;

                    fs = File.Open(_FilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

                    using (StreamWriter writer = new StreamWriter(fs, Encoding.UTF8))
                    {
                        line += "("
                            + ((except != null) ? except.Message : "")
                            + (((except != null) && (except.InnerException != null)) ? except.InnerException.Message : "")
                            + ")";
                        string s = DateTime.Now.ToString() + "  !EXCEPTION!  [" + new StackFrame(1).GetMethod().Name + "]  " + line;
                        writer.WriteLine(s);
                        Console.WriteLine(s);
                    }

                    _IncLogCnt();
                }
                catch (Exception logexcept)
                {
                    Console.WriteLine("EXCEPTION ToLogExceptAsync("
                        + ((logexcept != null) ? logexcept.Message : "")
                        + (((logexcept != null) && (logexcept.InnerException != null)) ? logexcept.InnerException.Message : "") + ")");
                }
            }
        }
    }
}
