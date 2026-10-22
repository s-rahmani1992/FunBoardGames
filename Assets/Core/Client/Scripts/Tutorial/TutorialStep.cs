using System;
using System.Collections.Generic;
using UnityEngine;

namespace FunBoardGames.Tutorial
{
    public enum TutorialAdvance
    {
        AnyClick,
        TargetClick,
        Condition,
    }

    public enum TutorialMessagePosition
    {
        Auto,
        Top,
        Center,
        Bottom,
    }

    public class TutorialStep
    {
        public string Message { get; private set; }
        public TutorialAdvance Advance { get; private set; } = TutorialAdvance.AnyClick;
        public TutorialMessagePosition MessagePosition { get; private set; } = TutorialMessagePosition.Auto;
        public bool Dim { get; private set; } = true;
        public bool ShowHand { get; private set; }

        public Func<IEnumerable<RectTransform>> Targets { get; private set; }
        public Func<RectTransform> HandTarget { get; private set; }
        public Func<bool> StartGate { get; private set; }
        public Func<bool> Condition { get; private set; }
        public Action Entered { get; private set; }
        public Action Exited { get; private set; }

        TutorialStep(string message)
        {
            Message = message;
        }

        public static TutorialStep Say(string message) => new(message);

        public TutorialStep Highlight(Func<RectTransform> target)
        {
            Targets = () => new[] { target() };
            return this;
        }

        public TutorialStep HighlightAll(Func<IEnumerable<RectTransform>> targets)
        {
            Targets = targets;
            return this;
        }

        public TutorialStep ClickTarget()
        {
            Advance = TutorialAdvance.TargetClick;
            ShowHand = true;
            return this;
        }

        public TutorialStep Until(Func<bool> condition)
        {
            Advance = TutorialAdvance.Condition;
            Condition = condition;
            return this;
        }

        public TutorialStep PointAt(Func<RectTransform> target)
        {
            ShowHand = true;
            HandTarget = target;
            return this;
        }

        public TutorialStep WaitFor(Func<bool> gate)
        {
            StartGate = gate;
            return this;
        }

        public TutorialStep NoDim()
        {
            Dim = false;
            return this;
        }

        public TutorialStep At(TutorialMessagePosition position)
        {
            MessagePosition = position;
            return this;
        }

        public TutorialStep OnEnter(Action action)
        {
            Entered = action;
            return this;
        }

        public TutorialStep OnExit(Action action)
        {
            Exited = action;
            return this;
        }
    }
}
