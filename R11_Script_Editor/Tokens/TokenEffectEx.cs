using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenEffectEx : Token
    {
        public new const TokenType Type = TokenType.effect_ex;

        public byte Unknown1 { get; set; }
        public byte Unknown2 { get; set; }
        public byte Unknown3 { get; set; }
        public UInt16 Unknown4 { get; set; }
        public UInt16 Unknown5 { get; set; }
        public UInt16 Unknown6 { get; set; }
        public UInt16 Unknown7 { get; set; }
        public UInt16 Unknown8 { get; set; }
        public UInt16 Unknown9 { get; set; }
        public UInt16 Unknown10 { get; set; }
        public UInt16 Unknown11 { get; set; }
        public UInt16 Unknown12 { get; set; }
        public UInt16 Unknown13 { get; set; }
        public UInt16 Unknown14 { get; set; }
        public UInt16 Unknown15 { get; set; }
        public UInt16 Unknown16 { get; set; }
        public UInt16 Unknown17 { get; set; }
        public UInt16 Unknown18 { get; set; }
        public UInt16 Unknown19 { get; set; }
        public UInt16 Unknown20 { get; set; }

        public TokenEffectEx(bool blank=false)
        {
            _command = "Effect Ex";
            _description = "[Undocumented]\nUnknown2 takes special values:" +
                "\n16 - Screen flashing (DMT effect)" +
                "\n18 - Ever17 perspective change effect" +
                "\n19 - Dark overlay" +
                "\n20 - Flashback desaturated overlay" +
                "\n21 - Gray oerlay? (Unknown 8)" +
                "\n22 - Flashback overlay (white vignette)" +
                "\n23 - Blizzard" +
                "\n24 - Blizzard" +
                "\n-----" +
                "\n252" + 
                "\n253 (apply effect?)"+
                "\n254" +
                "\n255" +
                "\n\nUnknown4: How long to keep the effect on (blizzard) (1 sec = 60, 0 keeps the effect permanently)"+
                "\n\nUnknown6: Opacity 0 to 100 (blizzard)" +
                "\n\nUnknown7: Snow speed (blizzard)";
            _length = 38;

            if (blank)
            {

            }
            else
            {
                Unknown1 = Tokenizer.ReadUInt8(1);
                Unknown2 = Tokenizer.ReadUInt8(2);
                Unknown3 = Tokenizer.ReadUInt8(3);
                Unknown4 = Tokenizer.ReadUInt16(4);
                Unknown5 = Tokenizer.ReadUInt16(6);
                Unknown6 = Tokenizer.ReadUInt16(8);
                Unknown7 = Tokenizer.ReadUInt16(10);
                Unknown8 = Tokenizer.ReadUInt16(12);
                Unknown9 = Tokenizer.ReadUInt16(14);
                Unknown10 = Tokenizer.ReadUInt16(16);
                Unknown11 = Tokenizer.ReadUInt16(18);
                Unknown12 = Tokenizer.ReadUInt16(20);
                Unknown13 = Tokenizer.ReadUInt16(22);
                Unknown14 = Tokenizer.ReadUInt16(24);
                Unknown15 = Tokenizer.ReadUInt16(26);
                Unknown16 = Tokenizer.ReadUInt16(28);
                Unknown17 = Tokenizer.ReadUInt16(30);
                Unknown18 = Tokenizer.ReadUInt16(32);
                Unknown19 = Tokenizer.ReadUInt16(34);
                Unknown20 = Tokenizer.ReadUInt16(36);
            }


            UpdateData();
        }
        
        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Unknown1;
            output[2] = (byte)Unknown2;
            output[3] = (byte)Unknown3;
            BitConverter.GetBytes(Unknown4).CopyTo(output, 4);
            BitConverter.GetBytes(Unknown5).CopyTo(output, 6);
            BitConverter.GetBytes(Unknown6).CopyTo(output, 8);
            BitConverter.GetBytes(Unknown7).CopyTo(output, 10);
            BitConverter.GetBytes(Unknown8).CopyTo(output, 12);
            BitConverter.GetBytes(Unknown9).CopyTo(output, 14);
            BitConverter.GetBytes(Unknown10).CopyTo(output, 16);
            BitConverter.GetBytes(Unknown11).CopyTo(output, 18);
            BitConverter.GetBytes(Unknown12).CopyTo(output, 20);
            BitConverter.GetBytes(Unknown13).CopyTo(output, 22);
            BitConverter.GetBytes(Unknown14).CopyTo(output, 24);
            BitConverter.GetBytes(Unknown15).CopyTo(output, 26);
            BitConverter.GetBytes(Unknown16).CopyTo(output, 28);
            BitConverter.GetBytes(Unknown17).CopyTo(output, 30);
            BitConverter.GetBytes(Unknown18).CopyTo(output, 32);
            BitConverter.GetBytes(Unknown19).CopyTo(output, 34);
            BitConverter.GetBytes(Unknown20).CopyTo(output, 36);

            return output;
        }


        public override void UpdateData()
        {
            Data = "[UNDOCUMENTED]";
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("Unknown1", "Unknown1");
            base.AddUint8("Unknown2", "Unknown2");
            base.AddUint8("Unknown3", "Unknown3");
            base.AddUint16("Unknown4", "Unknown4");
            base.AddUint16("Unknown5", "Unknown5");
            base.AddUint16("Unknown6", "Unknown6");
            base.AddUint16("Unknown7", "Unknown7");
            base.AddUint16("Unknown8", "Unknown8");
            base.AddUint16("Unknown9", "Unknown9");
            base.AddUint16("Unknown10", "Unknown10");
            base.AddUint16("Unknown11", "Unknown11");
            base.AddUint16("Unknown12", "Unknown12");
            base.AddUint16("Unknown13", "Unknown13");
            base.AddUint16("Unknown14", "Unknown14");
            base.AddUint16("Unknown15", "Unknown15");
            base.AddUint16("Unknown16", "Unknown16");
            base.AddUint16("Unknown17", "Unknown17");
            base.AddUint16("Unknown18", "Unknown18");
            base.AddUint16("Unknown19", "Unknown19");
            base.AddUint16("Unknown20", "Unknown20");
        }
    }
}
