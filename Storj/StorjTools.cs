using Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Magicnet.Storj
{
    public static class StorjTools
    {
        public static void createBuckets()
        {
            List<StorjBucket> buckets = null;
            try
            {
                buckets = StorjCore.getBuckets();
            }
            catch { }

            string bucketName = StorjConsts.BUCKET_TASKS;
            if ((buckets == null) || (!buckets.Any(y => y.name == bucketName)))
            {
                try
                {
                    StorjCore.createBucket(bucketName);
                }
                catch { }
            }

            bucketName = StorjConsts.BUCKET_TEMP;
            if ((buckets == null) || (!buckets.Any(y => y.name == bucketName)))
            {
                try
                {
                    StorjCore.createBucket(bucketName);
                }
                catch { }
            }

            bucketName = StorjConsts.BUCKET_LOGS;
            if ((buckets == null) || (!buckets.Any(y => y.name == bucketName)))
            {
                try
                {
                    StorjCore.createBucket(bucketName);
                }
                catch { }
            }

            bucketName = StorjConsts.BUCKET_STAT;
            if ((buckets == null) || (!buckets.Any(y => y.name == bucketName)))
            {
                try
                {
                    StorjCore.createBucket(bucketName);
                }
                catch { }
            }
        }

        public static bool tasksBucketIsEmpty()
        {
            return StorjCore.bucketIsEmpty(StorjConsts.BUCKET_TASKS);
        }

        public static void clearTasksBucket()
        {
            StorjCore.clearBucket(StorjConsts.BUCKET_TASKS);
        }

        public static void clearTempBucket()
        {
            StorjCore.clearBucket(StorjConsts.BUCKET_TEMP);
        }

        public static void clearLogsBucket()
        {
            StorjCore.clearBucket(StorjConsts.BUCKET_LOGS);
        }

        public static void clearStatBucket()
        {
            StorjCore.clearBucket(StorjConsts.BUCKET_STAT);
        }

        public static void uploadTaskFile(string fullFilePath)
        {
            StorjCore.uploadFile(StorjConsts.BUCKET_TASKS, fullFilePath);
        }

        public static void downloadTaskFile(string filename, string fullDestFilePath)
        {
            StorjCore.downloadFile(StorjConsts.BUCKET_TASKS, filename, fullDestFilePath);
        }

        public static List<StorjFile> getTasks()
        {
            return StorjCore.getFiles(StorjConsts.BUCKET_TASKS);
        }

        public static List<StorjFile> getTempFiles()
        {
            return StorjCore.getFiles(StorjConsts.BUCKET_TEMP);
        }

        public static void moveTaskToTemp(string filename)
        {
            moveFileToTemp(StorjConsts.BUCKET_TASKS, filename);
        }

        public static void moveFileToTemp(string destBucket, string filename)
        {
            StorjCore.moveFileToBucket(destBucket, StorjConsts.BUCKET_TEMP, filename);
        }

        public static void moveAllTasksToTemp()
        {
            List<StorjFile> files = getTasks();

            if ((files == null) || (files.Count == 0)) return;

            files.ForEach(y =>
            {
                moveTaskToTemp(y.filename);
            });
        }

        public static void setStatRequest()
        {
            string fullFilePath = Path.GetTempPath() + "/" + StorjConsts.GET_STAT_REQUEST;

            File.WriteAllText(fullFilePath, StorjConsts.GET_STAT_REQUEST);

            StorjCore.uploadFile(StorjConsts.BUCKET_STAT, fullFilePath);
        }

        public static string getStatResponse()
        {
            List<StorjFile> files = StorjCore.getFiles(StorjConsts.BUCKET_STAT);

            if ((files == null) || (files.Count == 0)) return "";

            StorjFile resp = files
                                .Where(y => y.filename == StorjConsts.SET_STAT_RESPONSE)
                                .FirstOrDefault();

            if (resp.filename == StorjConsts.SET_STAT_RESPONSE)
            {
                string fullFilePath = Path.GetTempPath() + "/" + resp.filename;

                StorjCore.downloadFile(StorjConsts.BUCKET_STAT, resp.filename, fullFilePath);

                moveFileToTemp(StorjConsts.BUCKET_STAT, resp.filename);

                string result = File.ReadAllText(fullFilePath);

                File.Delete(fullFilePath);

                return result;
            }

            return "";
        }

        public static void setStatResponse(string content)
        {
            string fullFilePath = Path.GetTempPath() + "/" + StorjConsts.SET_STAT_RESPONSE;

            File.WriteAllText(fullFilePath, content);

            StorjCore.uploadFile(StorjConsts.BUCKET_STAT, fullFilePath);
        }

        public static bool getStatRequest()
        {
            List<StorjFile> files = StorjCore.getFiles(StorjConsts.BUCKET_STAT);

            if ((files == null) || (files.Count == 0)) return false;

            if (files.Any(y => y.filename == StorjConsts.GET_STAT_REQUEST))
            {
                moveFileToTemp(StorjConsts.BUCKET_STAT, StorjConsts.GET_STAT_REQUEST);

                return true;
            }

            return false;
        }

        public static void setLogsRequest()
        {
            string fullFilePath = Path.GetTempPath() + "/" + StorjConsts.GET_LOGS_REQUEST;

            File.WriteAllText(fullFilePath, StorjConsts.GET_LOGS_REQUEST);

            StorjCore.uploadFile(StorjConsts.BUCKET_LOGS, fullFilePath);
        }

        public static string getLogsResponse()
        {
            List<StorjFile> files = StorjCore.getFiles(StorjConsts.BUCKET_LOGS);

            if ((files == null) || (files.Count == 0)) return "";

            StorjFile resp = files
                                .Where(y => y.filename != StorjConsts.GET_LOGS_REQUEST)
                                .FirstOrDefault();

            if ((resp.filename != null) && (resp.filename != string.Empty))
            {
                string fullFilePath = Path.GetTempPath() + "/" + resp.filename;

                StorjCore.downloadFile(StorjConsts.BUCKET_LOGS, resp.filename, fullFilePath);

                moveFileToTemp(StorjConsts.BUCKET_LOGS, resp.filename);

                return fullFilePath;
            }

            return "";
        }

        public static void setLogsResponse(string fullLogFilePath)
        {
            StorjCore.uploadFile(StorjConsts.BUCKET_LOGS, fullLogFilePath);
        }

        public static bool getLogsRequest()
        {
            List<StorjFile> files = StorjCore.getFiles(StorjConsts.BUCKET_LOGS);

            if ((files == null) || (files.Count == 0)) return false;

            if (files.Any(y => y.filename == StorjConsts.GET_LOGS_REQUEST))
            {
                moveFileToTemp(StorjConsts.BUCKET_LOGS, StorjConsts.GET_LOGS_REQUEST);

                return true;
            }

            return false;
        }
    }
}
