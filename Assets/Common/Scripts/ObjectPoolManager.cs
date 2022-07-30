using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace OnlineBoardGames
{
    public interface IPoolable
    {
        string ObjectTag { get; set; }
        void OnPush(params object[] parameters);
        void OnPull(params object[] parameters);
    }

    [System.Serializable]
    public class PoolPrefab
    {
        public string tag;
        public GameObject prefab;
    }

    public class ObjectPoolManager : MonoBehaviour
    {
        public PoolPrefab[] prefabs;

        private SortedList<string, List<IPoolable>> _poolList = new SortedList<string, List<IPoolable>>();

        private void Awake(){
            foreach (var p in prefabs)
                _poolList.Add(p.tag, new List<IPoolable>());
        }

        public void Push2List(GameObject gObject, params object[] parameters){
            IPoolable p = gObject.GetComponent<IPoolable>();
            _poolList[p.ObjectTag].Add(p);
            Debug.Log("Pool Push " + _poolList[p.ObjectTag]);
            p.OnPush(parameters);
        }

        public GameObject PullFromList(int tagIndex, params object[] parameters){
            IPoolable p;
            if (_poolList[prefabs[tagIndex].tag].Count == 0){
                Debug.Log("Pool Create " + prefabs[tagIndex].tag);
                p = GameObject.Instantiate(prefabs[tagIndex].prefab).GetComponent<IPoolable>();
                p.ObjectTag = prefabs[tagIndex].tag;
            }
            else{
                Debug.Log("Pool Pull" + prefabs[tagIndex].tag);
                p = _poolList[prefabs[tagIndex].tag][0];
                _poolList[prefabs[tagIndex].tag].RemoveAt(0);
            }
            p.OnPull(parameters);
            return (p as MonoBehaviour).gameObject;
        }
    }
}