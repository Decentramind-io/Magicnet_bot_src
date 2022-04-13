using Magicnet.CheckSafeDos;
using Magicnet.QueueManager;
using Logging;
using mmMagicNetBot;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AppSettings;

namespace MagicnetBot2
{
    class Program
    {
        private static CheckSafeDos _checkSafeDos;
        private static tbBot _bot;
        private static QueueManager _queueManager = new QueueManager(_cbImageReady, _cbImageFiled);

        static void Main(string[] args)
        {
            PlatformTools.CheckPlatform();
            PlatformTools.InitAppPath();

            AppSettingsRepo.LoadAppSettings(PlatformTools.GetAppPath() + PlatformTools.GetSlah() + "config.json");

            TLog.ModifyFilePath(PlatformTools.GetAppPath() + PlatformTools.GetSlah());

            TLog.ToLogAsync("MAX_CNT " + args[0]);
            TLog.ToLogAsync("DELTA_MINUTES " + args[1]);

            _checkSafeDos = new CheckSafeDos(int.Parse(args[0]), int.Parse(args[1]));

            _bot = new tbBot(AppSettingsRepo.tg_apikey, _cbAddNewTask, _cbCheckSafeDos, _cbUpdateAppParams, _cbGetStat, _cbGetLogs);

            Thread.Sleep(-1);
        }

        private static int _cbAddNewTask(NewTask newtask)
        {
            _bot.SetMessage(newtask.chatId, "Processing… it will take half of minute");

            _queueManager.AddNewTask(newtask);

            return 0;
        }

        private static bool _cbCheckSafeDos(long chatId)
        {
            return _checkSafeDos.checkUserId(chatId);
        }

        private static int _cbUpdateAppParams(UpdateParam param)
        {
            if (param.note == tgBotConsts.INPUT_MAX_IMG_CNT) _checkSafeDos.updateParamMaxCnt(int.Parse(param.value));

            if (param.note == tgBotConsts.INPUT_DELTA_MINUTES) _checkSafeDos.updateParamDeltaMinutes(int.Parse(param.value));

            return 0;
        }

        private static int _cbImageReady(string image)
        {
            _bot.SetImageInChan(image);

            return 0;
        }

        private static int _cbImageFiled(long chatid)
        {
            _bot.SetImageFiled(chatid);

            return 0;
        }

        private static int _cbGetStat(long chatId)
        {
            Task<int>.Run(() =>
            {
                try
                {
                    Magicnet.Storj.StorjTools.setStatRequest();
                }
                catch (Exception except)
                {
                    TLog.ToLogExceptAsync("_cbGetStat storjTools.setStatRequest", except);
                }

                DateTime start = DateTime.Now;
                string stat = string.Empty;
                while ((DateTime.Now - start).TotalSeconds < 61)
                {
                    try
                    {
                        stat = Magicnet.Storj.StorjTools.getStatResponse();
                    }
                    catch (Exception except)
                    {
                        TLog.ToLogExceptAsync("_cbGetStat storjTools.getStatResponse", except);
                    }

                    if (stat != string.Empty) break;

                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }

                if (stat != string.Empty) _bot.SetMessage(chatId, stat);
                else _bot.SetMessage(chatId, "Oh! Failed to get statistics");
            });            

            return 0;
        }

        private static int _cbGetLogs(long chatId)
        {
            Task<int>.Run(() =>
            {
                try
                {
                    Magicnet.Storj.StorjTools.setLogsRequest();
                }
                catch (Exception except)
                {
                    TLog.ToLogExceptAsync("_cbGetLogs storjTools.setLogsRequest", except);
                }

                DateTime start = DateTime.Now;
                string logs = string.Empty;
                while ((DateTime.Now - start).TotalSeconds < 61)
                {
                    try
                    {
                        logs = Magicnet.Storj.StorjTools.getLogsResponse();
                    }
                    catch (Exception except)
                    {
                        TLog.ToLogExceptAsync("_cbGetLogs storjTools.getLogsResponse", except);
                    }

                    if (logs != string.Empty) break;

                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }

                if (logs != string.Empty) _bot.SetFile(chatId, logs);
                else _bot.SetMessage(chatId, "Oh! Failed to get logs core");

                _bot.SetFile(chatId, TLog.GetFilePath());
            });

            return 0;
        }

    }
}
