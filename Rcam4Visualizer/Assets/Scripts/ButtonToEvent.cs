using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Rcam4 {

public sealed class ButtonToEvent : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] InputHandle _handle = null;
    [SerializeField] int _buttonIndex = 0;
    [SerializeField] InputAction _action = null;
    [SerializeField] UnityEvent _event = null;

    #endregion

    #region Private members

    bool _prev;

    #endregion

    #region MonoBehaviour implementation

    void OnEnable() => _action.Enable();
    void OnDisable() => _action.Disable();

    void Update()
    {
        var current = _handle.GetButton(_buttonIndex);
        if (current && !_prev || _action.triggered) _event.Invoke();
        _prev = current;
    }

    #endregion
}

} // namespace Rcam4
