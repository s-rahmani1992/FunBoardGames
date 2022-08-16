using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace OnlineBoardGames
{
    public class MenuUIEventHandler : MonoBehaviour
    {
        public Action<SerializableRoom[]> OnRoomListRefresh;
        public Action OnJoinRoomAttempt;

        private void Awake(){
            SingletonUIHandler.SetInstance(this);
        }

        private void OnDestroy(){
            OnRoomListRefresh = null;
            OnJoinRoomAttempt = null;
            SingletonUIHandler.SetInstance<MonoBehaviour>(null);
        }
    }
}