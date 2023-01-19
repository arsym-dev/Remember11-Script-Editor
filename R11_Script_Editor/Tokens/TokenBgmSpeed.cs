using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenBgmSpeed : Token
    {
        public new const TokenType Type = TokenType.bgm_speed;

        byte fixed1;
        public UInt16 Unknown1 { get; set; }
        public UInt16 Unknown2 { get; set; }

        public TokenBgmSpeed(bool blank=false)
        {
            _command = "BGM Speed";
            _description = "[Undocumented]";
            _length = 6;

            fixed1 = 0;

            if (blank)
            {
                Unknown1 = 128;
                Unknown2 = 825;
            }
            else
            {
                Unknown1 = Tokenizer.ReadUInt16(2); //[01 00], [10 00], [40 00] or [80 00]
                Unknown2 = Tokenizer.ReadUInt16(4); //Always [3c 80] to start, then becomes [39 03]. No idea why.
            }
            
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Unknown1.ToString() + ", " + Unknown2.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint16("Unknown 1", "Unknown1");
            base.AddUint16("Unknown 2", "Unknown2");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)fixed1;
            BitConverter.GetBytes(Unknown1).CopyTo(output, 2);
            BitConverter.GetBytes(Unknown2).CopyTo(output, 4);

            return output;
        }
    }
}
