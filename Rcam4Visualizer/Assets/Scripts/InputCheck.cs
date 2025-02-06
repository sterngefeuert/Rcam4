using UnityEngine;

namespace Rcam4 {

public sealed class InputLinker : MonoBehaviour
{
    #region Scene object references

    [SerializeField] FrameDecoder _decoder = null;

    #endregion

    #region Private members

    InputHandle _handle;

    #endregion

    #region MonoBehaviour implementation

    void Start()
      => _handle = GetComponent<InputHandle>();

    void Update()
    {
        _handle.InputState = _decoder.Metadata.InputState;
        Debug.Log(_handle.Knob0);
    }

    #endregion
}

} // namespace Rcam4
