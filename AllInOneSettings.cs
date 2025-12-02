using ExileCore.Shared.Attributes;
using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;
using SharpDX;
using System.Windows.Forms;

namespace AllInOne
{
    internal class AllInOneSettings : ISettings
    {
        [Menu("Enable Plugin ")]
        public ToggleNode Enable { get; set; }

        public HotkeyNode Key01 { get; set; }
        public HotkeyNode Key02 { get; set; }
        public HotkeyNode Key03 { get; set; }
        public HotkeyNode Key04 { get; set; }
        public HotkeyNode Key05 { get; set; }
        public HotkeyNode Key06 { get; set; }
        public HotkeyNode Key07 { get; set; }
        public HotkeyNode Key08 { get; set; }
        public HotkeyNode Key09 { get; set; }
        public HotkeyNode Key10 { get; set; }



        // Q40 Stuff
        [Menu("Hotkey for activation")]
        public HotkeyNode Q40HotKey { get; set; }

        [Menu("Enable Q40Picks ")]
        public ToggleNode EnableQ40 { get; set; }

        [Menu("Maximum Quality to Sell")]
        public RangeNode<int> MaxGemQuality { get; set; }

        [Menu("Maximum Level to Sell")]
        public RangeNode<int> MaxGemLevel { get; set; }

        [Menu("Use Flasks instead of Gems")]
        public ToggleNode UseFlask { get; set; }

        [Menu("Extra Delay between Pickup Klicks Q40 Picker")]
        public RangeNode<int> ExtraDelayQ40 { get; set; }

        // Aura Enabler  

        [Menu("Enable Aura Activator ")]
        public ToggleNode EnableAura { get; set; }


        // Golem recaster
        [Menu("Enable Golem Recaster ")]
        public ToggleNode EnableGolem { get; set; }


        // Resonator  Splitter
        [Menu("Hotkey for activation")]
        public HotkeyNode ResoHotKey { get; set; }
        [Menu("Enable Resonator Split ")]
        public ToggleNode EnableResoSplit { get; set; }


        /// <summary>
        ///  Itemlevel Frame
        /// </summary>
        [Menu("Enable ItemlevelFrame ")]
        public ToggleNode EnableILFrame { get; set; }


        // Delve Walls
        [Menu("Enable Delve Walls")]
        public ToggleNode EnableDelve { get; set; }

        [Menu("Maximum Range")]
        public RangeNode<int> DelveMaxRange { get; set; }

        [Menu("Show Grid")]
        public ToggleNode DelveShowGrid { get; set; }


        // Crafter 
        [Menu("Hotkey for activation")]
        public HotkeyNode CraftHotKey { get; set; }
        [Menu("Enable Craftie")]
        public ToggleNode EnableCraft { get; set; }

        public ToggleNode useScraps { get; set; }

        public ToggleNode useJewellers { get; set; }
        public RangeNode<int> minSlots { get; set; }

        public ToggleNode useFusings { get; set; }
        public RangeNode<int> minLinks { get; set; }

        [Menu("Extra Delay between Pickup Klicks Craftie")]
        public RangeNode<int> ExtraDelayCraftie { get; set; }

        // Gem leveling
        [Menu("Enable Gem leveling")]
        public ToggleNode EnableGemLeveling { get; set; }

        public ToggleNode GLMobsNear { get; set; }

        public RangeNode<int> GLMobRange { get; set; }

        public ToggleNode EnableCraftInfo { get; set; }


        public AllInOneSettings()
        {
            Key01 = Keys.Q;
            Key02 = Keys.W;
            Key03 = Keys.E;
            Key04 = Keys.R;
            Key05 = Keys.T;
            Key06 = Keys.Control | Keys.Q;
            Key07 = Keys.Control | Keys.W;
            Key08 = Keys.Control | Keys.E;
            Key09 = Keys.Control | Keys.R;
            Key10 = Keys.Control | Keys.T;
            //plugin 

            Enable = new ToggleNode(false);
            // Q40 Stuff
            Q40HotKey = Keys.NumPad1;
            ExtraDelayQ40 = new RangeNode<int>(100, 1, 1000);
            MaxGemQuality = new RangeNode<int>(18, 1, 19);
            MaxGemLevel = new RangeNode<int>(18, 1, 19);
            UseFlask = new ToggleNode(false);

            //Delvewall Stuff
            EnableDelve = new ToggleNode(false);
            DelveMaxRange = new RangeNode<int>(300, 1, 1000);
            DelveShowGrid = new ToggleNode(false);

            // Craftie Stuff
            CraftHotKey = Keys.NumPad2;
            ExtraDelayCraftie = new RangeNode<int>(100, 1, 1000);
            minSlots = new RangeNode<int>(5, 1, 6);
            minLinks = new RangeNode<int>(5, 1, 6);

            // Resonator splitter
            ResoHotKey = Keys.NumPad3;

            // Auto Gem Leveling
            EnableGemLeveling = new ToggleNode(true);
            GLMobsNear = new ToggleNode(false); 
            GLMobRange = new RangeNode<int>(100, 1, 400);

            EnableCraftInfo = new ToggleNode(false);
        }
    }
}
