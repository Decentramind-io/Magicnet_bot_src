using System;
using System.Collections.Generic;
using System.Text;

namespace Magicnet.Storj
{
    public struct StorjFile
    {
        public DateTime dt;
        public string filename;
        public int filesize;
    }

    public struct StorjBucket
    {
        public DateTime dt;
        public string name;
    }
}
