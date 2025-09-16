using System;
using UnityEngine;
using UnityEngine.Events;

namespace com.fscigliano.SequentialEvents
{
    
    
    [Serializable]
    public class SequenceItem
    {
        public bool enabled = true;
        [TextArea(3,6)] public string description;
        public Color color;
        public float waitBefore;
        public float waitAfter;
        public bool realtime;
        public UnityEvent action;
        public bool @async;
        public Sequence.UnityEventAsync actionAsync;

        [NonSerialized] public bool isCurrent;
        [NonSerialized] public float waitBeforeProgress;
        [NonSerialized] public float waitAfterProgress;
        [NonSerialized] public bool isWaitingAfter;
        [NonSerialized] public bool isWaitingBefore;
        [NonSerialized] public bool isExecuting;
        [NonSerialized] public bool waitingCompletion;

        public void ResetState()
        {
            isCurrent = false;
            waitBeforeProgress = 0;
            waitAfterProgress = 0;
            isWaitingAfter = false;
            isWaitingBefore = false;
            isExecuting = false;
            waitingCompletion = false;
        }
    }
}