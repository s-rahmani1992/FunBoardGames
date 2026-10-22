using System;
using System.Collections.Generic;
using UnityEngine;

namespace FunBoardGames.Tutorial
{
    public class TutorialRunner : MonoBehaviour
    {
        const float AnyClickLockSeconds = 0.35f;

        TutorialOverlay overlay;
        List<TutorialStep> steps;
        int index = -1;
        TutorialStep current;
        bool isShown;
        bool advancePending;
        float shownAt;

        public bool IsRunning => current != null;
        public int StepIndex => index;

        public event Action Completed;
        public event Action Skipped;

        public static TutorialRunner Create(TutorialOverlay overlay)
        {
            var runner = new GameObject("TutorialRunner").AddComponent<TutorialRunner>();
            runner.overlay = overlay;
            overlay.SkipClicked += runner.OnSkipClicked;
            return runner;
        }

        public void Run(IEnumerable<TutorialStep> tutorialSteps)
        {
            steps = new List<TutorialStep>(tutorialSteps);
            index = -1;
            MoveNext();
        }

        void OnSkipClicked()
        {
            Stop();
            Skipped?.Invoke();
        }

        public void Stop()
        {
            if (current != null && isShown)
                overlay.HideStep();

            current = null;
            isShown = false;
        }

        void MoveNext()
        {
            if (current != null)
            {
                if (isShown)
                    overlay.HideStep();

                current.Exited?.Invoke();
            }

            isShown = false;
            advancePending = false;
            index++;

            if (index >= steps.Count)
            {
                current = null;
                Completed?.Invoke();
                return;
            }

            current = steps[index];
        }

        void Update()
        {
            if (current == null)
                return;

            if (isShown == false)
            {
                if (current.StartGate != null && current.StartGate() == false)
                    return;

                isShown = true;
                shownAt = Time.unscaledTime;
                current.Entered?.Invoke();
                overlay.Show(current);
                return;
            }

            // The tap that completed a step is handled by the game in this same frame, so the next step starts one frame later
            if (advancePending)
            {
                MoveNext();
                return;
            }

            switch (current.Advance)
            {
                case TutorialAdvance.AnyClick:
                    advancePending = Time.unscaledTime - shownAt >= AnyClickLockSeconds && Input.GetMouseButtonUp(0);
                    break;

                case TutorialAdvance.TargetClick:
                    advancePending = Input.GetMouseButtonUp(0) && IsOverAnyTarget(Input.mousePosition);
                    break;

                case TutorialAdvance.Condition:
                    advancePending = current.Condition == null || current.Condition();
                    break;
            }
        }

        void LateUpdate()
        {
            if (current != null && isShown && advancePending == false)
                overlay.Track(current);
        }

        bool IsOverAnyTarget(Vector2 screenPoint)
        {
            if (current.Targets == null)
                return false;

            foreach (var target in current.Targets())
            {
                if (TutorialOverlay.ContainsScreenPoint(target, screenPoint))
                    return true;
            }

            return false;
        }

        void OnDestroy()
        {
            if (overlay != null)
                overlay.SkipClicked -= OnSkipClicked;
        }
    }
}
