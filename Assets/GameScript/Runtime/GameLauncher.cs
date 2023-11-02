using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MotionFramework;
using MotionFramework.Console;
using MotionFramework.Event;
using MotionFramework.Tween;
using MotionFramework.Resource;
using MotionFramework.Pool;
using MotionFramework.Audio;
using MotionFramework.Config;
using MotionFramework.Scene;
using MotionFramework.Window;
using YooAsset;

public class GameLauncher : MonoBehaviour
{
    public EPlayMode PlayMode = EPlayMode.EditorSimulateMode;

    private void Awake()
    {
#if !UNITY_EDITOR
        PlayMode = EPlayMode.OfflinePlayMode;
#endif

        // 初始化应用
        InitAppliaction();

        // 初始化控制台
        if (Application.isEditor || Debug.isDebugBuild)
        {
            DeveloperConsole.Initialize();
        }

        // 初始化框架
        MotionEngine.Initialize(this, HandleMontionFrameworkLog);
    }

    private void Start()
    {
        StartCoroutine(CreateGameModules());
    }

    private void Update()
    {
        // 更新框架
        MotionEngine.Update();
    }

    private void OnGUI()
    {
        // 绘制控制台
        if (Application.isEditor || Debug.isDebugBuild)
        {
            DeveloperConsole.Draw();
        }
    }

    // 初始化应用
    private void InitAppliaction()
    {
        Application.runInBackground = true;
        Application.backgroundLoadingPriority = ThreadPriority.High;

        // 设置最大帧数
        Application.targetFrameRate = 30;

        // 屏幕不休眠
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    // 框架监听日志
    private void HandleMontionFrameworkLog(ELogLevel logLevel, string log)
    {
        if (logLevel == ELogLevel.Log)
        {
            UnityEngine.Debug.Log(log);
        }
        else if (logLevel == ELogLevel.Error)
        {
            UnityEngine.Debug.LogError(log);
        }
        else if (logLevel == ELogLevel.Warning)
        {
            UnityEngine.Debug.LogWarning(log);
        }
        else if (logLevel == ELogLevel.Exception)
        {
            UnityEngine.Debug.LogError(log);
        }
        else
        {
            throw new NotImplementedException($"{logLevel}");
        }
    }

    private IEnumerator CreateGameModules()
    {
        // 创建事件管理器
        MotionEngine.CreateModule<EventManager>();

        // 创建补间管理器
        MotionEngine.CreateModule<TweenManager>();

        // 创建资源管理器
        string locationRoot = "Assets/GameRes/";
        if (PlayMode == EPlayMode.EditorSimulateMode)
        {
            var resourceCreateParam = new EditorSimulateModeParameters();
            resourceCreateParam.SimulateManifestFilePath = EditorSimulateModeHelper.SimulateBuild("DefaultPackage");
            MotionEngine.CreateModule<ResourceManager>(resourceCreateParam);
            var operation = ResourceManager.Instance.InitializeAsync(locationRoot);
            yield return operation;
        }
        else if (PlayMode == EPlayMode.OfflinePlayMode)
        {
            var resourceCreateParam = new OfflinePlayModeParameters();
            MotionEngine.CreateModule<ResourceManager>(resourceCreateParam);
            var operation = ResourceManager.Instance.InitializeAsync(locationRoot);
            yield return operation;
        }
        else
        {
            throw new System.NotImplementedException();
        }

        // 创建对象池管理器
        var poolCreateParam = new GameObjectPoolManager.CreateParameters();
        poolCreateParam.DefaultDestroyTime = 5f;
        MotionEngine.CreateModule<GameObjectPoolManager>(poolCreateParam);

        // 创建音频管理器
        MotionEngine.CreateModule<AudioManager>();

        // 创建配置表管理器
        MotionEngine.CreateModule<ConfigManager>();

        // 创建场景管理器
        MotionEngine.CreateModule<SceneManager>();

        // 创建窗口管理器
        MotionEngine.CreateModule<WindowManager>();

        // 启动游戏
    }
}
