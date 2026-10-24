using System.Collections.Generic;
using UnityEngine;

public class WhiteConePanel : MonoBehaviour
{
    [SerializeField] Transform holder;

    readonly List<GameObject> cones = new();
    GameObject template;

    void Awake()
    {
        template = holder.GetChild(0).gameObject;
        cones.Add(template);
    }

    public void UpdateUI(int coneCount)
    {
        coneCount = Mathf.Max(coneCount, 0);

        while (cones.Count < coneCount)
            cones.Add(Instantiate(template, holder));

        for (int i = 0; i < cones.Count; i++)
            cones[i].SetActive(i < coneCount);
    }
}
