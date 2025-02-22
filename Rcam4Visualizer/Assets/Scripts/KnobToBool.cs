using UnityEngine;
using UnityEngine.Events;

namespace Rcam4 {

public sealed class KnobToBool : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] InputHandle _handle = null;
    [SerializeField] int _knobIndex = 0;
    [SerializeField] float _threshold = 0.5f;
    [SerializeField] UnityEvent<bool> _event = null;

    #endregion

    #region MonoBehaviour implementation

    void LateUpdate()
      => _event.Invoke(_handle.GetKnob(_knobIndex) > _threshold);

    #endregion
}

} // namespace Rcam4
