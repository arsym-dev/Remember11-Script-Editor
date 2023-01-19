using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenFadeExStart : Token
    {
        public new const TokenType Type = TokenType.fade_ex_start;
        
        public byte Transition { get; set; }
        public UInt16 Duration { get; set; }
        public UInt16 Unknown3 { get; set; }

        public TokenFadeExStart(bool blank=false)
        {
            _command = "Fade Ex Start";
            _description = "[Undocumented] 60 ticks = 1 second\n\nTransition:\n1 - Circle\n2 - Rectangle\n4 - Blinds\n8 - Blur\n127 - Fade In\n128 - Fade Out\n129 - Invert Circle\n130 - Invert Rectangle\n132 - Invert Blinds\n144 - Zoom in";
            _length = 6;

            if (blank)
            {
                Transition = 144;
                Duration = 30;
                Unknown3 = 4096;
            }
            else
            {
                Transition = Tokenizer.ReadUInt8(1);
                Duration = Tokenizer.ReadUInt16(2);
                Unknown3 = Tokenizer.ReadUInt16(4); //4095=white, 4096=black
            }

            UpdateData();
        }

        public override void UpdateData()
        {
            Data = String.Format("{1:0.000} sec, Unknown1: {0}, Unknown3: {2}",
                Transition,
                Duration / 60.0,
                Unknown3
                );
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("Transition", "Transition");
            base.AddUint16("Duration", "Duration");
            base.AddUint16("Unknown3", "Unknown3");
        }
        
        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Transition;
            BitConverter.GetBytes(Duration).CopyTo(output, 2);
            BitConverter.GetBytes(Unknown3).CopyTo(output, 4);

            return output;
        }
    }
}
