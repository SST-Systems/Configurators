using System;
using SST.StableRef;
using UnityEngine;
using UnityEngine.UI;

namespace SST.Configurators.Samples.InstructionsForButton
{
    /// <summary>
    /// Handler-based sync instruction (data half): sets a UGUI Text's color.
    /// Shows the sync variant of the data/handler split (no async, runs instantly).
    /// </summary>
    [Serializable]
    [StableRefCategory("UI/Text")]
    public class ChangeTextColor : InstructionData<ChangeTextHandler>
    {
        public Text Text;
        public Color Color;
    }

    /// <summary>Handler half for <see cref="ChangeTextColor"/>: applies the color to the target Text.</summary>
    public class ChangeTextHandler : InstructionHandler<ChangeTextColor>
    {
        public override void Apply()
        {
            Data.Text.color = Data.Color;
        }
    }
}