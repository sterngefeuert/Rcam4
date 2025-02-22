using UnityEngine;

namespace Rcam4 {

public static class PropIDs
{
    public static readonly int Throttle = Shader.PropertyToID("Throttle");
}

public static class AsyncUtil
{
    public static void Forget(Awaitable awaitable) {}
}

} // namespace Rcam4
