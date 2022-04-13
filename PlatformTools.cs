using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public static class PlatformTools
{
    private static string _apppath;

    private static void _SetAppPath(string path)
    {
        _apppath = path;
    }

    public static string GetAppPath()
    {
        return _apppath;
    }
    public static bool PlatformIs(OSPlatform platform)
    {
        return System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(platform);
    }

    public static void CheckPlatform()
    {
        if (PlatformIs(OSPlatform.Windows)) Console.WriteLine("OS Windows");

        if (PlatformIs(OSPlatform.Linux)) Console.WriteLine("OS Linux");

        if (PlatformIs(OSPlatform.OSX)) Console.WriteLine("OS OSX");
    }

    public static void InitAppPath()
    {
        if (PlatformIs(OSPlatform.Windows)) _SetAppPath(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Remove(0, 6));

        if (PlatformIs(OSPlatform.Linux)) _SetAppPath(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Remove(0, 5));

        if (PlatformIs(OSPlatform.OSX)) _SetAppPath(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Remove(0, 5));
    }

    public static string GetSlah()
    {
        if (PlatformIs(OSPlatform.Windows)) return @"\";

        if (PlatformIs(OSPlatform.Linux)) return @"/";

        if (PlatformIs(OSPlatform.OSX)) return @"/";

        return @"\";
    }

}
