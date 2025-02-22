using UnityEngine;
using UnityEngine.Events;

namespace Rcam4 {

public sealed class KnobToVector2 : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] InputHandle _handle = null;
    [SerializeField] int _knobIndex = 0;
    [SerializeField] Vector2 _minValue = Vector2.zero;
    [SerializeField] Vector2 _maxValue = Vector2.one;
    [SerializeField] UnityEvent<Vector2> _event = null;

    #endregion

    #region MonoBehaviour implementation

    void LateUpdate()
      => _event.Invoke(Vector2.Lerp(_minValue, _maxValue, _handle.GetKnob(_knobIndex)));

    #endregion
}

} // namespace Rcam4
