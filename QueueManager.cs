using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Logging;
using NSUnsplashImageBank;

namespace Magicnet.QueueManager
{
    class QueueManager
    {
        private ConcurrentQueue<NewTask> _queue = new ConcurrentQueue<NewTask>();
        private Func<string, int> _cbImageReady;
        private Func<long, int> _cbImageFiled;
        private UnsplashImageBank _uspl = new UnsplashImageBank();
        private Random _rand = new Random();

        public QueueManager(Func<string, int> cbImageReady, Func<long, int> cbImageFiled)
        {
            _cbImageReady = cbImageReady;
            _cbImageFiled = cbImageFiled;

            DirTools.clearDir(QueueManagerConsts.TASK_IN_DIR);

            _initStorj();

            _QueueProcessing();
        }

        public void AddNewTask(NewTask newtask)
        {
            _queue.Enqueue(newtask);            
        }

        private void _QueueProcessing()
        {
            Storj.StorjTools.clearLogsBucket();
            Storj.StorjTools.moveAllTasksToTemp();

            Task.Run(() => 
            {
                NewTask tsk = new NewTask();
                bool bEmpty = false;

                while (true)
                {
                    try
                    {
                        bEmpty = Storj.StorjTools.tasksBucketIsEmpty();
                    }
                    catch (Exception except)
                    {
                        TLog.ToLogExceptAsync("Magicnet.QueueManager._QueueProcessing storjTools.tasksBucketIsEmpty", except);
                        continue;
                    }
                    
                    if (!bEmpty)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                        continue;
                    }

                    TLog.ToLogAsync("Magicnet.QueueManager._QueueProcessing select task");
                    try
                    {
                        if (_queue.TryDequeue(out tsk))
                        {
                            TLog.ToLogAsync("user image start");
                        }
                        else
                        {
                            TLog.ToLogAsync("random image start");

                            tsk = new NewTask()
                            {
                                chatId = -1,
                                filename = "1.jpg",
                                image = _uspl.getRandomPhoto(),
                                stylenum = _rand.Next(1, 7),
                            };
                        }
                    }
                    catch (Exception except)
                    {
                        TLog.ToLogExceptAsync("Magicnet.QueueManager._QueueProcessing select task", except);
                        if (tsk.chatId != -1) _cbImageFiled(tsk.chatId);

                        continue;
                    }

                    string fullFilePath = string.Empty;

                    TLog.ToLogAsync("Magicnet.QueueManager._QueueProcessing _getNewFilename");
                    try
                    {
                        fullFilePath = QueueManagerConsts.TASK_IN_DIR + "/" + _getNewFilename(tsk);
                    }
                    catch (Exception except)
                    {
                        TLog.ToLogExceptAsync("Magicnet.QueueManager._QueueProcessing _getNewFilename", except);
                        if (tsk.chatId != -1) _cbImageFiled(tsk.chatId);

                        continue;
                    }

                    TLog.ToLogAsync("Magicnet.QueueManager._QueueProcessing save file(" + fullFilePath + ")");
                    try
                    {
                        using (var fileStream = File.Create(fullFilePath))
                        {
                            tsk.image.Seek(0, SeekOrigin.Begin);
                            tsk.image.CopyTo(fileStream);
                        }
                    }
                    catch (Exception except)
                    {
                        TLog.ToLogExceptAsync("Magicnet.QueueManager._QueueProcessing save file(" + fullFilePath + ")", except);
                        if (tsk.chatId != -1) _cbImageFiled(tsk.chatId);

                        continue;
                    }

                    TLog.ToLogAsync("Magicnet.QueueManager._QueueProcessing ImageTools.resizeImage(" + fullFilePath + ")");
                    try
                    {
                        if (PlatformTools.PlatformIs(OSPlatform.Linux)) ImageTools.resizeImageToLinux(fullFilePath);
                    }
                    catch (Exception except)
                    {
                        TLog.ToLogExceptAsync("Magicnet.QueueManager._QueueProcessing ImageTools.resizeImage(" + fullFilePath + ")", except);
                        if (tsk.chatId != -1) _cbImageFiled(tsk.chatId);

                        continue;
                    }

                    TLog.ToLogAsync("Magicnet.QueueManager._QueueProcessing storjTools.uploadTaskFile(" + fullFilePath + ")");
                    try
                    {
                        Storj.StorjTools.uploadTaskFile(fullFilePath);
                    }
                    catch (Exception except)
                    {
                        TLog.ToLogExceptAsync("Magicnet.QueueManager._QueueProcessing storjTools.uploadTaskFile(" + fullFilePath + ")", except);
                        if (tsk.chatId != -1) _cbImageFiled(tsk.chatId);

                        continue;
                    }

                    TLog.ToLogAsync("Magicnet.QueueManager._QueueProcessing File.Delete(" + fullFilePath + ")");
                    try
                    {
                        File.Delete(fullFilePath);
                    }
                    catch (Exception except)
                    {
                        TLog.ToLogExceptAsync("Magicnet.QueueManager._QueueProcessing File.Delete(" + fullFilePath + ")", except);
                        continue;
                    }
                }
            });
        }

        private string _getNewFilename(NewTask newtask)
        {
            string newFilename = "";

            if (newtask.chatId < 0) newFilename = "m" + Math.Abs(newtask.chatId).ToString();
            else newFilename = newtask.chatId.ToString();

            newFilename += "_" + newtask.stylenum.ToString();

            newFilename += "_" + Guid.NewGuid().ToString().Replace("-", "_");

            newFilename += Path.GetExtension(newtask.filename);

            return newFilename;
        }

        private void _initStorj()
        {
            TLog.ToLogAsync("Magicnet.QueueManager._initStorj createBuckets");
            try
            {
                Storj.StorjTools.createBuckets();
            }
            catch(Exception except) 
            {
                TLog.ToLogExceptAsync("Magicnet.QueueManager._initStorj createBuckets", except);
            }

            TLog.ToLogAsync("Magicnet.QueueManager._initStorj clearLogsBucket");
            try
            {
                Storj.StorjTools.clearLogsBucket();
            }
            catch (Exception except)
            {
                TLog.ToLogExceptAsync("Magicnet.QueueManager._initStorj clearLogsBucket", except);
            }

            TLog.ToLogAsync("Magicnet.QueueManager._initStorj clearStatBucket");
            try
            {
                Storj.StorjTools.clearStatBucket();
            }
            catch (Exception except)
            {
                TLog.ToLogExceptAsync("Magicnet.QueueManager._initStorj clearStatBucket", except);
            }

            TLog.ToLogAsync("Magicnet.QueueManager._initStorj clearTasksBucket");
            try
            {
                Storj.StorjTools.clearTasksBucket();
            }
            catch (Exception except)
            {
                TLog.ToLogExceptAsync("Magicnet.QueueManager._initStorj clearTasksBucket", except);
            }

            TLog.ToLogAsync("Magicnet.QueueManager._initStorj clearTempBucket");
            try
            {
                Storj.StorjTools.clearTempBucket();
            }
            catch (Exception except)
            {
                TLog.ToLogExceptAsync("Magicnet.QueueManager._initStorj clearTempBucket", except);
            }  
        }
    }
}
