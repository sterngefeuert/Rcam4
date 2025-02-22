using UnityEngine;
using UnityEngine.Events;

namespace Rcam4 {

public sealed class KnobToColor : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] InputHandle _handle = null;
    [SerializeField] int _knobIndex = 0;
    [SerializeField] Gradient _gradient = null;
    [SerializeField] UnityEvent<Color> _event = null;

    #endregion

    #region MonoBehaviour implementation

    void LateUpdate()
      => _event.Invoke(_gradient.Evaluate(_handle.GetKnob(_knobIndex)));

    #endregion
}

} // namespace Rcam4
