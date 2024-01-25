using UnityEngine;

namespace FunBoardGames.CantStop
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
