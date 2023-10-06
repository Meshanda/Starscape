
using UI;
using UnityEngine;

public static class Settings
{
    public static Color cursorColor = Color.white;
    public static float cursorSize = 1;

    public static float volumeMusic = 1;
    public static float volumeFx = 1;

    public static SelectorMode fullScreenMode = SelectorMode.FullScreen;
    public static SelectorResolution resolution = SelectorResolution.FullHD;
    public static SelectorFPS targetFPS = SelectorFPS.Sixty;
    public static bool toggleVsync = false;

    public static bool IsFullScreen => fullScreenMode == SelectorMode.FullScreen;
}