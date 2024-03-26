// Copyright (c) https://github.com/Bian-Sh
// Licensed under the MIT License.

using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;
#if UNITY_EDITOR
#endif

namespace Loom
{
  public static class Loom
  {
    private static int _mainThreadId;
    private static readonly ConcurrentQueue<Action> Tasks = new();
    public static bool IsMainThread => Thread.CurrentThread.ManagedThreadId == _mainThreadId;

    /// <summary>
    ///   切换到主线程中执行
    /// </summary>
    public static SwitchToUnityThreadAwaitable ToMainThread => new();

    /// <summary>
    ///   切换到线程池中执行
    /// </summary>
    public static SwitchToThreadPoolAwaitable ToOtherThread => new();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Install()
    {
      _mainThreadId = Thread.CurrentThread.ManagedThreadId;

      #region 使用 PlayerLoop 在 Unity 主线程的 Update 中更新本任务同步器

      // 为了 ref 而 ref
      static ref PlayerLoopSystem FindSubSystem(PlayerLoopSystem root, Predicate<PlayerLoopSystem> predicate)
      {
        for (var i = 0; i < root.subSystemList.Length; i++)
          if (predicate.Invoke(root.subSystemList[i]))
            // 可以关注 ref 配合 return 的用法，这样可以直接修改 root.subSystemList[i] 的值
            // https://learn.microsoft.com/zh-cn/dotnet/csharp/language-reference/keywords/ref#ref-returns
            return ref root.subSystemList[i];
        throw new Exception("Not Found!");
      }

      var rootLoopSystem = PlayerLoop.GetCurrentPlayerLoop();
      ref var subPls = ref FindSubSystem(rootLoopSystem, v => v.type == typeof(Update));
      Array.Resize(ref subPls.subSystemList, subPls.subSystemList.Length + 1);
      subPls.subSystemList[^1] = new PlayerLoopSystem { type = typeof(Loom), updateDelegate = Update };
      PlayerLoop.SetPlayerLoop(rootLoopSystem);

#if UNITY_EDITOR
      //因为编辑器停止播放后上面插入的 loopsystem 依旧会触发，进入或退出Play 模式先清空 tasks
      EditorApplication.playModeStateChanged -= EditorApplicationPlayModeStateChanged;
      EditorApplication.playModeStateChanged += EditorApplicationPlayModeStateChanged;
      static void EditorApplicationPlayModeStateChanged(PlayModeStateChange obj)
      {
        if (obj == PlayModeStateChange.ExitingEditMode || obj == PlayModeStateChange.ExitingPlayMode)
          while (Tasks.TryDequeue(out _))
          {
          } //清空任务列表
      }
#endif

      #endregion
    }

#if UNITY_EDITOR
    // 确保编辑器下推送的事件也能被执行
    [InitializeOnLoadMethod]
    private static void EditorForceUpdate()
    {
      Install();
      EditorApplication.update -= ForceEditorPlayerLoopUpdate;
      EditorApplication.update += ForceEditorPlayerLoopUpdate;

      static void ForceEditorPlayerLoopUpdate()
      {
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling ||
            EditorApplication.isUpdating) return; // Not in Edit mode, don't interfere
        Update();
      }
    }
#endif

    /// <summary>
    ///   在主线程中执行
    /// </summary>
    /// <param name="task">要执行的委托</param>
    public static void Post(Action task)
    {
      if (IsMainThread)
        task?.Invoke();
      else
        Tasks.Enqueue(task);
    }

    private static void Update()
    {
      while (Tasks.TryDequeue(out var task))
        try
        {
          task?.Invoke();
        }
        catch (Exception e)
        {
          Debug.Log($"{nameof(Loom)}:  封送的任务执行过程中发现异常，请确认: {e}");
        }
    }

    public struct SwitchToUnityThreadAwaitable
    {
      public Awaiter GetAwaiter()
      {
        return new Awaiter();
      }

      public struct Awaiter : INotifyCompletion
      {
        public bool IsCompleted => IsMainThread;

        public void GetResult()
        {
        }

        public void OnCompleted(Action continuation)
        {
          Post(continuation);
        }
      }
    }

    public struct SwitchToThreadPoolAwaitable
    {
      public Awaiter GetAwaiter()
      {
        return new Awaiter();
      }

      public struct Awaiter : ICriticalNotifyCompletion
      {
        private static readonly WaitCallback SwitchToCallback = state => ((Action)state).Invoke();
        public bool IsCompleted => false;

        public void GetResult()
        {
        }

        public void OnCompleted(Action continuation)
        {
          ThreadPool.UnsafeQueueUserWorkItem(SwitchToCallback, continuation);
        }

        public void UnsafeOnCompleted(Action continuation)
        {
          ThreadPool.UnsafeQueueUserWorkItem(SwitchToCallback, continuation);
        }
      }
    }
  }
}