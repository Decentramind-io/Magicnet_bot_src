using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public static class DirTools
{
    public static void clearDir(string dir)
    {
        DirectoryInfo di = new DirectoryInfo(dir);

        foreach (FileInfo file in di.GetFiles())
        {
            try { file.Delete(); }
            catch (Exception except)
            {

            };
        }
    }
}
