using UnityEngine;
using UnityEngine.Events;

namespace Rcam4 {

public sealed class KnobToVector3 : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] InputHandle _handle = null;
    [SerializeField] int _knobIndex = 0;
    [SerializeField] Vector3 _minValue = Vector3.zero;
    [SerializeField] Vector3 _maxValue = Vector3.one;
    [SerializeField] UnityEvent<Vector3> _event = null;

    #endregion

    #region MonoBehaviour implementation

    void LateUpdate()
      => _event.Invoke(Vector3.Lerp(_minValue, _maxValue, _handle.GetKnob(_knobIndex)));

    #endregion
}

} // namespace Rcam4
