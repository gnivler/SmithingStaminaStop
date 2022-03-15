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
#if DEBUG
            FileLog.Log($"[Smithing Stamina Stop] {input ?? "null"}");
#endif
        }

        [HarmonyPatch(typeof(CraftingCampaignBehavior), "HourlyTick")]
        public static class CraftingCampaignBehaviorOnHourlyTickPatch
        {
            private static void Postfix(CraftingCampaignBehavior __instance)
            {
                if (stopWhenFull && 
                    MobileParty.MainParty.CurrentSettlement != null &&
                    MobileParty.MainParty.CurrentSettlement.IsTown)
                {
                    foreach (Hero hero in Helpers.CraftingHelper.GetAvailableHeroesForCrafting())
                    {
                        if (__instance.GetHeroCraftingStamina(hero) >= __instance.GetMaxHeroCraftingStamina(hero))
                        {
                            stopWhenFull = false;
                            InformationManager.AddQuickInformation(new TextObject("Full stamina"));
                            GameMenu.SwitchToMenu("town");
                            Campaign.Current.TimeControlMode = CampaignTimeControlMode.Stop;
                            break;
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(GameMenuItemVM), nameof(GameMenuItemVM.ExecuteAction))]
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
