using System;
using System.Collections.Generic;
using System.Text;

namespace AppSettings
{
    public static class AppSettingsRepo
    {
        private static string _tg_apikey;
        public static string tg_apikey
        {
            get { return _tg_apikey; }
            set
            {
                _tg_apikey = value;
            }
        }


        private static long _tg_gallery_chatId;
        public static long tg_gallery_chatId
        {
            get { return _tg_gallery_chatId; }
            set
            {
                _tg_gallery_chatId = value;
            }
        }


        private static string _tg_admin_pass;
        public static string tg_admin_pass
        {
            get { return _tg_admin_pass; }
            set
            {
                _tg_admin_pass = value;
            }
        }


        public static void LoadAppSettings(string filePath)
        {
            ConfigParams.setConfigFilePath(filePath);

            _tg_apikey = ConfigParams.getValueAsString("tg_apikey");

            _tg_gallery_chatId = ConfigParams.getValueAsLong("tg_gallery_chatId");

            _tg_admin_pass = ConfigParams.getValueAsString("tg_admin_pass");
        }
    }
}
