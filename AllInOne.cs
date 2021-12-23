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



namespace AllInOne
{
    internal class AllInOne : BaseSettingsPlugin<AllInOneSettings>
    {

        private bool isCrafting;
        bool useScraps = true;
        int slots = 5;
        int links = 5;

        private IngameUIElements ingameUI;


        public override void OnLoad()
        {
        }

        public override bool Initialise()
        {
            ingameUI = GameController.IngameState.IngameUi;
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
                Settings.MaxGemQuality.Value = ImGuiExtension.IntSlider("Maximum Quality to Sell", Settings.MaxGemQuality);
                Settings.MaxGemLevel.Value = ImGuiExtension.IntSlider("Maximum Gem level to Sell", Settings.MaxGemLevel);
                ImGui.TreePop();
            }
            if (ImGui.TreeNodeEx("Aura enabler", collapsingHeaderFlags))
            {
                Settings.EnableAura.Value = ImGuiExtension.Checkbox("Enable Aura Recaster", Settings.EnableAura);
                //ImGui.Separator();
                ImGui.TreePop();
            }
            if (ImGui.TreeNodeEx("Golem recaster", collapsingHeaderFlags))
            {
                Settings.EnableGolem.Value = ImGuiExtension.Checkbox("Enable Golem Recaster", Settings.EnableGolem);
                //ImGui.Separator();
                ImGui.TreePop();
            }
            if (ImGui.TreeNodeEx("Itemlevel Frame", collapsingHeaderFlags))
            {
                Settings.EnableILFrame.Value = ImGuiExtension.Checkbox("Enable Frame for Itemlevel (Chaos items)", Settings.EnableILFrame);
                //ImGui.Separator();
                ImGui.TreePop();
            }
            if (ImGui.TreeNodeEx("Craftie", collapsingHeaderFlags))
            {
                Settings.EnableCraft.Value = ImGuiExtension.Checkbox("Enable Crafting of items", Settings.EnableCraft);
                //ImGui.Separator();
                ImGui.TreePop();
            }
            if (ImGui.TreeNodeEx("ShowMySkellies (Dark Pact Build)", collapsingHeaderFlags))
            {
                Settings.EnableSMSkellies.Value = ImGuiExtension.Checkbox("Enable Position and info for skellies", Settings.EnableSMSkellies);
                //ImGui.Separator();
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
            if (Settings.EnableAura)
                ;
            if (Settings.EnableGolem)
                ;
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
                Thread.Sleep(50);//((int)GameController.Game.IngameState.CurLatency);
                Mouse.SetCursorPosAndLeftClick(RandomizedCenterPoint(itmPos), GameController.Window.GetWindowRectangle().TopLeft);
                Thread.Sleep(50);//((int)GameController.Game.IngameState.CurLatency);
                KeyboardHelper.KeyUp(System.Windows.Forms.Keys.LControlKey);
                Thread.Sleep(50);//((int)GameController.Game.IngameState.CurLatency);
                Thread.Sleep(Settings.ExtraDelay);
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
            if (!ingameUI.StashElement.IsVisible)
            {
                isCrafting = false;
                return;
            }
            if (((ingameUI.StashElement.VisibleStash.InvType != InventoryType.CurrencyStash)))
            {
                return;
            }
    
            LogMessage($"Toggle", 1);
            if (!isCrafting) // Hotkey Toggled
            {
                isCrafting = false;
                LogMessage($"Craftie: Hotkey currently toggled, press {Settings.HotKey.Value.ToString()} to disable.", 1);

            }
            isCrafting = !isCrafting;
        }
  
        private void Craftie ()
        { 
            if (isCrafting)
            {
                if (!ingameUI.StashElement.IsVisible)
                {
                    LogMessage($"No Open Stash -> leaving ", 1);
                    isCrafting = false; // turn off crafting if Stash is closed!
                    return;
                }
                if (ingameUI.StashElement.VisibleStash.InvType != InventoryType.CurrencyStash)
                {
                    LogMessage($"Crafing only in Curerncy Stash -> leaving", 1);
                    isCrafting = false; // turn off crafting if Stash is changed!
                    return;
                }
                CraftWindow();
                NormalInventoryItem itemToCraft = CraftingItemFromCurrencyStash();
                LogMessage($"Crafting {itemToCraft.Item.ToString()}", 1);
            }
        }

        private void CraftWindow()
        {
            bool windowState = true;
            System.Numerics.Vector2 Pos = new System.Numerics.Vector2(ingameUI.StashElement.VisibleStash.Position.X + ingameUI.StashElement.VisibleStash.Width,     ingameUI.StashElement.VisibleStash.Position.Y);

            ImGui.Begin("Craftie");// , ref windowState,ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoBringToFrontOnFocus); //| ImGuiWindowFlags.NoInputs 
            ImGui.SetNextWindowSizeConstraints(Pos, new System.Numerics.Vector2(300, 300));
            //ImGui.SetWindowPos(Pos);
            //ImGui.SetWindowSize(new System.Numerics.Vector2(300, 300));


            ImGui.Checkbox("Use Scraps", ref useScraps);
            ImGui.SliderInt("Min Slots", ref slots, 1, 6);
            ImGui.SliderInt("Min Links", ref links, 1, 6);

            if (ImGui.Button("Reload")) CraftIt();
            ImGui.End();
        }

        public void CraftIt()
        {

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

            if (!item.Item.GetComponent<Mods>().IsMirrored)
            {
                LogMessage("Item is non-craftable (Mirrored).", 5);
                return false;
            }


            return true;
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
                            itm = item;
                }
            }
            return itm;
        }

        #endregion

        #region basis Stuff 
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

