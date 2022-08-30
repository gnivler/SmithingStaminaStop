using System;
using System.Reflection;
using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.ViewModelCollection.GameMenu;
using TaleWorlds.Core;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

// ReSharper disable InconsistentNaming

namespace SmithingStaminaStop
{
    public class SubModule : MBSubModuleBase
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
                    __instance.GetHeroCraftingStamina(Hero.MainHero) >= __instance.GetMaxHeroCraftingStamina(Hero.MainHero))
                {
                    stopWhenFull = false;
                    InformationManager.DisplayMessage(new InformationMessage("Full stamina"));
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
                    InformationManager.DisplayMessage(new InformationMessage("Waiting until stamina is full"));
                    stopWhenFull = true;
                }

                if (stopWhenFull &&
                    __instance.OptionID == "wait_leave" &&
                    Hero.MainHero.CurrentSettlement.IsTown)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Cancelling stamina stop"));
                    stopWhenFull = false;
                }
            }
        }
    }
}
