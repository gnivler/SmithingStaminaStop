using System;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade;

// ReSharper disable InconsistentNaming

namespace Smithing_Stamina_Stop
{
    public class Mod : MBSubModuleBase
    {
        private static readonly Harmony harmony = new Harmony("ca.gnivler.bannerlord.SmithingStaminaStop");
        
        protected override void OnSubModuleLoad()
        {
            try
            {
                //Harmony.DEBUG = true;
                Log("Startup " + DateTime.Now.ToShortTimeString());
                ManualPatches();
                harmony.PatchAll(Assembly.GetExecutingAssembly());
                //Harmony.DEBUG = false;
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        private static void ManualPatches()
        {
            try
            {
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        [HarmonyPatch(typeof(CraftingCampaignBehavior), "HourlyTick")]
        public static class CraftingCampaignBehaviorOnHourlyTickPatch
        {
            private static void Postfix(CraftingCampaignBehavior __instance)
            {
                try
                {
                    // time will stop at 100 stamina unless holding shift 
                    var skip = Input.IsKeyDown(InputKey.LeftShift) || Input.IsKeyDown(InputKey.RightShift);
                    if (!skip &&
                        MobileParty.MainParty.CurrentSettlement != null &&
                        MobileParty.MainParty.CurrentSettlement.IsTown &&
                        __instance.GetHeroCraftingStamina(Hero.MainHero) >= 100)
                    {
                        Log("HourlyTick");
                        MessageManager.DisplayMessage("Smithing stamina is 100");
                        GameMenu.SwitchToMenu("town");
                        Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
                    }
                }
                catch (Exception ex)
                {
                    Log(ex);
                }
            }
        }

        private static void Log(object input)
        {
            //FileLog.Log($"[Smithing Stamina Stop] {input ?? "null"}");
        }
    }
}
