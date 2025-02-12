using UnityEngine;

namespace Rcam4 {

[RequireComponent(typeof(Camera))]
[AddComponentMenu("Rcam/Recolor Effect")]
public sealed class RecolorEffect : MonoBehaviour
{
    #region Public properties

    public bool IsReady => Properties != null;
    public MaterialPropertyBlock Properties { get; private set; }

    #endregion

    #region MonoBehaviour implementation

    void LateUpdate()
    {
        if (Properties == null) Properties = new MaterialPropertyBlock();
    }

    #endregion
}

} // namespace Rcam4
