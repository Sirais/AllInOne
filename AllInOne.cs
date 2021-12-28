using System;
using System.Collections.Generic;
using SharpDX;
using System.Threading;
using System.Windows.Forms;
using Druzil.Poe.Libs;
using ExileCore;
using ExileCore.PoEMemory.Components;
using ExileCore.PoEMemory.Elements;
using ExileCore.PoEMemory.Elements.InventoryElements;
using ExileCore.PoEMemory.MemoryObjects;
using ExileCore.PoEMemory.Models;
using ExileCore.Shared;
using ExileCore.Shared.Abstract;
using ExileCore.Shared.Enums;
using ExileCore.Shared.Helpers;
using ImGuiNET;
using TreeRoutine.Menu;


//Todo : Shift state der tastatur beim schließen zurücksetzten, ausschalten crafting mit F8 


namespace AllInOne
{
    internal class AllInOne : BaseSettingsPlugin<AllInOneSettings>
    {

        private bool isCraftingWindowVisible;
        private bool doCraft;
        bool useScraps = true;
        int sockets = 5;
        int links = 5;
        private string lastCurrency;
        private int lastState = 0;

        private IngameUIElements ingameUI;


        public override void OnLoad()
        {
        }

        public override bool Initialise()
        {
            ingameUI = GameController.IngameState.IngameUi;
            isCraftingWindowVisible = false;
            lastCurrency = "";
            doCraft = false;
            return true;
        }

        public override void AreaChange(AreaInstance area)
        {
            ingameUI = GameController.IngameState.IngameUi;
        }

        public override void DrawSettings()
        {
            ImGuiTreeNodeFlags collapsingHeaderFlags = ImGuiTreeNodeFlags.CollapsingHeader;

            Settings.HotKey.Value = ImGuiExtension.HotkeySelector("Hotkey", Settings.HotKey);

            if (ImGui.TreeNodeEx("Q40 Picker", collapsingHeaderFlags))
            {
                Settings.EnableQ40.Value = ImGuiExtension.Checkbox("Enable Q40", Settings.EnableQ40);
                ImGui.Separator();
                Settings.ExtraDelayQ40.Value = ImGuiExtension.IntSlider("extra Delay between clicks", Settings.ExtraDelayQ40.Value, 1, 500);
                Settings.MaxGemQuality.Value = ImGuiExtension.IntSlider("Maximum Quality to Sell", Settings.MaxGemQuality);
                Settings.MaxGemLevel.Value = ImGuiExtension.IntSlider("Maximum Gem level to Sell", Settings.MaxGemLevel);
                ImGui.TreePop();
            }
            if (ImGui.TreeNodeEx("Craftie", collapsingHeaderFlags))
            {
                Settings.EnableCraft.Value = ImGuiExtension.Checkbox("Enable Crafting of items", Settings.EnableCraft);
                Settings.ExtraDelayCraftie.Value = ImGuiExtension.IntSlider("extra Delay between crafting clicks", Settings.ExtraDelayCraftie.Value, 1, 500);
                //ImGui.Separator();
                ImGui.TreePop();
            }
            if (ImGui.TreeNodeEx("Aura enabler", collapsingHeaderFlags))
            {
                Settings.EnableAura.Value = ImGuiExtension.Checkbox("Enable Aura Recaster", Settings.EnableAura);
                ImGui.TreePop();
            }
            if (ImGui.TreeNodeEx("Golem recaster", collapsingHeaderFlags))
            {
                Settings.EnableGolem.Value = ImGuiExtension.Checkbox("Enable Golem Recaster", Settings.EnableGolem);
                ImGui.TreePop();
            }
            if (ImGui.TreeNodeEx("ShowMySkellies (Dark Pact Build)", collapsingHeaderFlags))
            {
                Settings.EnableSMSkellies.Value = ImGuiExtension.Checkbox("Enable Position and info for skellies", Settings.EnableSMSkellies);
                ImGui.TreePop();
            }
            if (ImGui.TreeNodeEx("Itemlevel Frame", collapsingHeaderFlags))
            {
                Settings.EnableILFrame.Value = ImGuiExtension.Checkbox("Enable Frame for Itemlevel (Chaos items)", Settings.EnableILFrame);
                ImGui.TreePop();
            }
        }


        public override void Render()
        {
            base.Render();
            if (!Settings.Enable.Value) // Plugin enabled ?
                return; // no, do nothing
            // Automatic routines
            if (Settings.EnableILFrame)
                MarkILvl();
            if (Settings.EnableAura);
            if (Settings.EnableGolem) ;
            if (Settings.EnableSMSkellies)
                ShowMySkellies();
            // Interactive and triggered Stuff
            if (Settings.HotKey.PressedOnce())
            { 
                //Triggered Hotkey routines
                if (Settings.EnableQ40)
                    Q40Pick();

                if (Settings.EnableCraft)
                    ToggleCraftie();
            }
            Craftie();
        }


        #region Q40 Pick Stuff
        private void Q40Pick()
        {
            if (!ingameUI.StashElement.IsVisible)
            {
                LogMessage($"No Open Stash -> leaving ", 1);
                return;
            }
            if (((ingameUI.StashElement.VisibleStash.InvType != InventoryType.NormalStash) &&
                (ingameUI.StashElement.VisibleStash.InvType != InventoryType.QuadStash)))
            {
                LogMessage($"No Normal or Quad Stash Open  -> leaving Q40", 1);
                return;
            }

            KeyboardHelper.KeyPress(Settings.HotKey.Value); // send the hotkey back to the system to turn off the Work

            List<setData> hits;
            hits = getQualityType("Skill Gem"); // Try to find gems
            if (hits == null || hits.Count == 0)
                hits = getQualityType("Flask"); //No gems so try flasks
            if (hits == null || hits.Count == 0)
            {
                LogMessage("No Quality Items found ", 1);
                return;
            }
            LogMessage($"AllInOne: Q40 found  {hits.Count} Quality Items in open stash.", 1);

            SetFinder Sets = new SetFinder(hits, 40);

            if (Sets.BestSet == null)
            {
                LogMessage("Added Quality is not 40", 1);
                return;
            }

            pickup(Sets);

        }

        /// <summary>
        /// Displays found set as  Logmessage. Just for debugging 
        /// </summary>
        /// <param name="Sets"></param>
        private void displaySet(SetFinder Sets)
        {
            LogMessage($"V5 :found set for Q40 contains {Sets.BestSet.Values.Count} Gems", 10);

            int i = 1;
            foreach (QualityItem g in Sets.BestSet.Values)
            {
                LogMessage($"{i} - Q{g.getValue()} X{g.CheckItem.InventPosX} - Y{g.CheckItem.InventPosY}", 10);
                i++;
            }
        }

        /// <summary>
        /// Pickup found Items into main inventory 
        /// </summary>
        /// <param name="Sets"></param>
        private void pickup(SetFinder Sets)
        {
            foreach (QualityItem g in Sets.BestSet.Values)
            {
                RectangleF itmPos = g.CheckItem.GetClientRect();
                KeyboardHelper.KeyDown(System.Windows.Forms.Keys.LControlKey);
                Thread.Sleep(20);//((int)GameController.Game.IngameState.CurLatency);
                Mouse.SetCursorPosAndLeftClick(RandomizedCenterPoint(itmPos), GameController.Window.GetWindowRectangle().TopLeft);
                Thread.Sleep(20);//((int)GameController.Game.IngameState.CurLatency);
                KeyboardHelper.KeyUp(System.Windows.Forms.Keys.LControlKey);
                Thread.Sleep(20);//((int)GameController.Game.IngameState.CurLatency);
                Thread.Sleep(Settings.ExtraDelayQ40);
            }
        }

        /// <summary>inventory
        /// "Skill Gem"
        /// </summary>
        /// <returns></returns>
        private List<setData> getQualityType(string itemtype)
        {
            List<setData> res = new List<setData>();
            var stashPanel = ingameUI.StashElement;
            if (!stashPanel.IsVisible)
                return null;
            var visibleStash = stashPanel.VisibleStash;
            if (visibleStash == null)
                return null;
            IList<NormalInventoryItem> inventoryItems = ingameUI.StashElement.VisibleStash.VisibleInventoryItems;
            if (inventoryItems != null)
            { 
                foreach (NormalInventoryItem item in inventoryItems)
                {
                    BaseItemType baseItemType = GameController.Files.BaseItemTypes.Translate(item.Item.Path);

                    if (baseItemType.ClassName.Contains(itemtype))
                    {
                        int Quality = item.Item.GetComponent<Quality>().ItemQuality;
                        if (Quality > 0)
                            if (Quality <= Settings.MaxGemQuality)
                                res.Add(new QualityItem(item, Quality));
                    }
                }
            }
            return res;
        }

        #endregion

        #region Mrk Ilvl 
        private void MarkILvl ()
        {
            var stashPanel = ingameUI.StashElement;
            if (!stashPanel.IsVisible)
                return;
            var visibleStash = stashPanel.VisibleStash;
            if (visibleStash == null)
                return;
            if (!((visibleStash.InvType != InventoryType.NormalStash) || (visibleStash.InvType != InventoryType.QuadStash)))
            {
                return;
            }

            IList<NormalInventoryItem> inventoryItems = ingameUI.StashElement.VisibleStash.VisibleInventoryItems;
            if (inventoryItems == null)
                return;
            foreach (NormalInventoryItem item in inventoryItems)
            {
                var mods = item.Item?.GetComponent<Mods>();

                if (mods?.ItemRarity == ItemRarity.Rare)
                {
                    var borderColor = Color.White;
                    if (mods.ItemLevel < 60)
                        borderColor = Color.DarkGray;
                    else if (mods.ItemLevel < 75)
                        borderColor = Color.Yellow;
                    else
                        borderColor = Color.Green;
                    var rect = item.GetClientRect();
                    rect.X += 2;
                    rect.Y += 2;
                    rect.Width -= 4;
                    rect.Height -= 4;
                    Graphics.DrawFrame(rect, borderColor, 1);

                }
            }
        }
        #endregion

        #region Skellie Stuff 
        private void ShowMySkellies()
        {

        }

        #endregion

        #region Craftie stuff
        private void ToggleCraftie()
        {
            LogMessage("Toggle Craftie", 1);
            if (!ingameUI.StashElement.IsVisible)
            {
                ResetAll("");
                return;
            }
            if (ingameUI.StashElement.VisibleStash.InvType != InventoryType.CurrencyStash)
            {
                ResetAll("");
                return;
            }
            if (isCraftingWindowVisible) // Currently Crafting Window is viisible, so turn it off
            {
                ResetAll("");
                return;

            }
            isCraftingWindowVisible = !isCraftingWindowVisible;
        }
  
        private void Craftie ()
        { 
            if (isCraftingWindowVisible)
            {
                if (!ingameUI.StashElement.IsVisible)
                {
                    ResetAll($"No Open Stash -> leaving ");
                    return;
                }
                if (ingameUI.StashElement.VisibleStash.InvType != InventoryType.CurrencyStash)
                {
                    ResetAll($"Crafing only in Curerncy Stash -> leaving");
                    return;
                }

                NormalInventoryItem itemToCraft = CraftingItemFromCurrencyStash();
                if (itemToCraft == null)
                {
                    ResetAll($"No Item To Craft -> leaving");
                    return;
                }

                if (!IsCraftable(itemToCraft))
                {
                    ResetAll($"Item not craftable -> leaving");
                    return;
                }
                CraftWindow(itemToCraft);
                if (doCraft)
                    CraftIt(itemToCraft);
            }
        }

        private void ResetAll (string msg)
        {
            if (!string.IsNullOrEmpty(msg))
                LogMessage(msg, 1); //Show info
            isCraftingWindowVisible = false; // turn off crafting if Stash is changed!
            if (KeyboardHelper.IsKeyDown(System.Windows.Forms.Keys.LShiftKey) && doCraft) // release shift state 
                KeyboardHelper.KeyUp(System.Windows.Forms.Keys.LShiftKey);
            doCraft = false; // crafting inactive
            lastState = 0;
        }
        private void ResetAll()
        {
            ResetAll("");
        }

        private void CraftWindow(NormalInventoryItem itemToCraft)
        {
            
            //LogMessage("CraftWindow", 1);
            System.Numerics.Vector2 Pos = new System.Numerics.Vector2(ingameUI.StashElement.VisibleStash.Position.X + ingameUI.StashElement.VisibleStash.Width,     ingameUI.StashElement.VisibleStash.Position.Y);

            ImGui.Begin("Craftie");
            ImGui.SetNextWindowSizeConstraints(Pos, new System.Numerics.Vector2(300, 300));

            ImGui.TextColored(Color.Yellow.ToImguiVec4(), GameController.Files.BaseItemTypes.Translate(itemToCraft.Item.Path).ClassName);

            ImGui.Checkbox("Use Scraps", ref useScraps);
            ImGui.SliderInt("Min Slots", ref sockets, 0, 6);
            ImGui.SliderInt("Min Links", ref links, 0, 6);

            
            if (doCraft)
            {
                if (ImGui.Button("Stop Crafting")) doCraft=false;
            }
            else
            {
                if (ImGui.Button("Crafit")) doCraft = true;
            }
            ImGui.End();
        }

        public void CraftIt(NormalInventoryItem itemToCraft)
        {
            int cnt = 0;
            while (!GameController.Window.IsForeground()) // ImGui makes Poe not beeing the forground Window, so make it active
            {
                LogMessage($"activating Poe {cnt}");
                WinApi.SetForegroundWindow(GameController.Window.Process.MainWindowHandle);
                Thread.Sleep(100);
                cnt++;
                if (cnt>20)
                {
                    ResetAll("");
                    return;
                }
            }

            var x = itemToCraft;
            string orb = "";
            if (itemToCraft.Item.HasComponent<Quality>() && useScraps)
            {
                Quality comp = itemToCraft.Item.GetComponent<Quality>();
                if (comp.ItemQuality < 20)
                    switch (getType(itemToCraft))
                    {
                        case "weapon":
                            orb = "Blacksmith's Whetstone";
                            break;
                        case "armour":
                            orb = "Armourer's Scrap";
                            break;
                        case "flask":
                            orb = "Glasblower's Bauble";
                            break;
                        default:
                            orb = "";
                            break;
                    }

            }

            //if (itemToCraft.Item.HasComponent<Sockets>() && sockets>0)
            //{

            //}
            //if (itemToCraft.Item.GetComponent<Sockets>)
            //{

            //}

            if (orb != "")
            {
                NormalInventoryItem Currency = DoWeHaveCurrency(orb);
                if (Currency != null)
                {
                    var currencyPos = CenterPoint(Currency.GetClientRect());
                    var windowOffset = GameController.Window.GetWindowRectangle().TopLeft;
                    switch (lastState)
                    {
                        case 0: // Rightklick crafting currency
                            //Mouse.SetCursorPosAndRightClick(currencyPos, windowOffset, 10);
                            Mouse.SetCursorPosAndLeftClick(currencyPos, windowOffset);//, 10);
                            lastState = 1;
                            ResetAll();
                            break;
                        case 1: // Press shift key
                            KeyboardHelper.KeyDown(System.Windows.Forms.Keys.LShiftKey); // Make sure shift is down
                            LogMessage("Craftit Laststate 0", 1);
                            lastState = 2;
                            break;
                        case 2: // Leftklick Crafting item
                            LogMessage("Craftit Laststate 3", 1);
                            Mouse.SetCursorPosAndLeftClick(itemToCraft.GetClientRect().Center, windowOffset, 10);
                            break;
                    }

                    Thread.Sleep(Settings.ExtraDelayCraftie.Value);
                    lastCurrency = orb;
                }
                else
                    ResetAll($"Crafting Currency {orb} not found");
            }
        }

        private void CraftWithCurrency (NormalInventoryItem itemToCraft, NormalInventoryItem currency)
        {
            LogMessage($"Using {currency}", 1);
        }


        private bool IsCraftable(NormalInventoryItem item)
        {
            if (item.Item.GetComponent<Base>().isCorrupted)
            {
                LogMessage("Item is non-craftable (corrupted).", 5);
                return false;
            }

            if (!item.Item.GetComponent<Mods>().Identified)
            {
                LogMessage("Item is non-craftable (unidentified).", 5);
                return false;
            }

            if (item.Item.GetComponent<Mods>().IsMirrored)
            {
                LogMessage("Item is non-craftable (Mirrored).", 5);
                return false;
            }

            return true;
        }

        private NormalInventoryItem DoWeHaveCurrency(string baseName)
        {
            NormalInventoryItem itm = null;
            IList<NormalInventoryItem> inventoryItems = ingameUI.StashElement.VisibleStash.VisibleInventoryItems;
            if (inventoryItems == null || inventoryItems.Count == 0)
            {
                return null;
            }
            int cnt = 0;
            while (itm == null && cnt < inventoryItems.Count)
                foreach (NormalInventoryItem item in inventoryItems)
                {
                    if (GameController.Files.BaseItemTypes.Translate(item.Item.Path).BaseName.Equals(baseName))
                    {
                        itm = item;
                        break;
                    }
                }
            return itm;
        }


        /// <summary>
        /// Find the first item in Currency Stash that is not a currency.
        /// </summary>
        /// <returns></returns>
        private NormalInventoryItem CraftingItemFromCurrencyStash()
        {
            IList<NormalInventoryItem> inventoryItems = ingameUI.StashElement.VisibleStash.VisibleInventoryItems;
            NormalInventoryItem itm = null;
            if (inventoryItems != null)
            {
                int cnt = 0;
                while (itm == null && cnt<inventoryItems.Count)
                foreach (NormalInventoryItem item in inventoryItems)
                {
                        if (!item.Item.GetComponent<RenderItem>().ResourcePath.Contains("Currency"))
                        {
                            itm = item;
                            break;
                        }
                }
            }
            return itm;
        }

        #endregion


        #region basic Stuff 

        private string getType(NormalInventoryItem itm)
        {
            if (isWeapon(itm)) return "weapon";
            if (isArmour(itm)) return "armour";
            if (isFlask(itm)) return "flask";
            if (isJewelry(itm)) return "jewelry";
            if (isJewel(itm)) return "jewel";
            return "";
        }

        private bool isWeapon(NormalInventoryItem itm)
        {
            List<string> Weapons = new List<string>
            {
                "One Hand Mace",
                "Two Hand Mace",
                "One Hand Axe",
                "Two Hand Axe",
                "One Hand Sword",
                "Two Hand Sword",
                "Thrusting One Hand Sword",
                "Bow",
                "Claw",
                "Dagger",
                "Sceptre",
                "Staff",
                "Wand"
            };
            return (Weapons.Contains(GameController.Files.BaseItemTypes.Translate(itm.Item.Path).ClassName));
        }

        private bool isArmour(NormalInventoryItem itm)
        {
            List<string> Weapons = new List<string>
            {
                "Body Armour",
                "Boots",
                "Gloves",
                "Helmet"
            };
            return (Weapons.Contains(GameController.Files.BaseItemTypes.Translate(itm.Item.Path).ClassName));
        }

        private bool isJewelry(NormalInventoryItem itm)
        {
            List<string> Weapons = new List<string>
            {
                "Belt",
                "Ring",
                "Amulet"
            };
            return (Weapons.Contains(GameController.Files.BaseItemTypes.Translate(itm.Item.Path).ClassName));
        }


        private bool isJewel(NormalInventoryItem itm)
        {
            List<string> Weapons = new List<string>
            {
                "Jewel",
                "AbyssJewel"
            };
            return (Weapons.Contains(GameController.Files.BaseItemTypes.Translate(itm.Item.Path).ClassName));
        }


        private bool isFlask(NormalInventoryItem itm)
        {
            List<string> Weapons = new List<string>
            {
                "UtilityFlask",
                "ManaFlask",
                "HybridFlask",
                "LifeFlask"
            };
            return (Weapons.Contains(GameController.Files.BaseItemTypes.Translate(itm.Item.Path).ClassName));
        }


        private static Vector2 CenterPoint(RectangleF rec)
        {
            var randomized = rec.Center;
            return randomized;
        }

        // Randomitze clicking Point for Items
        private static Vector2 RandomizedCenterPoint(RectangleF rec)
        {
            var randomized = rec.Center;
            var xOffsetMin = (int)(-1 * rec.Width / 2) + 2;
            var xOffsetMax = (int)(rec.Width / 2) - 2;
            var yOffsetMin = (int)(-1 * rec.Height / 2) + 2;
            var yOffsetMax = (int)(rec.Height / 2) - 2;
            var random = new Random();

            randomized.X += random.Next(xOffsetMin, xOffsetMax);
            randomized.Y += random.Next(yOffsetMin, yOffsetMax);

            return randomized;
        }


        #endregion

    }

}

