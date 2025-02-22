using UnityEngine;
using UnityEngine.Events;

namespace Rcam4 {

public sealed class KnobToFloatCurve : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] InputHandle _handle = null;
    [SerializeField] int _knobIndex = 0;
    [SerializeField] AnimationCurve _curve = null;
    [SerializeField] UnityEvent<float> _event = null;

    #endregion

    #region MonoBehaviour implementation

    void LateUpdate()
      => _event.Invoke(_curve.Evaluate(_handle.GetKnob(_knobIndex)));

    #endregion
}

} // namespace Rcam4
