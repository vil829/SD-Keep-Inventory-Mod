using System.Reflection;
using FishNet.Connection;
using HarmonyLib;
using MelonLoader;
using Players;
using Players.Network.Services;
using Players.Services;

[assembly: MelonInfo(typeof(KeepInventoryMod), "KeepInventoryMod", "1.0", "Vil")]

public class KeepInventoryMod : MelonMod 
{ 
    private static FieldInfo PlayersServerField;
    private static MethodInfo RespawnAtSurfaceMethod;
    private static MethodInfo RespawnAtShipMethod;
    
    public override void OnInitializeMelon()
    {
        base.OnInitializeMelon();
        
        HarmonyLib.Harmony harmony = new HarmonyLib.Harmony("vil.KeepInventoryMod");
        
        
        MethodInfo rpcLogicRespawnMethod = AccessTools.Method(typeof(Players.Network.Services.PlayerRespawnService), "RpcLogic___Respawn_ServerRpc___328543758");
        if (rpcLogicRespawnMethod == null)
        {
            MelonLogger.Error("rpcLogicRespawnMethod is null");
            return;
        }

        PlayersServerField = AccessTools.Field(typeof(PlayerRespawnService), "_playersServer");
        RespawnAtSurfaceMethod = AccessTools.Method(typeof(PlayerRespawnService), "RespawnAtSurface");
        RespawnAtShipMethod = AccessTools.Method(typeof(PlayerRespawnService), "RespawnAtShip");

        if (PlayersServerField == null)
        {
            MelonLogger.Error("PlayersServerField is null");
            return;
        }
        
        if (RespawnAtSurfaceMethod == null)
        {
            MelonLogger.Error("RespawnAtSurfaceMethod is null");
            return;
        }
        
        if (RespawnAtShipMethod == null)
        {
            MelonLogger.Error("RespawnAtShipMethod is null");
            return;
        }

        harmony.Patch(rpcLogicRespawnMethod,prefix: new  HarmonyMethod(typeof(KeepInventoryMod), nameof(RpcLogicRespawnServer)));

        MelonLogger.Msg("Keep Inventory Mod loaded!");
    }
    
    private static bool RpcLogicRespawnServer(NetworkConnection __0, PlayerRespawnService __instance)
    {
        //FishNet rewrites IL at runtime resulting in methods with nameless parameters like this one thus we index the parameter __0
        //Decompiled code from DnSpyEx 
        if (__0 == null)
            return false;
        
        PlayersServerTracker playersServers = PlayersServerField.GetValue(__instance) as PlayersServerTracker;
        TrackedPlayerServer trackedPlayerFromConnectionId = playersServers.GetTrackedPlayerFromConnectionId(__0.ClientId);
        
        if (trackedPlayerFromConnectionId == null)
            return false;
        
        trackedPlayerFromConnectionId.ResetHealthAndOxygen();
        //trackedPlayerFromConnectionId.SetInventoryToBasicGear();
        if (trackedPlayerFromConnectionId.RespawnPoint.UseDefaultSpawn)
        {
            RespawnAtSurfaceMethod.Invoke(__instance, new object[] { __0});
            return false;
        }
        
        RespawnAtShipMethod.Invoke(__instance, new object[] { __0 });
        return false;
    }
}    

