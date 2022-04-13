using Magicnet.QueueManager;
using Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Args;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;
using System.Threading.Tasks;
using AppSettings;

namespace mmMagicNetBot
{
    class tbBot
    {
        private TelegramBotClient _botClient;
        private Func<NewTask, int> _cbNewImage;
        private Func<long, bool> _cbCheckSafeDos;
        private Func<UpdateParam, int> _cbUpdateParams;
        private Func<long, int> _cbGetStat;
        private Func<long, int> _cbGetLogs;
        private ConcurrentDictionary<long, int> _dInlinesSelStyle = new ConcurrentDictionary<long, int>();
        private ConcurrentDictionary<long, List<NewTask>> _dUsersPhotos = new ConcurrentDictionary<long, List<NewTask>>();
        private long _adminId = 0;

        public tbBot(string ttoken, 
                     Func<NewTask, int> cbNewImage, 
                     Func<long, bool> cbCheckSafeDos, 
                     Func<UpdateParam, int> cbUpdateParams,
                     Func<long, int> cbGetStat,
                     Func<long, int> cbGetLogs
                     )
        {
            _cbNewImage = cbNewImage;
            _cbCheckSafeDos = cbCheckSafeDos;
            _cbUpdateParams = cbUpdateParams;
            _cbGetStat = cbGetStat;
            _cbGetLogs = cbGetLogs;

            _botClient = new TelegramBotClient(ttoken);
            _botClient.OnMessage += HandleMessage;
            _botClient.OnCallbackQuery += CallbackQuery;
            _botClient.StartReceiving();
        }

        private async void HandleMessage(object sender, MessageEventArgs messageEventArgs)
        {
            switch (messageEventArgs.Message.Type)
            {
                case MessageType.Text:
                    //register admin mode
                    if (messageEventArgs.Message.Text == AppSettingsRepo.tg_admin_pass)
                    {
                        _botClient.DeleteMessageAsync(messageEventArgs.Message.Chat.Id, messageEventArgs.Message.MessageId);

                        if (_adminId == messageEventArgs.Message.Chat.Id)
                        {
                            _botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id,
                                                            "You are already an admin",
                                                            replyMarkup: _CreateGenButtons());

                            return;
                        }

                        _adminId = messageEventArgs.Message.Chat.Id;
                        _botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id,
                                                            ">>>>>>Admin mode enabled",
                                                            replyMarkup: _CreateGenButtons());

                        return;
                    }

                    if (_adminId == messageEventArgs.Message.Chat.Id)
                    {
                        //check press admin buttons
                        foreach (botBtn btn in GenAdminButtons.AsText)
                        {
                            if (messageEventArgs.Message.Text == btn.caption)
                            {
                                string mess = btn.data;

                                //get stat
                                if (messageEventArgs.Message.Text == GenAdminButtons.AsText[GenAdminButtons.BTN_GET_STAT].caption)
                                {
                                    mess = "Please wait...";
                                    _cbGetStat(messageEventArgs.Message.Chat.Id);
                                };

                                //get logs
                                if (messageEventArgs.Message.Text == GenAdminButtons.AsText[GenAdminButtons.BTN_GET_LOGS].caption)
                                {
                                    mess = "Please wait...";
                                    _cbGetLogs(messageEventArgs.Message.Chat.Id);                                    
                                }

                                _botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id,
                                                                mess,
                                                                replyMarkup: _checkAdminMode(messageEventArgs.Message.Chat.Id));

                                return;
                            }
                        };

                        //check input admin params
                        if (messageEventArgs.Message.Text.StartsWith(tgBotConsts.INPUT_MAX_IMG_CNT))
                        {
                            int outval = 0;
                            if (!int.TryParse(messageEventArgs.Message.Text.Replace(tgBotConsts.INPUT_MAX_IMG_CNT, ""), out outval))
                            {
                                _botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id,
                                                            "parse error(" + messageEventArgs.Message.Text + ")",
                                                            replyMarkup: _checkAdminMode(messageEventArgs.Message.Chat.Id));
                                return;
                            }

                            _cbUpdateParams(new UpdateParam() { note = tgBotConsts.INPUT_MAX_IMG_CNT, value = outval.ToString() });

                            _botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id,
                                                            "ok(" + outval.ToString() + ")",
                                                            replyMarkup: _checkAdminMode(messageEventArgs.Message.Chat.Id));

                            return;
                        }
                        if (messageEventArgs.Message.Text.StartsWith(tgBotConsts.INPUT_DELTA_MINUTES))
                        {
                            int outval = 0;
                            if (!int.TryParse(messageEventArgs.Message.Text.Replace(tgBotConsts.INPUT_DELTA_MINUTES, ""), out outval))
                            {
                                _botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id,
                                                            "parse error(" + messageEventArgs.Message.Text + ")",
                                                            replyMarkup: _checkAdminMode(messageEventArgs.Message.Chat.Id));
                                return;
                            }

                            _cbUpdateParams(new UpdateParam() { note = tgBotConsts.INPUT_DELTA_MINUTES, value = outval.ToString() });

                            _botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id,
                                                            "ok(" + outval.ToString() + ")",
                                                            replyMarkup: _checkAdminMode(messageEventArgs.Message.Chat.Id));

                            return;
                        }

                        if (messageEventArgs.Message.Text.StartsWith(tgBotConsts.INPUT_GPU_IND))
                        {
                            int outval = 0;
                            if (!int.TryParse(messageEventArgs.Message.Text.Replace(tgBotConsts.INPUT_GPU_IND, ""), out outval))
                            {
                                _botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id,
                                                            "parse error(" + messageEventArgs.Message.Text + ")",
                                                            replyMarkup: _checkAdminMode(messageEventArgs.Message.Chat.Id));
                                return;
                            }

                            _cbUpdateParams(new UpdateParam() { note = tgBotConsts.INPUT_GPU_IND, value = outval.ToString() });

                            _botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id,
                                                            "ok(" + outval.ToString() + ")",
                                                            replyMarkup: _checkAdminMode(messageEventArgs.Message.Chat.Id));

                            return;
                        }
                    }

                    _botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id, 
                                                    "Hey! Send me a image and see the magic in @mm_magicnet",
                                                    replyMarkup: _checkAdminMode(messageEventArgs.Message.Chat.Id));

                    break;

                case MessageType.Document:
                    if (messageEventArgs.Message.Document.MimeType.StartsWith("image/"))
                    {
                        if (messageEventArgs.Message.Chat.Id != _adminId)
                        {
                            if (!_cbCheckSafeDos(messageEventArgs.Message.Chat.Id))
                            {
                                SetImageSafeDos(messageEventArgs.Message.Chat.Id);
                                break;
                            }
                        }                        

                        NewTask tsk = _GetTaskByDoc(messageEventArgs.Message.Document, messageEventArgs.Message.Chat.Id);

                        List<NewTask> tsklst2 = null;
                        if (_dUsersPhotos.TryGetValue(messageEventArgs.Message.Chat.Id, out tsklst2))
                            tsklst2.Add(tsk);
                        else tsklst2 = new List<NewTask>() { tsk, };

                        _dUsersPhotos.AddOrUpdate(messageEventArgs.Message.Chat.Id, tsklst2, (key, oldValue) => tsklst2);

                        _ShowInlineSelStyle(messageEventArgs.Message.Chat.Id);
                    }
                    else _botClient.SendTextMessageAsync(messageEventArgs.Message.Chat.Id, 
                                                         "Oh! For magic you need an image",
                                                         replyMarkup: _checkAdminMode(messageEventArgs.Message.Chat.Id));

                    break;

                case MessageType.Photo:

                    if (messageEventArgs.Message.Chat.Id != _adminId)
                    {
                        if (!_cbCheckSafeDos(messageEventArgs.Message.Chat.Id))
                        {
                            SetImageSafeDos(messageEventArgs.Message.Chat.Id);
                            break;
                        }
                    }

                    NewTask tsk2 = _GetTaskByPhoto(messageEventArgs.Message.Photo, messageEventArgs.Message.Chat.Id);

                    List<NewTask> tsklst = null;
                    if (_dUsersPhotos.TryGetValue(messageEventArgs.Message.Chat.Id, out tsklst))
                        tsklst.Add(tsk2);
                    else tsklst = new List<NewTask>() { tsk2, };

                    _dUsersPhotos.AddOrUpdate(messageEventArgs.Message.Chat.Id, tsklst, (key, oldValue) => tsklst);

                    _ShowInlineSelStyle(messageEventArgs.Message.Chat.Id);

                    break;
            }
        }

        private void CallbackQuery(object sc, Telegram.Bot.Args.CallbackQueryEventArgs ev)
        {
            List<NewTask> tsklst = null;
            if (_dUsersPhotos.TryGetValue(ev.CallbackQuery.Message.Chat.Id, out tsklst))
            {
                _RemoveInlineSelStyle(ev.CallbackQuery.Message.Chat.Id);

                List<NewTask> tsklsttmp = null;
                _dUsersPhotos.TryRemove(ev.CallbackQuery.Message.Chat.Id, out tsklsttmp);

                NewTask temptsk;
                foreach (NewTask tsk in tsklst)
                {
                    temptsk = tsk;
                    temptsk.stylenum = int.Parse(ev.CallbackQuery.Data);
                    _cbNewImage(temptsk);
                }
            }

            _botClient.AnswerCallbackQueryAsync(ev.CallbackQuery.Id);
        }

        public void SetImageInChan(string image)
        {
            Telegram.Bot.Types.IAlbumInputMedia[] inputMedia = new IAlbumInputMedia[1];
            inputMedia[0] = new InputMediaPhoto(new InputMedia(new MemoryStream(System.IO.File.ReadAllBytes(image)), "1"));
            _botClient.SendMediaGroupAsync(AppSettingsRepo.tg_gallery_chatId, inputMedia, false);
        }

        public void SetImageFiled(long chatid)
        {
            _botClient.SendTextMessageAsync(chatid, "Oh! Something went wrong( Please try another image", replyMarkup: _checkAdminMode(chatid));
        }

        public void SetImageSafeDos(long chatid)
        {
            _botClient.SendTextMessageAsync(chatid, "Oh! Magic takes time, please try again later", replyMarkup: _checkAdminMode(chatid));
        }

        public async void SetMessage(long chatid, string mess)
        {
            await _botClient.SendTextMessageAsync(chatid, mess, replyMarkup: _checkAdminMode(chatid));
        }

        public async void SetFile(long chatid, string fullFilePath)
        {
            InputOnlineFile iof = new InputOnlineFile(System.IO.File.OpenRead(fullFilePath), Path.GetFileName(fullFilePath));

            try
            {
                await _botClient.SendDocumentAsync(chatid, iof, replyMarkup: _checkAdminMode(chatid));
            }
            catch (Exception except)
            {
                TLog.ToLogExceptAsync("tgbot SetFile(" + chatid.ToString() + ", " + fullFilePath + ")", except);
            }
        }

        private NewTask _GetTaskByDoc(Document doc, long chatId)
        {
            var fileId = doc.FileId;

            return _GetTask(fileId, chatId);
        }

        private NewTask _GetTaskByPhoto(PhotoSize[] photo, long chatId)
        {
            var fileId = photo.LastOrDefault()?.FileId;
            
            return _GetTask(fileId, chatId);
        }

        private NewTask _GetTask(string fileId, long chatId)
        {
            var file = _botClient.GetFileAsync(fileId).GetAwaiter().GetResult();
            MemoryStream ms = new MemoryStream();
            _botClient.DownloadFileAsync(file.FilePath, ms).GetAwaiter().GetResult();

            return new NewTask()
            {
                filename = Path.GetFileName(file.FilePath),
                image = ms,
                chatId = chatId,
            };
        }

        private InlineKeyboardButton[][] _CreateBtnsStyles()
        {
            InlineKeyboardButton[][] result = new InlineKeyboardButton[2][];

            result[0] = new InlineKeyboardButton[3];
            result[0][0] = new InlineKeyboardButton
            {
                Text = "Wave",
                CallbackData = "1",
            };
            result[0][1] = new InlineKeyboardButton
            {
                Text = "La muse",
                CallbackData = "2",
            };
            result[0][2] = new InlineKeyboardButton
            {
                Text = "Rain princess",
                CallbackData = "3",
            };

            result[1] = new InlineKeyboardButton[3];
            result[1][0] = new InlineKeyboardButton
            {
                Text = "Scream",
                CallbackData = "4",
            };
            result[1][1] = new InlineKeyboardButton
            {
                Text = "Udnie",
                CallbackData = "5",
            };
            result[1][2] = new InlineKeyboardButton
            {
                Text = "Wreck",
                CallbackData = "6",
            };

            return result;
        }

        private void _ShowInlineSelStyle(long chatId)
        {
            var keyboardInline = _CreateBtnsStyles();
            int messId = 0;
            if (_dInlinesSelStyle.TryGetValue(chatId, out messId))
                _botClient.DeleteMessageAsync(chatId, messId);
            var ikm = new InlineKeyboardMarkup(keyboardInline);

            messId = _botClient.SendTextMessageAsync(chatId,
                                    "Almost done! It remains to choose the style of the picture",
                                    replyMarkup: ikm).GetAwaiter().GetResult().MessageId;

            _dInlinesSelStyle.AddOrUpdate(chatId, messId, (key, oldValue) => messId);
        }

        private void _RemoveInlineSelStyle(long chatId)
        {
            int messId = 0;
            if (_dInlinesSelStyle.TryGetValue(chatId, out messId))
                _botClient.DeleteMessageAsync(chatId, messId);

            _dInlinesSelStyle.TryRemove(chatId, out messId);

        }

        private IReplyMarkup _checkAdminMode(long chatid)
        {
            if (chatid != _adminId) return new ReplyKeyboardRemove();

            return null;
        }


        private ReplyKeyboardMarkup _CreateGenButtons()
        {
            ReplyKeyboardMarkup rkm = new ReplyKeyboardMarkup();

            rkm.Keyboard =
            new KeyboardButton[][]
            {
                new KeyboardButton[3]
                {
                    new KeyboardButton(GenAdminButtons.AsText[GenAdminButtons.BTN_OPT_MAX_CNT].caption),
                    new KeyboardButton(GenAdminButtons.AsText[GenAdminButtons.BTN_OPT_DELTA_MINUTES].caption),
                    new KeyboardButton(GenAdminButtons.AsText[GenAdminButtons.BTN_OPT_GPU_IND].caption),
                },

                new KeyboardButton[2]
                {
                    new KeyboardButton(GenAdminButtons.AsText[GenAdminButtons.BTN_GET_STAT].caption),
                    new KeyboardButton(GenAdminButtons.AsText[GenAdminButtons.BTN_GET_LOGS].caption),
                },
            };

            return rkm;
        }


    }
}
