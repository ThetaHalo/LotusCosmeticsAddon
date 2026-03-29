using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using Il2CppSystem.Linq;
using UnityEngine.AddressableAssets;
using VentLib.Logging;
using Lotus.Managers;
using UnityEngine;
using VentLib.Utilities;
using VentLib.Utilities.Extensions;

namespace LotusCosmetics.CustomHats;

public static class HatLoader
{
    private static readonly StandardLogger log = LoggerFactory.GetLogger<StandardLogger>(typeof(HatLoader));

    private static bool LoadedHats;

    internal static void LoadHatsRoutine()
    {
        if (LoadedHats || !DestroyableSingleton<HatManager>.InstanceExists || DestroyableSingleton<HatManager>.Instance.allHats.Count == 0)
            return;
        LoadedHats = true;
        Async.Schedule(LoadHats(), 0.01f);
    }

    internal static IEnumerator LoadHats()
    {
        try
        {
            var hatBehaviours = DiscoverHatBehaviours().Distinct().ToList();

            var hatData = new List<HatData>();
            hatData.AddRange(DestroyableSingleton<HatManager>.Instance.allHats);
            hatData.ForEach((Action<HatData>)(x => x.StoreName = "Vanilla"));

            var originalCount = DestroyableSingleton<HatManager>.Instance.allHats.ToList().Count;
            hatBehaviours.Reverse();
            for (var i = 0; i < hatBehaviours.Count; i++)
            {
                hatBehaviours[i].displayOrder = originalCount + i;
                hatData.Add(hatBehaviours[i]);
            }
            DestroyableSingleton<HatManager>.Instance.allHats = hatData.ToArray();
        }
        catch (Exception e)
        {
            log.Exception($"Error while loading hats: {e.Message}\nStack: {e.StackTrace}");
        }
        yield return null;
    }

    private static List<HatData> DiscoverHatBehaviours()
    {
        var hatBehaviours = new List<HatData>();
        var path = LotusCosmeticsAdon.RuntimeLocation + (OperatingSystem.IsAndroid() ? "/lotushats.catalog" : "\\lotushats.catalog");
        Addressables.AddResourceLocator(Addressables.LoadContentCatalog(path).WaitForCompletion());
        var allHatLocations = Addressables.LoadResourceLocationsAsync("lotushats").WaitForCompletion();
        var assets = Addressables.LoadAssetsAsync<HatData>(allHatLocations, null, false).WaitForCompletion();
        var array = new Il2CppSystem.Collections.Generic.List<HatData>(assets.Pointer);
        hatBehaviours.AddRange(array.ToArray());
        return hatBehaviours;
    }
}