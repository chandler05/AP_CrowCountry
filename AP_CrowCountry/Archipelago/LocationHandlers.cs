using System.Diagnostics;
using HarmonyLib;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;

namespace APCrowCountry.Archipelago;

public class Location {
    public long apid;
    public bool pickup;
    public string region;
    public string id;

    public Location(long apid, bool pickup, string region, string id) {
        this.apid = apid;
        this.pickup = pickup;
        this.region = region;
        this.id = id;
    }
}
public class LocationHandlers
{
    [HarmonyPatch(typeof(Fsm), "SwitchState")]
    public static class Fsm_SwitchState
    {
        public static void Prefix(Fsm __instance, ref FsmState toState)
        {
            if (__instance.GameObjectName == "FLOW CODE" && __instance.GameObject.transform.parent.parent.parent.parent.name != "Pickup Trap" && __instance.GameObject.transform.parent.parent.parent.name.IndexOf("vending machine") == -1)
            {
                //UnityEngine.Debug.Log(toState.Name);

                // Pickups
                if (toState.Name.IndexOf("add item") != -1) {
                    // Wooden Crate
                    if (__instance.GameObject.transform.parent.parent.parent.parent.parent.parent.parent.name.IndexOf("Wooden Crate") != -1) {
                        //UnityEngine.Debug.Log("Adding item");
                        SendLocationFromCrate(__instance, toState);
                        toState = toState.Transitions[0].ToFsmState;
                        //UnityEngine.Debug.Log(toState.Name);
                    } else {
                        //UnityEngine.Debug.Log("Adding item");
                        SendLocationFromPickup(__instance, toState);
                        toState = toState.Transitions[0].ToFsmState;
                        //UnityEngine.Debug.Log(toState.Name);
                    }
                }
            }
            if (__instance.GameObjectName == "flowcode" && __instance.GameObject.transform.parent.parent.parent.name.IndexOf("Trash can") != -1)
            {
                UnityEngine.Debug.Log(toState.Name);

                // Trashcan
                if (toState.Name.IndexOf("take") != -1) {
                    //UnityEngine.Debug.Log("Adding item");
                    SendLocationFromObject(__instance, toState);
                    //UnityEngine.Debug.Log(toState.Name);
                }
                if (toState.Name.IndexOf("add") != -1) {
                    //UnityEngine.Debug.Log("Adding item");
                    toState = toState.Transitions[0].ToFsmState;
                    //UnityEngine.Debug.Log(toState.Name);
                }
            }
            if (__instance.GameObjectName == "FLOW CODE" && __instance.GameObject.transform.parent.parent.parent.name.IndexOf("vending machine") != -1)
            {
                //UnityEngine.Debug.Log(toState.Name);

                // Vending Machine
                if (toState.Name.IndexOf("given heals already?") != -1) {
                    //UnityEngine.Debug.Log("Adding item");
                    toState = toState.Transitions[0].ToFsmState;
                    //UnityEngine.Debug.Log(toState.Name);
                }
                if (toState.Name.IndexOf("add") != -1) {
                    //UnityEngine.Debug.Log("Adding item");
                    SendLocationFromObject(__instance, toState);
                    toState = toState.Transitions[0].ToFsmState;
                    //UnityEngine.Debug.Log(toState.Name);
                }
            }
            if (__instance.GameObjectName == "Select a Pickup") {
                //UnityEngine.Debug.Log(toState.Name);

                // Wooden Box
                if (toState.Name.IndexOf("random choice") != -1) {
                    toState = toState.Transitions[2].ToFsmState;
                }
            }
        }
    }

    // Old Pocket Light and Laser Sight code
    /*[HarmonyPatch(typeof(SetBoolValue), "OnEnter")]
    public static class SetBoolValue_OnEnter
    {
        public static void Prefix(SetBoolValue __instance, out bool __state)
        {
            __state = false;
            if (__instance.Owner.name == "FLOW CODE")
            {
                if ((__instance.boolVariable.Name == "light found" || __instance.boolVariable.Name == "Hangun Laser") && __instance.Owner.scene.name == "Toilet") {
                    UnityEngine.Debug.Log("Light found");
                    SendSpecialLocation("Entrance", "PocketLight");
                    __state = true;
                }
                if ((__instance.boolVariable.Name == "light found" || __instance.boolVariable.Name == "Hangun Laser") && __instance.Owner.scene.name == "Station Square") {
                    UnityEngine.Debug.Log("Laser sight found");
                    //SendSpecialLocation("Entrance", "LaserSight");
                    //__state = true;
                }
            }
        }
        public static void Postfix(SetBoolValue __instance, bool __state)
        {
            if (__state)
            {
                __instance.Fsm.Event("light found");
                __instance.boolValue = false;
                __instance.boolVariable.Value = false;
            }
        }
    }*/

    public static void SendLocationFromPickup(Fsm __instance, FsmState toState) {
        string sceneName = __instance.GameObject.scene.name;
        UnityEngine.Debug.Log(sceneName);
        string id = __instance.GameObject.transform.parent.parent.parent.parent.GetChild(0).name;
        UnityEngine.Debug.Log(id);
        foreach (Location location in Plugin.ArchipelagoClient.Locations.Values) {
            if (location.region == sceneName && location.id == id) {
                UnityEngine.Debug.Log("Sending location " + location.apid + " from pickup");
                Plugin.ArchipelagoClient.SendLocationCheck(location.apid);
                break;
            }
        }
    }

    public static void SendLocationFromCrate(Fsm __instance, FsmState toState) {
        string sceneName = __instance.GameObject.scene.name;
        UnityEngine.Debug.Log(sceneName);
        string id = __instance.GameObject.transform.parent.parent.parent.parent.parent.parent.parent.GetChild(0).name;
        UnityEngine.Debug.Log(id);
        foreach (Location location in Plugin.ArchipelagoClient.Locations.Values) {
            if (location.region == sceneName && location.id == "Wooden Crate "+ id) {
                UnityEngine.Debug.Log("Sending location " + location.apid + " from pickup");
                Plugin.ArchipelagoClient.SendLocationCheck(location.apid);
                break;
            }
        }
    }

    public static void SendLocationFromObject(Fsm __instance, FsmState toState) {
        string sceneName = __instance.GameObject.scene.name;
        UnityEngine.Debug.Log(sceneName);
        string id = __instance.GameObject.transform.parent.parent.parent.name;
        UnityEngine.Debug.Log(id);
        foreach (Location location in Plugin.ArchipelagoClient.Locations.Values) {
            if (location.region == sceneName && location.id == id) {
                UnityEngine.Debug.Log("Sending location " + location.apid + " from trashcan");
                Plugin.ArchipelagoClient.SendLocationCheck(location.apid);
                break;
            }
        }
    }

    public static void SendSpecialLocation(string sceneName, string id) {
        foreach (Location location in Plugin.ArchipelagoClient.Locations.Values) {
            if (location.region == sceneName && location.id == id) {
                Plugin.ArchipelagoClient.SendLocationCheck(location.apid);
                break;
            }
        }
    }
}