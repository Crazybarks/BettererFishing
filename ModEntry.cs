using System;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using GenericModConfigMenu;

namespace BetterFishing
{
    
    public class ModEntry : Mod
    {
        private ModConfig modConfig;
        private bool gotTreasure = false;
        private bool doneFishing = false;

        public override void Entry(IModHelper Helper)
        {
            modConfig = Helper.ReadConfig<ModConfig>();
            Helper.Events.GameLoop.GameLaunched += ConfigUI;
            Helper.Events.Display.MenuChanged += SkipFishingMinigame;
            Helper.Events.Display.MenuChanged += CollectTreasure;
            Helper.Events.GameLoop.UpdateTicked += AutoReelInFish;
            Helper.Events.GameLoop.UpdateTicked += CastMaxDistance;
            Helper.Events.GameLoop.UpdateTicked += UnbreakableTackle;
            Helper.Events.Player.InventoryChanged += JunkDoesNotReduceBait;
            Helper.Events.GameLoop.UpdateTicked += CheckLastCatch;
        }

        private void ConfigUI(object sender, GameLaunchedEventArgs e)
        {
            var configMenu = Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
            if (configMenu is null)
            {
                return;
            }

            configMenu.Register(
                mod: ModManifest,
                reset: () => modConfig = new ModConfig(),
                save: () => Helper.WriteConfig(modConfig)
                );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Enable Better Fishing Mod",
                tooltip: () => "Enables the mod itself",
                getValue: () => modConfig.EnableBetterFishingMod,
                setValue: value => modConfig.EnableBetterFishingMod = value
                 );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Unbreakable Tackle",
                tooltip: () => "Your tackle won't break anymore",
                getValue: () => modConfig.UnbreakableTackle,
                setValue: value => modConfig.UnbreakableTackle = value
                 );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Skip Minigame",
                tooltip: () => "Skips the minigame entirely for time saving",
                getValue: () => modConfig.SkipMiniGame,
                setValue: value => modConfig.SkipMiniGame = value
                 );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Skip Minigame Of Legendary Fish",
                tooltip: () => "Skips the minigame entirely for legendary fish",
                getValue: () => modConfig.SkipMiniGameOfLegendaryFish,
                setValue: value => modConfig.SkipMiniGameOfLegendaryFish = value
                 );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Junk Does Not Reduce Bait",
                tooltip: () => "Catching junk items won't reduce bait",
                getValue: () => modConfig.JunkDoesNotReduceBait,
                setValue: value => modConfig.JunkDoesNotReduceBait = value
                 );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Auto Reel In Fish",
                tooltip: () => "Whether fish will be automatically hooked",
                getValue: () => modConfig.AutoReelInFish,
                setValue: value => modConfig.AutoReelInFish = value
                 );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Auto Obtain Treasure Chest",
                tooltip: () => "Grab treasures automatically",
                getValue: () => modConfig.AutoObtainTreasureChest,
                setValue: value => modConfig.AutoObtainTreasureChest = value
                 );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Auto Grab Treasure Loot",
                tooltip: () => "Put treasures into inventory without a prompt",
                getValue: () => modConfig.AutoGrabTreasureLoot,
                setValue: value => modConfig.AutoGrabTreasureLoot = value
                 );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Always Cast Max Distance",
                tooltip: () => "Whether to always cast at your max distance",
                getValue: () => modConfig.AlwaysCastMaxDistance,
                setValue: value => modConfig.AlwaysCastMaxDistance = value
                 );
            configMenu.AddBoolOption(
                mod: ModManifest,
                name: () => "Always Perfect",
                tooltip: () => "Whether to always catch fish with perfect accuracy",
                getValue: () => modConfig.AlwaysPerfect,
                setValue: value => modConfig.AlwaysPerfect = value
                 );
        }

        private async void AutoReelInFish(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            if (!(modConfig.EnableBetterFishingMod && modConfig.AutoReelInFish)) return;

            FishingRod fishingRod = (FishingRod)(Game1.player.CurrentTool is FishingRod ? Game1.player.CurrentTool : null);
            if (fishingRod != null && Game1.player.UsingTool && fishingRod.isFishing && fishingRod.isNibbling)
            {
                await Task.Delay(modConfig.ReactionTime);
                Game1.player.EndUsingTool();
            }
        }

        private void CastMaxDistance(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            if (!(modConfig.EnableBetterFishingMod && modConfig.AlwaysCastMaxDistance)) return;

            FishingRod fishingRod = (FishingRod)(Game1.player.CurrentTool is FishingRod ? Game1.player.CurrentTool : null);
            if (fishingRod != null && fishingRod.isTimingCast)
            {
                fishingRod.castingPower = 1.01f;
            }
        }

        private void UnbreakableTackle(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            if (!(modConfig.EnableBetterFishingMod && modConfig.UnbreakableTackle)) return;

            FishingRod fishingRod = (FishingRod)(Game1.player.CurrentTool is FishingRod ? Game1.player.CurrentTool : null);
            if (fishingRod != null && fishingRod.attachments[1] != null)
            {
                fishingRod.attachments[1].uses.Set(0);
            }
        }

        private void SkipFishingMinigame(object sender, MenuChangedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            if (!(modConfig.EnableBetterFishingMod && modConfig.SkipMiniGame)) return;

            IClickableMenu newMenu = e.NewMenu;
            BobberBar bobberBarMenu = (BobberBar)((newMenu is BobberBar) ? newMenu : null);
            FishingRod fishingRod = (FishingRod)(Game1.player.CurrentTool is FishingRod ? Game1.player.CurrentTool : null);
            if (bobberBarMenu != null && fishingRod != null)
            {
                string whichFish = Helper.Reflection.GetField<string>(bobberBarMenu, "whichFish", true).GetValue();
                int fishSize = Helper.Reflection.GetField<int>(bobberBarMenu, "fishSize", true).GetValue();
                int fishQuality = Helper.Reflection.GetField<int>(bobberBarMenu, "fishQuality", true).GetValue();
                int bobberBarHeight = Helper.Reflection.GetField<int>(bobberBarMenu, "bobberBarHeight", true).GetValue();
                int motionType = Helper.Reflection.GetField<int>(bobberBarMenu, "motionType", true).GetValue();
                float difficulty = Helper.Reflection.GetField<float>(bobberBarMenu, "difficulty", true).GetValue();
                bool fromFishPond = Helper.Reflection.GetField<bool>(bobberBarMenu, "fromFishPond", true).GetValue();
                bool bossFish = Helper.Reflection.GetField<bool>(bobberBarMenu, "bossFish", true).GetValue();
                bool treasure = Helper.Reflection.GetField<bool>(bobberBarMenu, "treasure", true).GetValue();
                bool treasureCaught = Helper.Reflection.GetField<bool>(bobberBarMenu, "treasure", true).GetValue();
                bool perfect = Helper.Reflection.GetField<bool>(bobberBarMenu, "perfect", true).GetValue();
                int num = (Game1.player.CurrentTool != null && Game1.player.CurrentTool is FishingRod && (Game1.player.CurrentTool as FishingRod).attachments[0] != null) ? (Game1.player.CurrentTool as FishingRod).attachments[0].ParentSheetIndex : (-1);
                bool caughtDouble = !bossFish && num == 774 && Game1.random.NextDouble() < 0.25 + Game1.player.DailyLuck / 2.0;
                int NumCaught = 1;
                if (caughtDouble)
                {
                    NumCaught = 2;
                }

                if (!modConfig.SkipMiniGameOfLegendaryFish && bossFish)
                {
                    return;
                }
                if (modConfig.AutoObtainTreasureChest && treasure)
                {
                    treasureCaught = true;
                    perfect = false;
                    gotTreasure = true;
                }
                else
                {
                    treasureCaught = false;
                    perfect = modConfig.IfPerfect(difficulty, bobberBarHeight, motionType);
                }
                if (modConfig.AlwaysPerfect)
                {
                    perfect = true;
                }
                if (perfect)
                {
                    DelayedAction.playSoundAfterDelay("crit", 200);
                }

                fishingRod.pullFishFromWater(whichFish, fishSize, fishQuality, (int)difficulty, treasureCaught, perfect, fromFishPond, "", bossFish, NumCaught);
                Game1.exitActiveMenu();
                Game1.setRichPresence("location", Game1.currentLocation.Name);
            }
        }

        private void CollectTreasure(object sender, MenuChangedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            if (!(modConfig.EnableBetterFishingMod && modConfig.AutoGrabTreasureLoot)) return;

            if (gotTreasure)
            {
                IClickableMenu newMenu = e.NewMenu;
                ItemGrabMenu itemGrabMenu = (ItemGrabMenu)((newMenu is ItemGrabMenu) ? newMenu : null);
                if (itemGrabMenu != null)
                {
                    gotTreasure = false;
                    IList<Item> actualInventory = itemGrabMenu.ItemsToGrabMenu.actualInventory;
                    int inventoryCount = actualInventory.Count;
                    bool empty = true;

                    for (int i = inventoryCount - 1; i >= 0; i--)
                    {
                        Item item = actualInventory[i];
                        if (Game1.player.couldInventoryAcceptThisItem(item))
                        {
                            actualInventory.Remove(item);
                            Game1.player.addItemToInventory(item);
                        }
                        else
                        {
                            empty = false;
                        }
                    }

                    if(empty)
                    {
                        Game1.exitActiveMenu();
                        Game1.setRichPresence("location", Game1.currentLocation.Name);
                    }
                }
            }
        }

        private void CheckLastCatch(object sender, UpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            if (!(modConfig.EnableBetterFishingMod && modConfig.JunkDoesNotReduceBait)) return;

            FishingRod fishingRod = (FishingRod)(Game1.player.CurrentTool is FishingRod ? Game1.player.CurrentTool : null);
            if (fishingRod != null && fishingRod.attachments[0] != null && fishingRod.isFishing && !doneFishing)
            {
                doneFishing = true;
            }
        }

        private static bool WasJunk(Item item)
        {
            return item.Category == -20 || item.QualifiedItemId == "(O)152" || item.QualifiedItemId == "(O)153" || item.QualifiedItemId == "(O)157" || item.QualifiedItemId == "(O)797" || item.QualifiedItemId == "(O)79" || item.QualifiedItemId == "(O)73" || item.QualifiedItemId == "(O)842" || item.QualifiedItemId == "(O)820" || item.QualifiedItemId == "(O)821" || item.QualifiedItemId == "(O)822" || item.QualifiedItemId == "(O)823" || item.QualifiedItemId == "(O)824" || item.QualifiedItemId == "(O)825" || item.QualifiedItemId == "(O)826" || item.QualifiedItemId == "(O)827" || item.QualifiedItemId == "(O)828" || item.QualifiedItemId == "(O)890";
        }

        private void JunkDoesNotReduceBait(object sender, InventoryChangedEventArgs e)
        {
            if (!Context.IsWorldReady) return;
            if (!(modConfig.EnableBetterFishingMod && modConfig.JunkDoesNotReduceBait)) return;

            if (doneFishing)
            {
                doneFishing = false;
                bool gotJunk = false;
                IEnumerable<Item> itemsAdded = e.Added;
                foreach (Item item in itemsAdded)
                {
                    gotJunk = WasJunk(item);
                }
                IEnumerable<ItemStackSizeChange> itemsQuantityChanged = e.QuantityChanged;
                foreach (ItemStackSizeChange itemQC in itemsQuantityChanged)
                {
                    gotJunk = WasJunk(itemQC.Item);
                }

                if (gotJunk)
                {
                    FishingRod fishingRod = (FishingRod)(Game1.player.CurrentTool is FishingRod ? Game1.player.CurrentTool : null);
                    if (fishingRod != null && fishingRod.attachments[0] != null)
                    {
                        fishingRod.attachments[0].Stack++;
                    }
                }
            }
        }
    }
}