using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenVibrationEx : Token
    {
        public new const TokenType Type = TokenType.vib_ex;
        
        public byte Unknown1 { get; set; }
        public byte Unknown2 { get; set; }
        public byte Unknown3 { get; set; }
        public byte Unknown4 { get; set; }
        public byte Unknown5 { get; set; }

        public TokenVibrationEx()
        {
            _command = "Vibration Ex";
            _description = "Vibrate the controller (on PS2)";
            _length = 6;

            Unknown1 = Tokenizer.ReadUInt8(1);
            Unknown2 = Tokenizer.ReadUInt8(2);
            Unknown3 = Tokenizer.ReadUInt8(3);
            Unknown4 = Tokenizer.ReadUInt8(4);
            Unknown5 = Tokenizer.ReadUInt8(5);

            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Unknown1.ToString() + ", " + Unknown2.ToString() + ", " + Unknown3.ToString() + ", " + Unknown4.ToString() + ", " + Unknown5.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("Unknown 1", "Unknown1");
            base.AddUint8("Unknown 2", "Unknown2");
            base.AddUint8("Unknown 3", "Unknown3");
            base.AddUint8("Unknown 4", "Unknown4");
            base.AddUint8("Unknown 5", "Unknown5");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Unknown1;
            output[2] = (byte)Unknown2;
            output[3] = (byte)Unknown3;
            output[4] = (byte)Unknown4;
            output[5] = (byte)Unknown5;

            return output;
        }
    }
}
