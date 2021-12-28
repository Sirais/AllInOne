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

        [Menu("Hotkey for picking up gems")]
        public HotkeyNode HotKey { get; set; }


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

        [Menu("Enable Aura Activator ")]
        public ToggleNode EnableAura { get; set; }

        [Menu("Enable Golem Recaster ")]
        public ToggleNode EnableGolem { get; set; }

        [Menu("Enable ItemlevelFrame ")]
        public ToggleNode EnableILFrame { get; set; }

        [Menu("Enable ShowMySkellies ")]
        public ToggleNode EnableSMSkellies { get; set; }
        [Menu("Range check for Skellies")]
        public RangeNode<int> SkellieRange { get; set; }

        [Menu("Enable Craftie")]
        public ToggleNode EnableCraft { get; set; }

        [Menu("Extra Delay between Pickup Klicks Craftie")]
        public RangeNode<int> ExtraDelayCraftie { get; set; }


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
        HotKey = Keys.NumPad8;
            ExtraDelayQ40 = new RangeNode<int>(100, 1, 1000);
            ExtraDelayCraftie = new RangeNode<int>(100, 1, 1000);
            UseFlask = new ToggleNode(false);
            Enable = new ToggleNode(false);
            MaxGemQuality = new RangeNode<int>(18, 1, 19);
            MaxGemLevel = new RangeNode<int>(18, 1, 19);
        }
    }
}
