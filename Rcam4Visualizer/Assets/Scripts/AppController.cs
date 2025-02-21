using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace Rcam4 {

public sealed class AppController : MonoBehaviour
{
    #region Scene object references

    [SerializeField] UIDocument _ui = null;
    [SerializeField] FrameDecoder _decoder = null;
    [SerializeField] InputHandle _inputHandle = null;

    #endregion

    #region Private members

    VisualElement _uiDisplay;

    void ToggleUI()
      => _uiDisplay.visible = (Cursor.visible ^= true);

    #endregion

    #region MonoBehaviour implementation

    async Awaitable Start()
    {
        var root = _ui.rootVisualElement;
        _uiDisplay = root.Q("display");

        // UI root as a clickable UI visibility toggle
        root.AddManipulator(new Clickable(ToggleUI));

        // Initially hidden UI
        _uiDisplay.visible = Cursor.visible = false;

        // Reset button wait
        while (!_inputHandle.Button15) await Awaitable.NextFrameAsync();
        while ( _inputHandle.Button15) await Awaitable.NextFrameAsync();

        // Reset
        SceneManager.LoadScene(0);
    }

    void Update()
      => _inputHandle.InputState = _decoder.Metadata.InputState;

    #endregion
}

} // namespace Rcam4
