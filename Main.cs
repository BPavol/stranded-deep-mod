using System.Reflection;
using HarmonyLib;
using UnityEngine;
using UnityModManagerNet;
using Beam;

namespace PalmFarmMod
{
    #if DEBUG
    [EnableReloading]
    #endif
    static class Main
    {
        static Harmony HarmonyInstance;

        // Send a response to the mod manager about the launch status, success or not.
        public static bool Load(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance = new Harmony(modEntry.Info.Id);
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());

            modEntry.OnGUI = OnGUI;

            #if DEBUG
            modEntry.OnUnload = Unload;
            UnityModManager.Logger.Log("Palm Farm mod loaded.");
            #endif

            return true; // If false the mod will show an error.
        }

        #if DEBUG
        static bool Unload(UnityModManager.ModEntry modEntry)
        {
            HarmonyInstance.UnpatchAll();

            return true;
        }
        #endif

        static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            GUILayout.Label("Palm Farm mod");
        }
    }

    [HarmonyPatch(typeof(Beam.InteractiveObject_FOOD), "Awake")]
    static class InteractiveObject_FOOD_Patch
    {
        static void Prefix(Beam.InteractiveObject_FOOD __instance)
        {
            if (__instance.DisplayName != "INTERACTIVE_TYPE_FOOD_COCONUT_GREEN")
            {
                return;
            }

            var plantable = __instance.gameObject.AddComponent<Beam.Crafting.Plantable>();
            Traverse.Create(plantable).Field("_plantType").SetValue(Beam.PlantType.Kura);
        }
    }

    [HarmonyPatch(typeof(Beam.Crafting.Plant), "Initialize")]
    static class Plant_Patch
    {
        static void Postfix(Beam.Crafting.Plant __instance, Beam.Crafting.IFruitFactory fruitFactory, Beam.PlantType plantType, float growthTime)
        {
            if (plantType != Beam.PlantType.Kura) {
                return;
            }

            var _plantStages = Traverse.Create(__instance).Field("_plantStages").GetValue<GameObject[]>();
            var _deadStage = Traverse.Create(__instance).Field("_deadStage").GetValue<GameObject>();

            var palm = new PalmPlantModel(PalmPlantModel.AlterPlantStages(_plantStages), PalmPlantModel.AlterDeadStage(_deadStage), (float) 1.0);
            Traverse.Create(__instance).Field("_plantModel").SetValue(palm);
        }
    }


    [HarmonyPatch(typeof(Beam.Crafting.Plot), "CreateCrop")]
    static class Plot_Patch
    {
        static void Postfix(Beam.Crafting.Plot __instance, Beam.PlantType plantType)
        {
            if (plantType != Beam.PlantType.Kura)
            {
                return;
            }

            var _plant = Traverse.Create(__instance).Field("_plant").GetValue<Beam.Crafting.Plant>();
            var _plantModel = Traverse.Create(_plant).Field("_plantModel").GetValue<Beam.Crafting.PlantModel>();
            if (!(_plantModel is PalmPlantModel palmPlantModel)) {
                return;
            }

            palmPlantModel.addOnLoppedListener(delegate () {
                Traverse.Create(__instance).Field("_water").SetValue(0);
            });
        }
    }

    [HarmonyPatch(typeof(Beam.Crafting.PlantFruit), "CreateFruit")]
    static class PlantFruit_Patch
    {
        static bool Prefix(Beam.Crafting.PlantFruit __instance)
        {
            var _plantType = Traverse.Create(__instance).Field("_plantType").GetValue<Beam.PlantType>();
            if (_plantType != Beam.PlantType.Kura)
            {
                return true;
            }

            // Do not create any fruit
            return false;
        }
    }

    [HarmonyPatch(typeof(Beam.InteractiveObject_PALM), "Save")]
    static class InteractiveObject_PALM_Patch
    {
        static bool Prefix(Beam.InteractiveObject_PALM __instance, ref Beam.Serialization.Json.JObject __result)
        {
            if (__instance.gameObject.GetComponent<PreventSave>() == null) {
                return true;
            }

            __result = (Beam.Serialization.Json.JObject) null;
            return false;
        }
    }
}
