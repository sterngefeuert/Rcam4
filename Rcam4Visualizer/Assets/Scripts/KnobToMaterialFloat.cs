using UnityEngine;

namespace Rcam4 {

public sealed class KnobToMaterialFloat : MonoBehaviour
{
    #region Editable attributes

    [SerializeField] InputHandle _handle = null;
    [SerializeField] int _knobIndex = 0;
    [SerializeField] Renderer _target = null;
    [SerializeField] string _property = "_Throttle";

    #endregion

    #region Private members

    MaterialPropertyBlock _props;

    #endregion

    #region MonoBehaviour implementation

    void Start()
      => _props = new MaterialPropertyBlock();

    void LateUpdate()
    {
        _target.GetPropertyBlock(_props);
        _props.SetFloat(_property, _handle.GetKnob(_knobIndex));
        _target.SetPropertyBlock(_props);
    }

    #endregion
}

} // namespace Rcam4
