using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Utils {

public class ClickFilter
{
    public static bool HitUI() {
        var pe = new PointerEventData(EventSystem.current);
        pe.position = Input.mousePosition;
        var raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pe, raycastResults);
        return raycastResults.Count > 0;
    }
}

} // namespace LiquidShader
