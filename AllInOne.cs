using System;
using System.Collections.Generic;
using SharpDX;
using System.Threading;
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



namespace AllInOne
{
    internal class AllInOne : BaseSettingsPlugin<AllInOneSettings>
    {

        private IngameUIElements ingameUI;


        public override void OnLoad()
        {
        }

        public override bool Initialise()
        {
            ingameUI = GameController.IngameState.IngameUi;
            return true;
        }

        public override void AreaChange(AreaInstance area) => ingameUI = GameController.IngameState.IngameUi;



        public override void Render()
        {
            base.Render();
            if (!Settings.Enable.Value) // Plugin enabled = 
                return; // no, do nothing
            if (Settings.EnableILFrame)
                MarkILvl();
            if (Settings.EnableQ40)
                Q40Pick();
            if (Settings.EnableAura) 
                ;
            if (Settings.EnableGolem) 
                ;



        }


        #region Q40 Pick Stuff
        private void Q40Pick()
        {
            if (!KeyboardHelper.IsKeyToggled(Settings.Hotkey.Value)) // Hotkey Pressed ? 
                return; // No key pressed just leave

            LogMessage($"AllInOne: Hotkey ({Settings.Hotkey.Value.ToString()}) toggled, Running Q40 Picker", 1);

            if (!GameController.Game.IngameState.IngameUi.StashElement.IsVisible)
            {
                LogMessage($"No Open Stash -> leaving ", 1);
                KeyboardHelper.KeyPress(Settings.Hotkey.Value);
                return;
            }
            List<setData> hits;
            hits = getQualityType("Skill Gem"); // Try to find gems
            if (hits == null || hits.Count == 0)
                hits = getQualityType("Flask"); //No gems so try flasks
            if (hits == null || hits.Count == 0)
            {
                LogMessage("No Quality Items found ", 1);
                KeyboardHelper.KeyPress(Settings.Hotkey.Value);
                return;
            }
            LogMessage($"AllInOne: Q40 found  {hits.Count} Quality Items in open stash.", 1);

            SetFinder Sets = new SetFinder(hits, 40);

            if (Sets.BestSet == null)
            {
                LogMessage("Added Quality is not 40", 1);
                KeyboardHelper.KeyPress(Settings.Hotkey.Value);
                return;
            }

            pickup(Sets);

            KeyboardHelper.KeyPress(Settings.Hotkey.Value); // send the hotkey back to the system to turn off the Work
        }

        /// <summary>
        /// Displays found set as  Logmessage. Just for debugging 
        /// </summary>
        /// <param name="Sets"></param>
        private void displaySet(SetFinder Sets)
        {
            LogMessage($"V5 :found set for Q40 contains {Sets.BestSet.Values.Count} Gems", 10);

            int i = 1;
            foreach (QualityGem g in Sets.BestSet.Values)
            {
                LogMessage($"{i} - Q{g.getValue()} X{g.Gem.InventPosX} - Y{g.Gem.InventPosY}", 10);
                i++;
            }
        }

        /// <summary>
        /// Pickup found Items into main inventory 
        /// </summary>
        /// <param name="Sets"></param>
        private void pickup(SetFinder Sets)
        {
            foreach (QualityGem g in Sets.BestSet.Values)
            {
                RectangleF itmPos = g.Gem.GetClientRect();
                KeyboardHelper.KeyDown(System.Windows.Forms.Keys.LControlKey);
                Thread.Sleep(100);//((int)GameController.Game.IngameState.CurLatency);
                Mouse.SetCursorPosAndLeftClick(RandomizedCenterPoint(itmPos), GameController.Window.GetWindowRectangle().TopLeft);
                Thread.Sleep(100);//((int)GameController.Game.IngameState.CurLatency);
                KeyboardHelper.KeyUp(System.Windows.Forms.Keys.LControlKey);
                Thread.Sleep(100);//((int)GameController.Game.IngameState.CurLatency);
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
            var stashPanel = GameController.Game.IngameState.IngameUi.StashElement;
            if (!stashPanel.IsVisible)
                return null;
            var visibleStash = stashPanel.VisibleStash;
            if (visibleStash == null)
                return null;
            IList<NormalInventoryItem> inventoryItems = ingameUI.StashElement.VisibleStash.VisibleInventoryItems;
            foreach (NormalInventoryItem item in inventoryItems)
            {
                BaseItemType baseItemType = GameController.Files.BaseItemTypes.Translate(item.Item.Path);

                if (baseItemType.ClassName.Contains(itemtype))
                {
                    int Quality = item.Item.GetComponent<Quality>().ItemQuality;
                    if (Quality > 0)
                        if (Quality <= Settings.MaxGemQuality)
                            res.Add(new QualityGem(item, Quality));
                }
            }
            return res;
        }

        #endregion


        #region Mrk Ilvl 
        private void MarkILvl ()
        {
            var stashPanel = GameController.Game.IngameState.IngameUi.StashElement;
            if (!stashPanel.IsVisible)
                return;
            var visibleStash = stashPanel.VisibleStash;
            if (visibleStash == null)
                return;
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

    /// <summary>
    /// Collectionobject for subsetSum Quality 40 !
    /// </summary>
    public class QualityGem : setData
    {
        public NormalInventoryItem Gem { get; set; }
        public int Quality { get; set; }
        public QualityGem (NormalInventoryItem gem,int quality)
        {
            Gem = gem;
            Quality = quality;
        }

        public int getValue()
        {
            return Quality; // Gem.GetComponent<Quality>().ItemQuality;
        }

        public override string ToString()
        {
            return Gem.Item.ToString();
        }

    }




}

