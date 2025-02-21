using UnityEngine;
using UnityEngine.SceneManagement;

namespace Rcam4 {

public sealed class ResetButton : MonoBehaviour
{
    async Awaitable Start()
    {
        while (true)
        {
            var input = GetComponent<InputHandle>();
            while (!input.Button15) await Awaitable.NextFrameAsync();
            while ( input.Button15) await Awaitable.NextFrameAsync();
            SceneManager.LoadScene(0);
        }
    }
}

} // namespace Rcam4
