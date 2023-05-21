using System;
using HarmonyLib;
using Beam;
using UnityEngine;

namespace PalmFarmMod
{
    class PalmPlantModel : Beam.Crafting.PlantModel
    {
        private GameObject palm;
        private Beam.InteractiveObject_PALM interactiveObject;

        public PalmPlantModel(GameObject[] plantStages, GameObject deadStage, float growthTime) : base(plantStages, deadStage, growthTime)
        {
            this.palm = plantStages[4];

            this.interactiveObject = this.palm.GetComponent<Beam.InteractiveObject_PALM>();
            this.interactiveObject.Lopped = new BaseActionUnityEvent();
        }

        public void addOnLoppedListener(Action action)
        {
            this.interactiveObject.Lopped.AddListener(new UnityEngine.Events.UnityAction<IBase, IBaseActionEventData>(delegate (IBase sender, IBaseActionEventData data) {
                action();
            }));
        }

        public static GameObject[] AlterPlantStages(GameObject[] originalPlantStages)
        {
            GameObject[] plantStages = {
                InheritObjectTransformation(originalPlantStages[0], UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/StrandedObjects/Food/COCONUT_ORANGE"))),
                InheritObjectTransformation(originalPlantStages[1], UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/StrandedObjects/Trees/YOUNG_PALM_2"))),
                InheritObjectTransformation(originalPlantStages[2], UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/StrandedObjects/Trees/YOUNG_PALM_1"))),
                InheritObjectTransformation(originalPlantStages[3], UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/StrandedObjects/Trees/YOUNG_PALM_1"))),
                InheritObjectTransformation(originalPlantStages[4], UnityEngine.Object.Instantiate<GameObject>(Resources.Load<GameObject>("Prefabs/StrandedObjects/Trees/PALM_4")))
            };

            // Remove rigidbody until palm is ready for harvest
            for (int i = 0; i <= 3; i++)
            {
                UnityEngine.Object.Destroy(plantStages[i].GetComponent<Rigidbody>());
                UnityEngine.Object.Destroy(plantStages[i].GetComponent<InteractiveObject>());
                UnityEngine.Object.Destroy(plantStages[i].GetComponent<Saveable>());
            }

            // Saving is handled by PlantModel
            plantStages[4].AddComponent(typeof(PreventSave));

            // Destroy replaced object
            foreach (GameObject plantStage in originalPlantStages)
            {
                UnityEngine.Object.Destroy(plantStage);
            }

            return plantStages;
        }


        public static GameObject AlterDeadStage(GameObject original)
        {
            InteractiveObject_PALM palm = Resources.Load<InteractiveObject_PALM>("Prefabs/StrandedObjects/Trees/PALM_4");
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(Traverse.Create(palm).Field("_stump").GetValue<GameObject>());    
            gameObject.transform.rotation = original.transform.rotation;
            gameObject.transform.localPosition = original.transform.localPosition;
            gameObject.transform.position = original.transform.position;
            gameObject.transform.parent = original.transform.parent;
            UnityEngine.Object.Destroy(original);

            return gameObject;
        }

        private static GameObject InheritObjectTransformation(GameObject original, GameObject newObject)
        {
            newObject.transform.rotation = original.transform.rotation;
            newObject.transform.localPosition = original.transform.localPosition;
            newObject.transform.position = original.transform.position;
            newObject.transform.parent = original.transform.parent;

            return newObject;
        }
    }
}
