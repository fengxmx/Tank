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

        // ��ʼ��Ӧ��
        InitAppliaction();

        // ��ʼ������̨
        if (Application.isEditor || Debug.isDebugBuild)
        {
            DeveloperConsole.Initialize();
        }

        // ��ʼ�����
        MotionEngine.Initialize(this, HandleMontionFrameworkLog);
    }

    private void Start()
    {
        StartCoroutine(CreateGameModules());
    }

    private void Update()
    {
        // ���¿��
        MotionEngine.Update();
    }

    private void OnGUI()
    {
        // ���ƿ���̨
        if (Application.isEditor || Debug.isDebugBuild)
        {
            DeveloperConsole.Draw();
        }
    }

    // ��ʼ��Ӧ��
    private void InitAppliaction()
    {
        Application.runInBackground = true;
        Application.backgroundLoadingPriority = ThreadPriority.High;

        // �������֡��
        Application.targetFrameRate = 30;

        // ��Ļ������
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }

    // ��ܼ�����־
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
        // �����¼�������
        MotionEngine.CreateModule<EventManager>();

        // �������������
        MotionEngine.CreateModule<TweenManager>();

        // ������Դ������
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

        // ��������ع�����
        var poolCreateParam = new GameObjectPoolManager.CreateParameters();
        poolCreateParam.DefaultDestroyTime = 5f;
        MotionEngine.CreateModule<GameObjectPoolManager>(poolCreateParam);

        // ������Ƶ������
        MotionEngine.CreateModule<AudioManager>();

        // �������ñ������
        MotionEngine.CreateModule<ConfigManager>();

        // ��������������
        MotionEngine.CreateModule<SceneManager>();

        // �������ڹ�����
        MotionEngine.CreateModule<WindowManager>();

        // ������Ϸ
    }
}
