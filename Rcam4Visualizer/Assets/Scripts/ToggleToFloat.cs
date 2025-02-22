using UnityEngine;
using UnityEngine.Events;

namespace Rcam4 {

public sealed class ToggleToFloat : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] InputHandle _handle = null;
    [SerializeField] int _toggleIndex = 0;
    [SerializeField] UnityEvent<float> _event = null;
    [SerializeField] float _transitionTime = 1;

    #endregion

    #region Private members

    float _value;

    #endregion

    #region MonoBehaviour implementation

    void LateUpdate()
    {
        var flag = _handle.GetToggle(_toggleIndex);

        var dt = Time.deltaTime;
        var delta = (flag ? 1 : -1) * dt / _transitionTime;
        _value = Mathf.Clamp01(_value + delta);

        _event.Invoke(_value);
    }

    #endregion
}

} // namespace Rcam4
