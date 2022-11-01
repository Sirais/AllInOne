using System;
using System.Collections.Generic;
using SharpDX;
using System.Threading;
using System.Windows.Forms;
using Druzil.Poe.Libs;
using ExileCore;
using ExileCore.PoEMemory;
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
using System.Globalization;
using System.Linq;
using ExileCore.Shared.AtlasHelper;
using System.Collections;
using AllInOne.Misc;

//Todo : Shift state der tastatur beim schließen zurücksetzten, ausschalten crafting mit F8 


namespace AllInOne
{

    public enum Routines
    {
        Q40Pick = 1,
        Craftie = 2,
        ResoSplit = 3
    }



    internal class AllInOne : BaseSettingsPlugin<AllInOneSettings>
    {
        private bool isCraftingWindowVisible;
        private bool doCraft;
        int sockets = 5;
        int links = 5;
        private string lastCurrency;
        private int lastState = 0;
        private DateTime timer;
        

        private IngameUIElements ingameUI;


        public AllInOne()
        {

        }

        public override void OnLoad()
        {
            //Load Graphics for Delve Walls 
            Graphics.InitImage("directions.png");
            Graphics.InitImage("Icons.png");
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

//            ImGui.PushStyleColor(ImGuiCol.Header,Color.Green.ToImguiVec4());
            if (ImGui.TreeNodeEx("Q40 Picker", collapsingHeaderFlags))
            {
                Settings.EnableQ40.Value = ImGuiExtension.Checkbox("Enable Q40", Settings.EnableQ40);
                Settings.Q40HotKey.Value = ImGuiExtension.HotkeySelector("Hotkey", Settings.Q40HotKey.Value);
                ImGui.Separator();
                Settings.ExtraDelayQ40.Value = ImGuiExtension.IntSlider("extra Delay between clicks", Settings.ExtraDelayQ40.Value, 1, 500);
                Settings.MaxGemQuality.Value = ImGuiExtension.IntSlider("Maximum Quality to Sell", Settings.MaxGemQuality.Value,1,20);
                Settings.MaxGemLevel.Value = ImGuiExtension.IntSlider("Maximum Gem level to Sell", Settings.MaxGemLevel.Value, 1, 20);
            }
            if (ImGui.TreeNodeEx("Craftie", collapsingHeaderFlags))
            {
                Settings.EnableCraft.Value = ImGuiExtension.Checkbox("Enable Crafting of items", Settings.EnableCraft);
                Settings.CraftHotKey.Value = ImGuiExtension.HotkeySelector("Hotkey", Settings.CraftHotKey.Value);
                Settings.ExtraDelayCraftie.Value = ImGuiExtension.IntSlider("extra Delay between crafting clicks", Settings.ExtraDelayCraftie.Value, 1, 2000);
                ImGui.Separator();
                Settings.useScraps.Value = ImGuiExtension.Checkbox("Use Scraps", Settings.useScraps);
                Settings.useJewellers.Value = ImGuiExtension.Checkbox("Use Jewellers", Settings.useJewellers);
                Settings.minSlots.Value = ImGuiExtension.IntSlider("minimum Slots", Settings.minSlots);
                Settings.useFusings.Value = ImGuiExtension.Checkbox("Use Fusings", Settings.useFusings);
                Settings.minLinks.Value = ImGuiExtension.IntSlider("minimum Links", Settings.minLinks);
            }
            if (ImGui.TreeNodeEx("Delve Walls", collapsingHeaderFlags))
            {
                Settings.EnableDelve.Value = ImGuiExtension.Checkbox("Enable Delve Walls", Settings.EnableDelve);
                Settings.DelveMaxRange.Value = ImGuiExtension.IntSlider("Maximum Distance", Settings.DelveMaxRange);
            }

            //if (ImGui.TreeNodeEx("Resonator Splitter", collapsingHeaderFlags))
            //{
            //    Settings.EnableDelve.Value = ImGuiExtension.Checkbox("Enable Delve Walls", Settings.EnableDelve);
            //    Settings.DelveMaxRange.Value = ImGuiExtension.IntSlider("Maximum Distance", Settings.DelveMaxRange);
            //}

            if (ImGui.TreeNodeEx("Resonator Splitter", collapsingHeaderFlags))
            {
                Settings.EnableResoSplit.Value = ImGuiExtension.Checkbox("Enable Resonator Splitter", Settings.EnableResoSplit);
                Settings.ResoHotKey.Value = ImGuiExtension.HotkeySelector("Hotkey", Settings.ResoHotKey.Value);
            }

            if (ImGui.TreeNodeEx("Aura enabler", collapsingHeaderFlags))
            {
                Settings.EnableAura.Value = ImGuiExtension.Checkbox("Enable Aura Recaster", Settings.EnableAura);
            }
            if (ImGui.TreeNodeEx("Golem recaster", collapsingHeaderFlags))
            {
                Settings.EnableGolem.Value = ImGuiExtension.Checkbox("Enable Golem Recaster", Settings.EnableGolem);
            }
            if (ImGui.TreeNodeEx("ShowMySkellies (Dark Pact Build)", collapsingHeaderFlags))
            {
                Settings.EnableSMSkellies.Value = ImGuiExtension.Checkbox("Enable Position and info for skellies", Settings.EnableSMSkellies);
            }
            if (ImGui.TreeNodeEx("Itemlevel Frame", collapsingHeaderFlags))
            {
                Settings.EnableILFrame.Value = ImGuiExtension.Checkbox("Enable Frame for Itemlevel (Chaos items)", Settings.EnableILFrame);
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
            if (Settings.EnableDelve)
                DelveWalls();
            if (Settings.EnableAura);
            if (Settings.EnableGolem) ;
            if (Settings.EnableSMSkellies)
                ShowMySkellies();
            // Interactive and triggered Stuff
            //if (Settings.Q40HotKey.PressedOnce())
            //{
            //    //Triggered Hotkey routines
            //    if (Settings.EnableQ40)
            //        Q40Pick();
            //}
            if (Settings.CraftHotKey.PressedOnce())
            {
                if (Settings.EnableCraft)
                ToggleCraftie();
            }
            if (Settings.ResoHotKey.PressedOnce())
            {
                if (Settings.EnableResoSplit)
                    Resosplitter();
            }
            if (doCraft)
                Craftie();
        }

        public override Job Tick()
            {
            if (Settings.Q40HotKey.PressedOnce())
            {
                if (Core.ParallelRunner.FindByName(Routines.Q40Pick.ToString()) == null)
                {
                    StartCoroutine(Routines.Q40Pick);
                }
                else
                {
                    StopCoroutine(Name);
                }
            }
            return null;
        }

        private void StartCoroutine(Routines routine)
        {
            switch (routine)
            {
                case Routines.Q40Pick:
                    Core.ParallelRunner.Run(new Coroutine(PickupQ40Routine(), this, Routines.Q40Pick.ToString()));
                    break;
                case Routines.ResoSplit:
                    Core.ParallelRunner.Run(new Coroutine(PickupQ40Routine(), this, Routines.Q40Pick.ToString()));
                    break;

                //case Routines.Craftie:
                //    Core.ParallelRunner.Run(new Coroutine(TurnInDivCardsRoutine(), this, "TurnInDivCards"));
                //    break;
            }
        }

        private void StopCoroutine(string routineName)
        {
            var routine = Core.ParallelRunner.FindByName(routineName);
            routine?.Done();
        }


        #region Q40 Pick Stuff

        private IEnumerator PickupQ40Routine()
        {
            if (!Q40canStart())
            {
                StopCoroutine("Q40Pick");
                yield break;
            }

            //KeyboardHelper.KeyPress(Settings.Q40HotKey.Value); // send the hotkey back to the system to turn off the Work
            List<setData> hits;
            hits = getQualityType("Skill Gem"); // Try to find gems
            if (hits == null || hits.Count == 0)
                hits = getQualityType("Flask"); //No gems so try flasks
            if (hits != null && hits.Count > 0)
            {
                LogMessage($"AllInOne: Q40 found  {hits.Count} Quality Items in open stash.", 1);
                SetFinder Sets = new SetFinder(hits, 40);
                if (Sets.BestSet == null)
                {
                    LogMessage("Added Quality is not 40", 1);
                    StopCoroutine("Q40Pick");
                    yield break;
                }
                yield return Q40pickup(Sets);
            }
            StopCoroutine(Routines.Q40Pick.ToString());
        }

        /// <summary>
        /// Pickup found Items into main inventory 
        /// </summary>
        /// <param name="Sets"></param>
        private IEnumerator Q40pickup(SetFinder Sets)
        {
            foreach (QualityItem g in Sets.BestSet.Values)
            {
                RectangleF itmPos = g.CheckItem.GetClientRect();
                Input.KeyDown(System.Windows.Forms.Keys.LControlKey);
                Input.SetCursorPositionAndClick(g.CheckItem.GetClientRect().Center);
                Thread.Sleep(20);//((int)GameController.Game.IngameState.CurLatency);
                Input.KeyUp(System.Windows.Forms.Keys.LControlKey);
                Thread.Sleep(20);//((int)GameController.Game.IngameState.CurLatency);
                Thread.Sleep(Settings.ExtraDelayQ40);
            }
            yield break;
        }

        private bool Q40canStart()
        {
            if (!ingameUI.StashElement.IsVisible && !ingameUI.SellWindow.IsVisible)
            {
                LogMessage($"No Open Stash  and no Trade Window -> leaving ", 1);
                return false;
            }
            if (ingameUI.StashElement.IsVisible &&
                (ingameUI.StashElement.VisibleStash.InvType != InventoryType.NormalStash) &&
                (ingameUI.StashElement.VisibleStash.InvType != InventoryType.QuadStash)
                )
            {
                LogMessage($"No Normal or Quad Stash Open  -> leaving Q40", 1);
                return false;
            }
            return true;
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

        /// <summary>inventory
        /// "Skill Gem"
        /// </summary>
        /// <returns></returns>
        private List<setData> getQualityType(string itemtype)
        {
            LogMessage($"Chcking Quality items");
            List<setData> res = new List<setData>();
            var stashPanel = ingameUI.StashElement;
            if (!ingameUI.StashElement.IsVisible && !ingameUI.SellWindow.IsVisible)
                return null;
            //var visibleStash = ingameUI.StashElement.VisibleStash;
            //if (visibleStash == null )
            //    return null;
            IList<NormalInventoryItem> inventoryItems = null;
            if (ingameUI.StashElement.IsVisible)
                inventoryItems = ingameUI.StashElement.VisibleStash.VisibleInventoryItems;
            else if (ingameUI.InventoryPanel.IsVisible)
                inventoryItems = ingameUI.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;
            LogMessage($"testing {inventoryItems.Count} items");
            if (inventoryItems != null)
            {
                //LogMessage($"items in Stash {ingameUI.StashElement.VisibleStash.VisibleInventoryItems.ToString()}: {inventoryItems.Count}", 10);
                int cnt = 1;
                foreach (NormalInventoryItem item in inventoryItems)
                {
                    BaseItemType baseItemType = GameController.Files.BaseItemTypes.Translate(item.Item.Path);

                    if (baseItemType.ClassName.Contains(itemtype))
                    {
                        int Quality = item.Item.GetComponent<Quality>().ItemQuality;
                        //LogMessage($"item {cnt} : Quality = {Quality}",10);
                        if (Quality > 0)
                            if (Quality <= Settings.MaxGemQuality)
                                res.Add(new QualityItem(item, Quality));
                        cnt++;
                    }
                }
            }
            return res;
        }



        #endregion

        #region Resonator Splitter

        private IEnumerator ResosplitterRoutine()
        {
            LogMessage("Resonator Splitter", 101);
            if (!ingameUI.InventoryPanel.IsVisible)
            {
                ResetAll("Inventory not open");
                yield break;
            }
            NormalInventoryItem reso = HasResonators();
            if (reso != null)
            {
                LogMessage("Found resonator", 101);
                SplitResonator(reso);
            }
            StopCoroutine(Routines.ResoSplit.ToString());
        }

        private void Resosplitter()
        {
            LogMessage("Resonator Splitter", 101);
            if (!ingameUI.InventoryPanel.IsVisible)
            {
                ResetAll("Inventory not open");
                return;
            }
            NormalInventoryItem reso = HasResonators();
            if (reso != null)
            {
                LogMessage("Found resonator", 101);
                SplitResonator(reso);
            }

            //if (GameController.IngameState.ServerData.PlayerInventories[0].Inventory.Items.Where(x => x.Path == "Metadata/Items/DivinationCards/DivinationCardDeck").Count() < 0;
        }

        private void SplitResonator(NormalInventoryItem item)
        {
            Vector2 freeslot = new Vector2();
            if (GetNextFreeSlot(ref freeslot))
            {
                var windowOffset = GameController.Window.GetWindowRectangle().TopLeft;
                var pos = CenterPoint(item.GetClientRect());
                Input.KeyDown(System.Windows.Forms.Keys.LShiftKey); // Make sure shift is down for continous clicks
                Thread.Sleep(20);
                Input.SetCursorPositionAndClick(item.GetClientRect().Center);
                Thread.Sleep(20);
                Input.KeyPress(System.Windows.Forms.Keys.Return);
                Thread.Sleep(40);
                Input.KeyUp(System.Windows.Forms.Keys.LShiftKey); // Release Shift 
                //Thread.Sleep(20);
                //Mouse.SetCursorPosAndLeftClick(freeslot, windowOffset);
            }


        }

        private bool GetNextFreeSlot(ref Vector2 pos)
        {
            LogMessage("Checking open Slots", 101);
            //ServerInventory 
            Point openSlotPos = Point.Zero;
            IEnumerable<ServerInventory.InventSlotItem> playerInventory = GameController.Game.IngameState.ServerData.PlayerInventories[0].Inventory.InventorySlotItems;
            var slots = GetInventoryLayout(playerInventory); /// Read all inventroryslots
            LogMessage("slots:" + playerInventory.Count().ToString(), 101);
            LogMessage(slots.Print(ref openSlotPos));
            if (!slots.GetNextOpenSlot(ref openSlotPos)) // is there a free slot for the item ? 
            {
                pos= CenterPoint(GetClientRectFromPoint(openSlotPos, 1, 1));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Fills an Array with 1 (used by an item) or 0 (free)
        /// Found this in Plugin UnstackDecks and didnt have to reinvent. Thanks to the Creator
        /// </summary>
        /// <param name="slots"></param>
        /// <returns></returns>
        private static int[,] GetInventoryLayout(IEnumerable<ServerInventory.InventSlotItem> slots)
        {
            
            var inventorySlots = new[,]
            {
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0},
                {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
            };

            foreach (var slot in slots)
            {
                inventorySlots.Fill(1, slot.PosX, slot.PosY, slot.SizeX, slot.SizeY);
            }

            return inventorySlots;
        }

        private RectangleF GetClientRectFromPoint(Point pos, int width, int height)
        {
            var inv = GameController.Game.IngameState.IngameUi.InventoryPanel[InventoryIndex.PlayerInventory].GetClientRect();
            var Size = inv.Width / GameController.Game.IngameState.ServerData.PlayerInventories[0].Inventory.Columns;
            return new RectangleF(
                inv.X + pos.X + Size * pos.X,
                inv.Y + pos.Y + Size * pos.Y,
                pos.X + width * Size,
                pos.Y + height * Size);
        }


        private NormalInventoryItem HasResonators()
        {
            NormalInventoryItem found = null;
            IList<NormalInventoryItem> items = ingameUI.InventoryPanel[InventoryIndex.PlayerInventory].VisibleInventoryItems;
            int cnt = 0;
            while (found == null && cnt < items.Count())
            {
                NormalInventoryItem item = items[cnt];
                if (GameController.Files.BaseItemTypes.Translate(item.Item.Path).BaseName.Contains("Resonator"))
                {
                    var stacksize = item.Item.GetComponent<ExileCore.PoEMemory.Components.Stack>()?.Size ?? 0;
                    //ExileCore.PoEMemory.Components.Stack stack = item.Item?.GetComponent<ExileCore.PoEMemory.Components.Stack>();
                    //if (stack != null && stack.Size > 1)
                    if (stacksize>1)
                        found = item;
                }
                cnt++;
            }
            return found;
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

        #region Delvewalls
        public void DelveWalls()
        {
            if (!GameController.InGame)
                return;
            if (GameController.Area.CurrentArea.IsTown)
                return;
            if (GameController.Area.CurrentArea.IsHideout)
                return;
            if (GameController.IsLoading)
                return;
            if (ingameUI.StashElement.IsVisible)
                return;
            if (ingameUI.InventoryPanel.IsVisible)
                return;
            if (ingameUI.OpenLeftPanel.IsVisible)
                return;
            if (ingameUI.OpenRightPanel.IsVisible)
                return;
            if (ingameUI.DelveWindow.IsVisible) //Talking to niko in HO opens the map too, so not only in Mine 
                Showgrid();
            if (!GameController.Area.CurrentArea.Name.Contains("Azurite Mine")) // Not in Mine /no need to check Hidden walls
                return;
            ShowWalls();
        }

        /// <summary>
        /// Draw the Mine Grid.
        /// Maybe one day i find out about nodes and crap. Stupid to keep it that secret. 
        /// </summary>
        private void Showgrid()
        {
            SubterraneanChart Mine = GameController.Game.IngameState.IngameUi.DelveWindow;
            int c = 0;
            RectangleF connection_area = RectangleF.Empty;
            foreach (DelveBigCell BCell in Mine.GridElement.Cells)
            {
                foreach (DelveCell cell in BCell.Cells)
                {
                    RectangleF rec = cell.GetClientRect();
                    Mine.GetClientRect().Contains(ref rec, out var contains);
                    if (contains)
                    {
                        Graphics.DrawFrame(cell.GetClientRect(), Color.DarkGray, 1);
                        //LogMessage($"Minesrtext[{c}]={ cell.MinesText}", 1, Color.AliceBlue);
                        //LogMessage($"Type[{c}]={ cell.Type}");
                        //LogMessage($"Typehuman[{c}]={ cell.TypeHuman}");

                        foreach (var connection in cell.Children)
                        {
                            int width = (int)connection.Width;
                            if ((width == 10 || width == 4))
                                Graphics.DrawFrame(connection.GetClientRect(), Color.LightGreen, 1);
                        }
                        c++;
                    }
                }
            }
        }

        private void ShowWalls()
        {
            var entites = GameController.Entities;
            foreach (Entity e in entites)
            {
                if (e.Path.Contains("DelveWall"))
                    wall(e);
            }
        }

        /// <summary>
        /// Draw Direktion of breakable walls depending on Distance
        /// </summary>
        /// <param name="e"></param>
        public void wall(Entity e)
        {
            if (e.IsAlive)
            {

                Vector2 delta = e.GridPos - GameController.Player.GridPos;
                double phi;
                double distance = delta.GetPolarCoordinates(out phi);
                if (distance > Settings.DelveMaxRange) 
                    return;
                RectangleF Dir = MathHepler.GetDirectionsUV(phi, distance);
                RectangleF rect = GameController.Window.GetWindowRectangle();
                Vector2 center = new Vector2(rect.X + rect.Width / 2, rect.Height - 10);
                center = GameController.Game.IngameState.Camera.WorldToScreen(GameController.Player.Pos);
                RectangleF rectDirection = new RectangleF(center.X - 20, center.Y - 40, 40, 40);
                Graphics.DrawImage("directions.png", rectDirection, Dir, Color.LightGreen);
            }
        }

        #endregion

        #region Craftie stuff

        /// <summary>
        /// 
        /// </summary>
        private void ToggleCraftie()
        {
            if (!ingameUI.StashElement.IsVisible)
            {
                ResetAll("stash not open");
                return;
            }
            if (ingameUI.StashElement.VisibleStash.InvType != InventoryType.CurrencyStash)
            {
                ResetAll("Currency Stash not open");
                return;
            }
            //if (isCraftingWindowVisible) // Currently Crafting Window is viisible, so turn it off
            //{
            //    ResetAll("");
            //    return;

            //}
            doCraft = !doCraft;
            if (doCraft)
            {
                timer = DateTime.Now;
                lastState = 0;
            }
            
            //LogMessage($"Toggle Craftie. Current State = {doCraft.ToString()}", 1);
            //isCraftingWindowVisible = !isCraftingWindowVisible; // interactive Window doesnt work at all, so activating via hotkey
        }

        /// <summary>
        /// 
        /// </summary>
        private void Craftie ()
        {
            //LogMessage($"CraftieState = {doCraft.ToString()}", 1);
            //if (isCraftingWindowVisible)
            //{
            //    if (!ingameUI.StashElement.IsVisible)
            //    {
            //        ResetAll($"No Open Stash -> leaving ");
            //        return;
            //    }
            //    if (ingameUI.StashElement.VisibleStash.InvType != InventoryType.CurrencyStash)
            //    {
            //        ResetAll($"Crafing only in Curerncy Stash -> leaving");
            //        return;
            //    }

            if (!ingameUI.StashElement.IsVisible)
            {
                ResetAll("stash not open");
                return;
            }
            if (ingameUI.StashElement.VisibleStash.InvType != InventoryType.CurrencyStash)
            {
                ResetAll("Currency Stash not open");
                return;
            }
            NormalInventoryItem itemToCraft = CraftingItemFromCurrencyStash();
            LogMessage($"item to Craft : {GameController.Files.BaseItemTypes.Translate(itemToCraft.Item.Path).ClassName}", 1,Color.LightBlue);
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
            if (doCraft && (DateTime.Now - timer).TotalMilliseconds > Settings.ExtraDelayCraftie.Value)
            {
                CraftIt(itemToCraft);
            }

            //    else
            //        CraftWindow(itemToCraft);

            //}
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        private void ResetAll (string msg)
        {
            if (!string.IsNullOrEmpty(msg))
                LogMessage(msg, 1); //Show info
            isCraftingWindowVisible = false; // turn off crafting if Stash is changed!
            if (Input.IsKeyDown(System.Windows.Forms.Keys.LShiftKey) && doCraft) // release shift state 
                Input.KeyUp(System.Windows.Forms.Keys.LShiftKey);
            doCraft = false; // crafting inactive
            lastState = 0;
        }
        
        /// <summary>
        /// 
        /// </summary>
        private void ResetAll()
        {
            ResetAll("");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemToCraft"></param>
        private void CraftWindow(NormalInventoryItem itemToCraft)
        {
            
            //LogMessage("CraftWindow", 1);
            System.Numerics.Vector2 Pos = new System.Numerics.Vector2(ingameUI.StashElement.VisibleStash.Position.X + ingameUI.StashElement.VisibleStash.Width,     ingameUI.StashElement.VisibleStash.Position.Y);

            ImGui.Begin("Craftie");
            ImGui.SetNextWindowSizeConstraints(Pos, new System.Numerics.Vector2(300, 300));

            ImGui.TextColored(Color.Yellow.ToImguiVec4(), GameController.Files.BaseItemTypes.Translate(itemToCraft.Item.Path).ClassName);

            Settings.useScraps.Value = ImGuiExtension.Checkbox("Use Scraps", Settings.useScraps);
            Settings.useJewellers.Value = ImGuiExtension.Checkbox("Use Jewellers", Settings.useJewellers);
            Settings.minSlots.Value = ImGuiExtension.IntSlider("minimum Slots", Settings.minSlots.Value, 1, 6);
            Settings.useFusings.Value = ImGuiExtension.Checkbox("Use Fusings", Settings.useFusings);
            Settings.minLinks.Value = ImGuiExtension.IntSlider("minimum Links", Settings.minLinks.Value, 1, 6);

            
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemToCraft"></param>
        public void CraftIt(NormalInventoryItem itemToCraft)
        {
            //if (!BringToFront()) return;
            var x = itemToCraft;
            string orb = "";
            if (itemToCraft.Item.HasComponent<Quality>() && Settings.useScraps && itemToCraft.Item.GetComponent<Quality>().ItemQuality<20)
            {
                //Quality comp = itemToCraft.Item.GetComponent<Quality>();
                //if (comp.ItemQuality < 20)
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
            else if (itemToCraft.Item.HasComponent<Quality>() && Settings.useJewellers)
            {

            }
            else if(itemToCraft.Item.HasComponent<Quality>() && Settings.useFusings)
            {

            }
            else
            {
                ResetAll("Nothing to do");
                return;
            }

            OrbCrafting(orb,itemToCraft);
            timer = DateTime.Now;
        }


        /// <summary>
        /// Activates the Orb to craft and klicks on the Craftingitem;
        /// </summary>
        /// <param name="orb"></param>
        /// <param name="itemToCraft"></param>
        private void OrbCrafting(string orb, NormalInventoryItem itemToCraft)
        {
            if (orb != "")
            {
                NormalInventoryItem Currency = DoWeHaveCurrency(orb);
                if (Currency != null)
                {
                    switch (lastState)
                    {
                        case 0: // Rightklick crafting currency
                            Input.SetCursorPositionAndClick(Currency.GetClientRect().Center,MouseButtons.Right);
                            LogMessage("Craftit Laststate 0", 101);
                            lastState = 1;
                            //ResetAll(); // test : remove !
                            break;
                        case 1: // Press shift key
                            Input.KeyDown(System.Windows.Forms.Keys.LShiftKey); // Make sure shift is down for continous clicks
                            LogMessage("Craftit Laststate 1", 101);
                            lastState = 2;
                            break;
                        case 2: // Leftklick Crafting item
                            LogMessage("Craftit Laststate 2", 101);
                            Input.SetCursorPositionAndClick(itemToCraft.GetClientRect().Center);
                            //ResetAll(); // test : remove !
                            break;
                    }
                    //Thread.Sleep(Settings.ExtraDelayCraftie.Value);
                    lastCurrency = orb;
                }
                else
                    ResetAll($"Crafting Currency {orb} not found");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="itemToCraft"></param>
        /// <param name="currency"></param>
        private void CraftWithCurrency (NormalInventoryItem itemToCraft, NormalInventoryItem currency)
        {
            LogMessage($"Using {currency}", 1);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private bool IsCraftable(NormalInventoryItem item)
        {
            if (item.Item.GetComponent<Base>().isCorrupted)
            {
                LogMessage("Item is non-craftable (corrupted).", 5);
                return false;
            }
            Mods m = item.Item.GetComponent<Mods>();
            if (m==null)
            {
                LogMessage("Item Mods undetectable", 5,Color.Red);
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseName"></param>
        /// <returns></returns>
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

