using Druzil.Poe.Libs;
using ExileCore.PoEMemory.Elements.InventoryElements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllInOne
{
    /// <summary>
    /// Collectionobject for subsetSum Quality 40 !
    /// </summary>
    internal class QualityItem : setData
    {
        public NormalInventoryItem CheckItem { get; set; }
        public int Quality { get; set; }
        public QualityItem(NormalInventoryItem itm, int quality)
        {
            CheckItem = itm;
            Quality = quality;
        }

        public int getValue()
        {
            return Quality;
        }

        public override string ToString()
        {
            return CheckItem.Item.ToString();
        }

    }
}
