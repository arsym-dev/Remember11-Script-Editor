using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenClockDisp : Token
    {
        public new const TokenType Type = TokenType.clock_disp;

        byte fixed1;
        public UInt16 Hours { get; set; }
        public UInt16 Minutes { get; set; }

        public TokenClockDisp()
        {
            _command = "Clock Display";
            _description = "Shows the current time in hours and minutes.";
            _length = 6;

            fixed1 = 0;
            Hours = Tokenizer.ReadUInt16(2);
            Minutes = Tokenizer.ReadUInt16(4);

            UpdateData();
        }


        public override void UpdateData()
        {
            Data = Hours.ToString().PadLeft(2) +":"+ Minutes.ToString().PadLeft(2);
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint16("Hours", "Hours");
            base.AddUint16("Minutes", "Minutes");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)fixed1;
            BitConverter.GetBytes(Hours).CopyTo(output, 2);
            BitConverter.GetBytes(Minutes).CopyTo(output, 4);

            return output;
        }
    }
}
