using UnityEngine;

namespace OnlineBoardGames.CantStop
{
    public class BoardCell : MonoBehaviour
    {
        [SerializeField] Transform coneHolder;

        public void AddCone(GameObject cone)
        {
            cone.transform.SetParent(coneHolder);
        }
    }
}
