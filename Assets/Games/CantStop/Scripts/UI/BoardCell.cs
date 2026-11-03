using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace FunBoardGames.CantStop
{
    public class BoardCell : MonoBehaviour
    {
        [SerializeField] Transform coneHolder;
        [SerializeField] float coneScale = 0.55f;

        readonly Dictionary<GameObject, RectTransform> holders = new();

        public void AddCone(GameObject cone, bool animate = true)
        {
            if (holders.ContainsKey(cone))
                return;

            var size = ((RectTransform)cone.transform).sizeDelta * coneScale;

            var holder = new GameObject("Holder", typeof(RectTransform), typeof(LayoutElement)) { layer = coneHolder.gameObject.layer };
            var rect = (RectTransform)holder.transform;
            rect.SetParent(coneHolder, false);
            rect.sizeDelta = size;

            var layout = holder.GetComponent<LayoutElement>();
            layout.preferredWidth = size.x;
            layout.preferredHeight = size.y;

            holders[cone] = rect;
            ConeMover.Of(cone).MoveTo(rect, coneScale, () => ReleaseHolder(cone), animate);
        }

        void ReleaseHolder(GameObject cone)
        {
            if (holders.Remove(cone, out var holder))
                Destroy(holder.gameObject);
        }
    }
}
