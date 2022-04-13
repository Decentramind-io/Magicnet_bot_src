using Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

//storj.io

namespace Magicnet.Storj
{
    public static class StorjCore
    {
        public static List<StorjBucket> getBuckets()
        {
            List<string> bucketlist = _runUtilityWhileNotErr("ls", true);

            if ((bucketlist == null) || (bucketlist.Count == 0)) return null;

            return _parseBucketList(bucketlist);
        }

        public static bool bucketIsEmpty(string bucket)
        {
            List<StorjFile> files = getFiles(bucket);
            return ((files == null) || (files.Count == 0));
        }

        public static void clearBucket(string bucket)
        {
            List<StorjFile> files = getFiles(bucket);

            if ((files == null) || (files.Count == 0)) return;

            foreach (StorjFile file in files)
            {
                removeFile(bucket, file.filename);
            }
        }

        public static void uploadFile(string bucket, string fullFilePath)
        {
            _runUtilityWhileNotErr("cp " + fullFilePath + " sj://" + bucket, true);
        }

        public static void downloadFile(string bucket, string filename, string fullDestFilePath)
        {
            _runUtilityWhileNotErr("cp " + " sj://" + bucket + "/" + filename + " " + fullDestFilePath, true);
        }

        public static void removeFile(string bucket, string filename)
        {
            _runUtilityWhileNotErr("rm sj://" + bucket + "/" + filename, true);
        }

        public static void moveFileToBucket(string sourceBucket, string destBucket, string filename)
        {
            _runUtilityWhileNotErr("mv sj://" + sourceBucket + "/" + filename + " sj://" + destBucket + "/" + filename, true);
        }

        public static void createBucket(string bucket)
        {
            _runUtilityWhileNotErr("mb sj://" + bucket, true);
        }

        public static List<StorjFile> getFiles(string bucket)
        {
            List<string> filelist = _runUtilityWhileNotErr("ls sj://" + bucket + " --utc", true);

            if ((filelist == null) || (filelist.Count == 0)) return null;

            return _parseFileList(filelist);
        }

        private static List<StorjFile> _parseFileList(List<string> filelist)
        {
            return
                filelist
                .Where(y => !((y.Contains("KIND")) && (y.Contains("CREATED") && (y.Contains("SIZE")) && (y.Contains("KEY")))))
                .Select(z =>
                {
                    return _parseStrFromFilelist(z);
                }).ToList<StorjFile>();
        }

        private static List<StorjBucket> _parseBucketList(List<string> bucketlist)
        {
            return
                bucketlist
                .Where(y => !((y.Contains("NAME")) && (y.Contains("CREATED"))))
                .Select(z =>
                {
                    return _parseStrFromBucketlist(z);
                }).ToList<StorjBucket>();
        }

        private static StorjFile _parseStrFromFilelist(string strFromFilelist)
        {
            StorjFile result = new StorjFile();

            string[] clmns = strFromFilelist.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (clmns.Length != 5) return result;

            result.dt = DateTime.ParseExact((clmns[1] + " " + clmns[2]), "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture);

            result.filesize = int.Parse(clmns[3]);

            result.filename = clmns[4];

            return result;
        }

        private static StorjBucket _parseStrFromBucketlist(string strFromBucketlist)
        {
            StorjBucket result = new StorjBucket();

            string[] clmns = strFromBucketlist.Split(" ", StringSplitOptions.RemoveEmptyEntries);

            if (clmns.Length != 3) return result;

            result.dt = DateTime.ParseExact((clmns[0] + " " + clmns[1]), "yyyy-MM-dd HH:mm:ss",
                                       System.Globalization.CultureInfo.InvariantCulture);

            result.name = clmns[2];

            return result;
        }

        private static List<string> _runUtility(string arguments, bool waitForExit)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo("uplink", arguments);
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            List<string> listout = new List<string>();
            string sout = string.Empty;

            if (process.StandardError != null)
            {
                while (!process.StandardError.EndOfStream)
                {
                    sout = process.StandardError.ReadLine();
                    listout.Add(sout);

                    TLog.ToLogAsync(process.StandardError.ReadLine());
                }
            }
            
            if (process.StandardOutput != null)
            {
                while (!process.StandardOutput.EndOfStream)
                {
                    sout = process.StandardOutput.ReadLine();
                    listout.Add(sout);
                    TLog.ToLogAsync(sout);
                }
            }

            if (waitForExit) process.WaitForExit(1000 * 20);

            listout.ForEach(y =>
            {
                if ((y.Contains("failed")) || (y.Contains("dial tcp: i/o timeout")) || (y.Contains("error")))
                    throw new Exception(string.Join(Environment.NewLine, listout));
            });

            return listout;
        }

        private static List<string> _runUtilityWhileNotErr(string arguments, bool waitForExit, bool whileNotErr = true, int whileNotErrSec = 60)
        {
            List<string> listout = new List<string>();

            string lasterr = string.Empty;
            bool err = true;
            DateTime start = DateTime.Now;

            if (whileNotErr)
            {
                while (true)
                {
                    if ((DateTime.Now - start).TotalSeconds > whileNotErrSec) break;

                    try
                    {
                        listout = _runUtility(arguments, waitForExit);
                    }
                    catch (Exception except)
                    {
                        lasterr = except.Message;
                        err = true;

                        Thread.Sleep(TimeSpan.FromSeconds(3));
                        continue;
                    }

                    err = false;
                    break;
                }

                if (err) throw new Exception(lasterr);
            }
            else listout = _runUtility(arguments, waitForExit);

            return listout;
        }
    }
}
