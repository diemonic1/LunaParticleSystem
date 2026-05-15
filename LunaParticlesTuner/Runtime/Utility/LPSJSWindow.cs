#if UNITY_LUNA
using Bridge;

[External]
[Name("window")]
public static class LPSJSWindow
{
    public static string windowLPSData; // the name must be super-unique and must begin with a small letter
    public static string windowLPSDataTimeScale;
    public static string windowLPSDataEnableLogs;
    public static extern void windowLPSsendToPage(string data);
}
#endif