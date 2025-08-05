using System.Threading;
using Cysharp.Threading.Tasks;

namespace Cardevil.Managers
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using UnityEngine;
    using UnityEngine.Pool;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// 액션을 순차적으로 실행하는 매니저
    /// actionQueue -> finalActionQueue -> (Exec 종료) -> postExecuteQueue 순으로 실행됨.
    /// TODO: 씬 로드/언로드 상황 핸들링
    /// </summary>
    public class ExecutionManager
    {
        private LinkedList<ActionWrapper> _actionQueue = new LinkedList<ActionWrapper>();
        private LinkedList<ActionWrapper> _finalActionQueue = new LinkedList<ActionWrapper>();
        private LinkedList<ActionWrapper> _postExecuteQueue = new LinkedList<ActionWrapper>();
        
        
        private bool _isExecuting = false;
        private int _doNotFinishYetCount = 0;
        private int _pausedCount = 0;
        private float _totalDelay = 0f;

        private CancellationTokenSource _cts;
        
        public bool ForcePause = false;

        /// <summary>
        /// 내부 액션이 끝나는걸 기다려야하는 경우. 이걸 False로 두고 실행하면됨
        /// </summary>
        /// <code>
        /// () => {
        /// ExecutionManager.IsReadyToNextAction = false;
        /// ... 많은 행동
        /// ExecutionManager.IsReadyToNextAction = true;
        /// }
        /// </code>
        public static bool IsReadyToNextAction = true;

        /// <summary>
        /// 큐가 비어있더라도 액션을 끝내지 않고 대기하도록함.
        /// 이걸 += 1 하면 대기하고, -= 1 하면 대기 해제됨
        /// 뮤텍스 비슷하게 작동.
        /// </summary>
        /// <remarks>
        /// 코루틴 내에서 EnqueueAction을 호출하면 이것을 조정해야할 수 있음.
        /// 만약 이걸 조정하지 않으면, finalAction이 먼저 실행될 수 있다.
        /// </remarks>
        /// <code>
        /// ExecutionManager.DoNotFinishYetCount += 1; // lock
        /// ...
        /// yield return new WaitForSeconds(1f); // 무엇이 됐든 딜레이가 있어야 의미있음
        /// ...
        /// ExecutionManager.DoNotFinishYetCount -= 1; // unlock
        /// </code>
        public int DoNotFinishYetCount
        {
            get => _doNotFinishYetCount;
            set
            {
                if (value < 0)
                {
                    _doNotFinishYetCount = 0;
                    Debug.LogError("DoNotFinishYetCount에 음수값이 들어옴.");
                }
                else
                {
                    _doNotFinishYetCount = value;
                }
            }
        }

        /// <summary>
        /// 일시정지 상태를 나타내는 변수.
        /// 다음 액션을 실행하기 전에 이 값이 0이 되어야 함.
        /// </summary>
        public int PausedCount
        {
            get => _pausedCount;
            set
            {
                if (value < 0)
                {
                    _pausedCount = 0;
                    Debug.LogError("PausedCount에 음수값이 들어옴.");
                }
                else
                {
                    _pausedCount = value;
                }
            }
        }

        public bool IsExecuting => _isExecuting;


        public float TotalDelay => Mathf.Max(0f, _totalDelay);

        private IObjectPool<ActionWrapper> _actionWrapperPool;

        private void Init()
        {
            _actionWrapperPool = new ObjectPool<ActionWrapper>(() => new ActionWrapper(null),
                (wrapper => wrapper.Clear()), null, null, false, 10, 50);
            
            _cts = new CancellationTokenSource();
        }
        // private void OnSceneUnloaded(Scene scene, LoadSceneMode mode)
        // {
        //     if (scene.name == "MainMenu")
        //     {
        //         _actionQueue.Clear();
        //         _actionWrapperPool.Clear();
        //         Destroy(gameObject);
        //     }
        // }


        /// <summary>
        /// 액션을 감싸는 래퍼 클래스
        /// </summary>
        private class ActionWrapper
        {
            public Action Action;
            public float BeforeDelay;
            public float AfterDelay;

            /// <summary>
            /// 액션 래퍼 생성자
            /// </summary>
            /// <param name="action"></param>
            /// <param name="beforeDelay">실행전 딜레이</param>
            /// <param name="afterDelay">실행 후 딜레이</param>
            public ActionWrapper(Action action, float beforeDelay = 0f, float afterDelay = 0f)
            {
                Action = action;
                BeforeDelay = beforeDelay;
                AfterDelay = afterDelay;
            }

            public void Clear()
            {
                Action = null;
                BeforeDelay = 0f;
                AfterDelay = 0f;
            }
        }

        /// <summary>
        /// 호출 큐의 마지막에 넣는다.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="beforeDelay">해당 액션이 호출되기 전의 딜레이</param>
        /// <param name="afterDelay">해당 액션이 호출된 후의 딜레이</param>
        public void EnqueueAction([NotNull] Action action, float beforeDelay = 0f, float afterDelay = 0f)
        {
            ActionWrapper wrapper = _actionWrapperPool.Get();
            wrapper.Action = action;
            wrapper.BeforeDelay = beforeDelay;
            wrapper.AfterDelay = afterDelay;
            _totalDelay += beforeDelay + afterDelay;
            _actionQueue.AddLast(wrapper);
        }

        /// <summary>
        /// 호출 큐의 마지막에 넣고, 바로 실행한다.
        /// 이미 실행중이라면 무시한다.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="beforeDelay"></param>
        [Obsolete("이걸 실행하면 이후 EnqueueAction이 무시될 수 있음. EnqueueAction 사용후 TurnManager를 기다리기.")]
        public void EnqueueActionAndExecute([NotNull] Action action, float beforeDelay = 0f, float afterDelay = 0f)
        {
            Debug.Log(
                $"EnqueueActionAndExecute: {action.Method.Name} with beforeDelay: {beforeDelay}, afterDelay: {afterDelay}");
            EnqueueAction(action, beforeDelay, afterDelay);
            StartExecuteActionsRoutine();
        }

        /// <summary>
        /// 호출 큐의 처음에 넣는다.(새치기)
        /// </summary>
        /// <remarks>
        /// final 혹은 post 도중에 호출하면 의도대로 작동하지 않음
        /// </remarks>
        /// <param name="action"></param>
        /// <param name="beforeDelay">해당 액션이 호출되기 전의 딜레이</param>
        /// <param name="afterDelay">해당 액션이 호출된 후의 딜레이</param>
        public void EnqueueFirstAction(Action action, float beforeDelay = 0f, float afterDelay = 0f)
        {
            ActionWrapper wrapper = _actionWrapperPool.Get();
            wrapper.Action = action;
            wrapper.BeforeDelay = beforeDelay;
            wrapper.AfterDelay = afterDelay;
            _totalDelay += beforeDelay + afterDelay;

            _actionQueue.AddFirst(wrapper);
        }

        /// <summary>
        /// 호출 큐의 처음에 넣고, 바로 실행한다.(새치기)
        /// </summary>
        /// <remarks>
        /// final 혹은 post 도중에 호출하면 의도대로 작동하지 않음
        /// </remarks>
        /// <param name="action"></param>
        /// <param name="beforeDelay"></param>
        public void EnqueueFirstActionAndExecute(Action action, float beforeDelay = 0f, float afterDelay = 0f)
        {
            EnqueueFirstAction(action, beforeDelay, afterDelay);
            StartExecuteActionsRoutine();
        }

        /// <summary>
        /// 모든 기본 액션이 끝난 후 실행되는 이벤트 큐
        /// isExecuting은 True임.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="beforeDelay"></param>
        /// <param name="afterDelay"></param>
        public void EnqueueFinalAction(Action action, float beforeDelay = 0f, float afterDelay = 0f)
        {
            ActionWrapper wrapper = _actionWrapperPool.Get();
            wrapper.Action = action;
            wrapper.BeforeDelay = beforeDelay;
            wrapper.AfterDelay = afterDelay;

            _totalDelay += beforeDelay + afterDelay;
            _finalActionQueue.AddLast(wrapper);
        }

        /// <summary>
        /// 모든 실행이 끝난후 실행되는 이벤트 큐
        /// isExecuting가 false가 된 후에 실행됨.
        /// 다른 Execution이 동시에 실행될 수 있음
        /// </summary>
        /// <param name="action"></param>
        /// <param name="beforeDelay"></param>
        /// <param name="afterDelay"></param>
        public void EnqueuePostExecuteAction(Action action, float beforeDelay = 0f, float afterDelay = 0f)
        {
            ActionWrapper wrapper = _actionWrapperPool.Get();
            wrapper.Action = action;
            wrapper.BeforeDelay = beforeDelay;
            wrapper.AfterDelay = afterDelay;

            _totalDelay += beforeDelay + afterDelay;
            _postExecuteQueue.AddLast(wrapper);
        }

        public bool StartExecuteActionsRoutine()
        {
            if (_isExecuting)
            {
                Debug.Log("ExecutionManager: Already executing actions.");
                return false;
            }

            if (_actionQueue.Count == 0 && _finalActionQueue.Count == 0 && _postExecuteQueue.Count == 0)
            {
                Debug.Log("ExecutionManager: No actions to execute.");
                return false;
            }

            ExecuteActionsRoutine(_cts.Token).Forget(Debug.LogException);
            return true;
        }

        public void ClearAll()
        {
            _actionQueue.Clear();
            _finalActionQueue.Clear();
            _totalDelay = 0f;
            _isExecuting = false;
            _actionWrapperPool.Clear();
            ForcePause = false;
            DoNotFinishYetCount = 0;
            IsReadyToNextAction = true;
            
            _cts.Cancel();
        }

        private async UniTask ExecuteActionsRoutine(CancellationToken token)
        {
            Debug.Log("ExecutionManager: Starting action execution routine.");
            _isExecuting = true;
            while (true)
            {
                while (_actionQueue.Count > 0)
                {

                    await UniTask.WaitWhile(() => ForcePause || PausedCount > 0 || !IsReadyToNextAction, cancellationToken: token);

                    ActionWrapper wrapper = _actionQueue.First.Value;
                    // StopAllCoroutines();
                    // 사전 딜레이 체크
                    if (wrapper.BeforeDelay > 0f)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(wrapper.BeforeDelay));
                        _totalDelay -= wrapper.BeforeDelay;
                        Debug.Log($"ExecutionManager: Waiting for beforeDelay: {wrapper.BeforeDelay}");
                    }

                    await UniTask.WaitWhile(() => ForcePause || PausedCount > 0 || !IsReadyToNextAction, cancellationToken: token);


                    // 액션 실행
                    Debug.Log(
                        $"ExecutionManager: Executing action {wrapper.Action.Method.Name} with beforeDelay: {wrapper.BeforeDelay}, afterDelay: {wrapper.AfterDelay}");
                    _actionQueue.RemoveFirst();
                    wrapper.Action?.Invoke();
                    _actionWrapperPool.Release(wrapper);

                    // 사후 딜레이 체크
                    if (wrapper.AfterDelay > 0f)
                    {
                        await UniTask.Delay(TimeSpan.FromSeconds(wrapper.AfterDelay), cancellationToken: token);
                        _totalDelay -= wrapper.AfterDelay;
                    }
                }

                if (DoNotFinishYetCount == 0)
                {
                    break;
                }
            }

            // 최종 액션 실행
            Debug.Log("ExecutionManager: Executing final actions.");
            while (_finalActionQueue.Count > 0)
            {
                await UniTask.WaitWhile(() => ForcePause || PausedCount > 0 || !IsReadyToNextAction, cancellationToken: token);

                ActionWrapper wrapper = _finalActionQueue.First.Value;

                // 사전 딜레이 체크
                if (wrapper.BeforeDelay > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(wrapper.BeforeDelay), cancellationToken: token);
                    _totalDelay -= wrapper.BeforeDelay;
                }

                await UniTask.WaitWhile(() => ForcePause || PausedCount > 0 || !IsReadyToNextAction, cancellationToken: token);

                // 액션 실행
                _finalActionQueue.RemoveFirst();
                wrapper.Action?.Invoke();
                _actionWrapperPool.Release(wrapper);

                // 사후 딜레이 체크
                if (wrapper.AfterDelay > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(wrapper.AfterDelay));
                    _totalDelay -= wrapper.AfterDelay;
                }
            }

            Debug.Log("ExecutionManager: Finished executing all actions.");
            _isExecuting = false;

            // 모든 액션이 끝나면 후처리 큐 실행
            while (_postExecuteQueue.Count > 0)
            {
                await UniTask.WaitWhile(() => ForcePause, cancellationToken: token);

                ActionWrapper wrapper = _postExecuteQueue.First.Value;

                // 사전 딜레이 체크
                if (wrapper.BeforeDelay > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(wrapper.BeforeDelay), cancellationToken: token);
                    _totalDelay -= wrapper.BeforeDelay;
                }

                await UniTask.WaitWhile(() => ForcePause, cancellationToken: token);

                // 액션 실행
                _postExecuteQueue.RemoveFirst();
                wrapper.Action?.Invoke();
                _actionWrapperPool.Release(wrapper);

                // 사후 딜레이 체크
                if (wrapper.AfterDelay > 0f)
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(wrapper.AfterDelay), cancellationToken: token);
                    _totalDelay -= wrapper.AfterDelay;
                }
            }
        }
    }
}
