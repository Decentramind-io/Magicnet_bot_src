using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

public static class ImageTools
{
    public static void resizeImageToLinux(string filepath)
    {
        string strCmdText;
        strCmdText = "-resize 1920x1080 " + filepath;

        var process = System.Diagnostics.Process.Start("mogrify", strCmdText);
        process.WaitForExit(1000 * 20);
    }

    public static void addBorderImageToLinux(string filepath)
    {
        int w = 0;
        int h = 0;

        if (!getImageSzPxlToLinux(filepath, out w, out h)) return;

        if (h > w)
        {
            float new_w = 3 / 2 * h;
            double add_w = (new_w - w) / 2;

            //coef
            add_w *= 2;

            _addBorderImageToLinux(filepath, (int)add_w);
        }
    }

    private static void _addBorderImageToLinux(string filepath, int width)
    {
        string strCmdText;

        strCmdText = filepath + " -background black -gravity northwest -splice " + width.ToString() + "x0 " + filepath;
        var process = System.Diagnostics.Process.Start("convert", strCmdText);
        process.WaitForExit(1000 * 20);

        strCmdText = filepath + " -background black -gravity east -splice " + width.ToString() + "x " + filepath;
        process = System.Diagnostics.Process.Start("convert", strCmdText);
        process.WaitForExit(1000 * 20);
    }

    public static bool getImageSzPxlToLinux(string filepath, out int w, out int h)
    {
        w = 0;
        h = 0;

        Process process = new Process();
        process.StartInfo = new ProcessStartInfo("identify", "-format \" %[fx: w]x %[fx: h]\" " + filepath);
        process.StartInfo.RedirectStandardOutput = true;
        process.Start();
        process.WaitForExit(1000 * 20);

        StreamReader sr = process.StandardOutput;

        if (sr == null) return false;

        string[] size = sr.ReadToEnd().Split("x");

        if ((size == null) || (size.Length != 2)) return false;

        if (!int.TryParse(size[0], out w)) return false;

        if (!int.TryParse(size[1], out h)) return false;

        return true;
    }

    public static bool getImagePxlsToLinux(string filepath, out long pxls)
    {
        pxls = 0;
        int w = 0;
        int h = 0;

        if (!getImageSzPxlToLinux(filepath, out w, out h)) return false;

        pxls = w * h;

        return true;
    }
}
