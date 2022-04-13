using System;
using System.Collections.Generic;
using System.Text;

namespace mmMagicNetBot
{
    public static class GenAdminButtons
    {
        public const int BTN_OPT_MAX_CNT = 0;
        public const int BTN_OPT_DELTA_MINUTES = 1;
        public const int BTN_OPT_GPU_IND = 2;
        public const int BTN_GET_STAT = 3;
        public const int BTN_GET_LOGS = 4;

        public static botBtn[] AsText = new botBtn[5]
        {
            new botBtn(){caption = "Max img cnt example", data = "Enter, for example: maximgcnt=15", },
            new botBtn(){caption = "Delta minutes example", data = "Enter, for example: deltaminutes=20", },
            new botBtn(){caption = "GPU ind example", data = "Enter, for example: gpuindex=0", },
            new botBtn(){caption = "Get statistics", data = "", },
            new botBtn(){caption = "Get last logs", data = "", },
        };
    }

    public static class tgBotConsts
    {
        public const string INPUT_MAX_IMG_CNT = "maximgcnt=";
        public const string INPUT_DELTA_MINUTES = "deltaminutes=";
        public const string INPUT_GPU_IND = "gpuindex=";
    }
}
