using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace com.fscigliano.SequentialEvents
{
    [Serializable]
    public class Sequence
    {
        [Serializable] public class UnityEventAsync : UnityEvent<Sequence> {}

        [SerializeField] private List<SequenceItem> _items = new List<SequenceItem>();
        [SerializeField] private UnityEvent _onFinishEvt;

        private CancellationTokenSource _cancelToken;
        private bool _isPlaying;
        public bool IsPlaying => _isPlaying;

        public async Task RunAsync()
        {
            if (_isPlaying)
            {
                Debug.LogWarning($"Sequence {this} is already playing.");
                return;
            }

            _isPlaying = true;
            _cancelToken = new CancellationTokenSource();

            try
            {
                foreach (var item in _items) item.ResetState();

                foreach (var item in _items)
                {
                    if (!item.enabled) continue;

                    item.isCurrent = true;

                    if (item.waitBefore > 0)
                    {
                        item.isWaitingBefore = true;
                        await Wait(item.waitBefore, item.realtime, v => item.waitBeforeProgress = v, _cancelToken.Token);
                        item.isWaitingBefore = false;
                    }

                    if (item.@async)
                    {
                        item.isExecuting = true;
                        item.waitingCompletion = true;
                        item.actionAsync.Invoke(this);

                        while (item.waitingCompletion && !_cancelToken.IsCancellationRequested)
                            await Task.Yield();

                        item.isExecuting = false;
                        if (_cancelToken.IsCancellationRequested) return;
                    }
                    else
                    {
                        item.action.Invoke();
                    }

                    if (item.waitAfter > 0)
                    {
                        item.isWaitingAfter = true;
                        await Wait(item.waitAfter, item.realtime, v => item.waitAfterProgress = v, _cancelToken.Token);
                        item.isWaitingAfter = false;
                    }

                    item.isCurrent = false;
                }

                _onFinishEvt?.Invoke();
            }
            catch (OperationCanceledException) {}
            finally { _isPlaying = false; }
        }

        public void Run() => _ = RunAsync();

        public void ItemComplete(SequenceItem item) => item.waitingCompletion = false;

        public void OnDestroy() => _cancelToken?.Cancel();

        private static async Task Wait(float duration, bool realtime, Action<float> progressUpdate, CancellationToken token)
        {
            float elapsed = 0f;
            float deltaTime() => realtime ? Time.unscaledDeltaTime : Time.deltaTime;

            while (elapsed < duration && !token.IsCancellationRequested)
            {
                await Task.Yield();
                elapsed += deltaTime();
                progressUpdate?.Invoke(Mathf.Clamp01(elapsed / duration));
            }

            progressUpdate?.Invoke(1f);
        }
    }
}
