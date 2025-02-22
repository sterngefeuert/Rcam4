using UnityEngine;

namespace Rcam4 {

public static class PropIDs
{
    public static readonly int Throttle = Shader.PropertyToID("Throttle");
    public static readonly int _BackFill = Shader.PropertyToID("_BackFill");
    public static readonly int _BgColors = Shader.PropertyToID("_BgColors");
    public static readonly int _Dithering = Shader.PropertyToID("_Dithering");
    public static readonly int _FgColors = Shader.PropertyToID("_FgColors");
    public static readonly int _FrontFill = Shader.PropertyToID("_FrontFill");
}

public static class AsyncUtil
{
    public static void Forget(Awaitable awaitable) {}
}

} // namespace Rcam4
