using System.Collections.Generic;
using UnityEngine;

namespace FunBoardGames.CantStop
{
    public class WhiteConePanel : MonoBehaviour
    {
        [SerializeField] Transform holder;

        readonly List<RectTransform> slots = new();
        GameObject template;
        GameObject[] cones;

        int currentIndex;

        public Vector2 ConeSize { get; private set; }

        void Awake()
        {
            template = holder.GetChild(0).gameObject;
            ConeSize = ((RectTransform)template.transform).sizeDelta;
            template.SetActive(false);
        }

        public void Initialize(int coneCount)
        {
            foreach (var slot in slots)
                Destroy(slot.gameObject);

            slots.Clear();
            cones = new GameObject[coneCount];
            currentIndex = coneCount - 1;

            for (int i = 0; i < cones.Length; i++)
            {
                var slot = new GameObject("Holder", typeof(RectTransform)) { layer = holder.gameObject.layer };
                var slotRect = (RectTransform)slot.transform;
                slotRect.SetParent(holder, false);
                slotRect.sizeDelta = ConeSize;
                slots.Add(slotRect);

                var cone = Instantiate(template, slotRect);
                var coneRect = (RectTransform)cone.transform;
                coneRect.anchorMin = coneRect.anchorMax = new Vector2(0.5f, 0.5f);
                coneRect.anchoredPosition = Vector2.zero;
                cone.SetActive(true);
                cones[i] = cone;
            }
        }

        public GameObject PullMarker()
        {
            if (currentIndex < 0)
                return null;

            var cone = cones[currentIndex];
            cones[currentIndex] = null;
            currentIndex--;
            return cone;
        }

        public void PushMarker(GameObject cone)
        {
            if (currentIndex >= cones.Length - 1)
                return;

            currentIndex++;
            cones[currentIndex] = cone;
            ConeMover.Of(cone).MoveTo(slots[currentIndex], 1f);
        }
    }
}
