using System;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

// ReSharper disable InconsistentNaming

namespace Smithing_Stamina_Stop
{
    public class Mod : MBSubModuleBase
    {
        private static readonly Harmony harmony = new Harmony("ca.gnivler.bannerlord.SmithingStaminaStop");
        private static bool stopWhenFull;

        protected override void OnSubModuleLoad()
        {
            try
            {
                Log("Startup " + DateTime.Now.ToShortTimeString());
                harmony.PatchAll(Assembly.GetExecutingAssembly());
            }
            catch (Exception e)
            {
                Log(e);
            }
        }

        private static void Log(object input)
        {
            //FileLog.Log($"[Smithing Stamina Stop] {input ?? "null"}");
        }

        [HarmonyPatch(typeof(CraftingCampaignBehavior), "HourlyTick")]
        public static class CraftingCampaignBehaviorOnHourlyTickPatch
        {
            private static void Postfix(CraftingCampaignBehavior __instance)
            {
                if (stopWhenFull &&
                    MobileParty.MainParty.CurrentSettlement != null &&
                    MobileParty.MainParty.CurrentSettlement.IsTown &&
                    __instance.GetHeroCraftingStamina(Hero.MainHero) >= 100)
                {
                    stopWhenFull = false;
                    InformationManager.AddQuickInformation(new TextObject("Full stamina"));
                    GameMenu.SwitchToMenu("town");
                    Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
                }
            }
        }

        [HarmonyPatch(typeof(GameMenuItemVM), "ExecuteAction")]
        public class GameMenuItemVMExecuteActionPatch
        {
            private static void Postfix(GameMenuItemVM __instance)
            {
                Log(__instance.OptionID);
                if (__instance.OptionID == "town_wait" &&
                    (Input.IsKeyDown(InputKey.LeftShift) || Input.IsKeyDown(InputKey.RightShift)))
                {
                    InformationManager.AddQuickInformation(new TextObject("Waiting until stamina is full"));
                    stopWhenFull = true;
                }

                if (stopWhenFull &&
                    __instance.OptionID == "wait_leave" &&
                    Hero.MainHero.CurrentSettlement.IsTown)
                {
                    InformationManager.AddQuickInformation(new TextObject("Cancelling stamina stop"));
                    stopWhenFull = false;
                }
            }
        }
    }
}
