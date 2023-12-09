using UnityEngine;

public class WhiteConePanel : MonoBehaviour
{
    [SerializeField] GameObject[] cones;
    
    public void UpdateUI(int coneCount)
    {
        coneCount = Mathf.Clamp(coneCount, 0, cones.Length);

        for (int i = 0; i < coneCount; i++)
            cones[i].SetActive(true);

        for (int i = coneCount; i < cones.Length; i++)
            cones[i].SetActive(false);
    }
}
