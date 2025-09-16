using UnityEngine;

namespace com.fscigliano.SequentialEvents
{
    public class SequenceComponent : MonoBehaviour
    {
        public bool autoPlay = false;
        public Sequence sequence;

        void Start()
        {
            if (autoPlay)
                sequence.Run();
        }

        public void Run()
        {
            sequence.Run();
        }
        private void OnDestroy()
        {
            sequence.OnDestroy();
        }
    }
}