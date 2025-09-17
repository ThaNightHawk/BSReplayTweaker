using IPA;
using IPA.Loader;
using IpaLogger = IPA.Logging.Logger;
using HarmonyLib;
using SiraUtil.Zenject;
using Zenject;

namespace ReplayTweaker;

[Plugin(RuntimeOptions.DynamicInit)]
internal class Init
{
    private readonly Harmony _harmony;
    private static IpaLogger Log { get; set; } = null!;

    [Init]
    public Init(IpaLogger ipaLogger, PluginMetadata pluginMetadata, Zenjector zenjector)
    {
        Init.Log.Info($"{pluginMetadata.Name} {pluginMetadata.HVersion} initialized.");
        Init.Log.Warn("This is meant for content creation. Do NOT use this to fake replays as being your own.");
        _harmony = new Harmony(pluginMetadata.Id);
        zenjector.Install<PluginInstaller>(Location.StandardPlayer);
    }

    [OnStart]
    public void OnApplicationStart()
    {
        _harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
    }

    [OnExit]
    public void OnApplicationQuit()
    {
        _harmony.UnpatchSelf();
    }

    public class PluginInstaller : Installer<PluginInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<ReplayObjectRemover>().AsSingle().NonLazy();
        }
    }

    private class ReplayObjectRemover : IInitializable
    {
        public void Initialize()
        {
            var hiderObject = new UnityEngine.GameObject("ReplayObjectDisabler");
            hiderObject.AddComponent<ReplayObjectDisabler>();
            UnityEngine.Object.DontDestroyOnLoad(hiderObject);
        }
    }
    
    private class ReplayObjectDisabler : UnityEngine.MonoBehaviour
    {
        private void Start()
        {
            var objectsToRemove = new[]
            {
                "ReplayWatermark",
                "CustomNotes Watermark",
                "CustomUIText-ScoreSaber",
                "InGameReplayUI"
            };

            foreach (var objName in objectsToRemove)
            {
                var obj = UnityEngine.GameObject.Find(objName);
                if (obj == null)
                {
                    Init.Log.Warn($"Couldn't find {objName}");
                    continue;
                }
                
                Init.Log.Info($"Disabling {objName}");
                obj.SetActive(false);
            }
        }
    }
}