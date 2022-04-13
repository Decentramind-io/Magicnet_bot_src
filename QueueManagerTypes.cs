using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Magicnet.QueueManager
{
    public struct NewTask
    {
        public MemoryStream image;
        public string filename;
        public int stylenum;
        public long chatId;
    }
}
