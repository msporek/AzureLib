using System;
using System.Runtime.InteropServices;

namespace AzureLib;

public class InternetOptionsHandler
{
    public void ClearCookies()
    {
        const int INTERNET_OPTION_END_BROWSER_SESSION = 42;
        InternetSetOption(IntPtr.Zero, INTERNET_OPTION_END_BROWSER_SESSION, IntPtr.Zero, 0);
    }

    [DllImport("wininet.dll", SetLastError = true)]
    private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);
}
