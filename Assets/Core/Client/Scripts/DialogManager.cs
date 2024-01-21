using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace OnlineBoardGames
{
    public enum DialogShowOptions
    {
        OverAll,
        Replace
    }

    public class DialogManager : MonoBehaviour
    {
        [SerializeField] GraphicRaycaster raycaster;
        [SerializeField] BaseDialog[] allDialog;

        List<BaseDialog> opendialogs = new();
        public static DialogManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                DontDestroyOnLoad(gameObject);
                Instance = this;
            }

            else
                DestroyImmediate(gameObject);
        }

        private void Start()
        {
            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;
        }

        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            GetComponent<Canvas>().worldCamera = Camera.main;
            CloseAllDialogs();
        }

        public void ShowDialog<TDialog>(TDialog dialog, DialogShowOptions showOption) where TDialog : BaseDialog
        {
            var createdDialog = Instantiate(dialog, transform);
            ArrangeDialog(createdDialog, showOption);
        }

        public void ShowDialog<TDialog, TData>(TDialog dialog, DialogShowOptions showOption, TData data) where TDialog : BaseDialog, IDataDialog<TData>
        {
            var createdDialog = Instantiate(dialog, transform);
            createdDialog.Initialize(data);
            ArrangeDialog(createdDialog, showOption);
        }

        public T SpawnDialog<T>(DialogShowOptions option, System.Action<BaseDialog> onSpawn = null) where T : BaseDialog
        {
            T dialog = null;
            foreach (var d in allDialog)
            {
                if (d is T)
                {
                    dialog = Instantiate(d.gameObject, transform).GetComponent<T>();
                    break;
                }
            }

            if (dialog == null) return dialog;
            dialog.OnSpawned = onSpawn;
            ArrangeDialog(dialog, option);
            return dialog;
        }

        void ArrangeDialog(BaseDialog dialog, DialogShowOptions option)
        {
            if (option == DialogShowOptions.OverAll)
            {
                if (opendialogs.Count == 0)
                    opendialogs.Add(dialog);
                else
                    opendialogs.Insert(0, dialog);
                StartCoroutine(ShowAsync(dialog));
            }

            else
            {
                if (opendialogs.Count == 0)
                {
                    opendialogs.Add(dialog);
                    StartCoroutine(ShowAsync(dialog));
                }
                else
                {
                    var d = opendialogs[opendialogs.Count - 1];
                    opendialogs[opendialogs.Count - 1] = dialog;
                    StartCoroutine(CloseShowAsync(d, dialog));
                }
            }
            dialog.canvas.sortingOrder = opendialogs.Count;
        }

        public void CloseDialog(BaseDialog dialog)
        {
            if (opendialogs.Contains(dialog))
            {
                opendialogs.Remove(dialog);
                StartCoroutine(CloseAsync(dialog));
            }
        }

        public void CloseDialog<T>()
        {
            foreach (var d in opendialogs)
            {
                if (d is T)
                {
                    CloseDialog(d);
                    break;
                }
            }
        }

        void RefreshDialogs()
        {
            if (opendialogs.Count > 0)
            {
                raycaster.enabled = true;
                for (int i = 0; i < opendialogs.Count; i++)
                {
                    opendialogs[i].rayCast.enabled = false;
                    opendialogs[i].canvas.sortingOrder = i + 1;
                }
                opendialogs[opendialogs.Count - 1].rayCast.enabled = true;
            }
            else
                raycaster.enabled = false;
        }

        IEnumerator CloseAsync(BaseDialog dialog)
        {
            dialog.OnClose();
            yield return new WaitForSeconds(dialog.closeTime);
            Destroy(dialog.gameObject);
            RefreshDialogs();
        }

        IEnumerator ShowAsync(BaseDialog dialog)
        {
            dialog.Show();
            yield return new WaitForSeconds(dialog.showTime);
            RefreshDialogs();
        }

        IEnumerator CloseShowAsync(BaseDialog oldDialog, BaseDialog newDialog)
        {
            oldDialog.OnClose();
            yield return new WaitForSeconds(oldDialog.closeTime);
            Destroy(oldDialog.gameObject);
            newDialog.Show();
            yield return new WaitForSeconds(newDialog.showTime);
            RefreshDialogs();
        }

        public void CloseAllDialogs()
        {
            foreach (var d in opendialogs)
                Destroy(d.gameObject);

            opendialogs.Clear();
        }
    }
}