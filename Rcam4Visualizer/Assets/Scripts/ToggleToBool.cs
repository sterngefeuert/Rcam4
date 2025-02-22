using UnityEngine;
using UnityEngine.Events;

namespace Rcam4 {

public sealed class ToggleToBool : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] InputHandle _handle = null;
    [SerializeField] int _toggleIndex = 0;
    [SerializeField] UnityEvent<bool> _event = null;

    #endregion

    #region MonoBehaviour implementation

    void LateUpdate()
      => _event.Invoke(_handle.GetToggle(_toggleIndex));

    #endregion
}

} // namespace Rcam4
