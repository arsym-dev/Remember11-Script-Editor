using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenGraphScaleAuto : Token
    {
        public new const TokenType Type = TokenType.graph_scale_auto;
        
        public byte Unknown1 { get; set; }
        public UInt16 Unknown2 { get; set; }
        public UInt16 Unknown3 { get; set; }
        public UInt16 Unknown4 { get; set; }
        public UInt16 Unknown5 { get; set; }
        public UInt16 Unknown6 { get; set; }
        public UInt16 Unknown7 { get; set; }
        public UInt16 Unknown8 { get; set; }

        public TokenGraphScaleAuto(bool blank = false)
        {
            _command = "Graph Scale Auto";
            _description = "[Undocumented]";
            _length = 16;

            Unknown1 = Tokenizer.ReadUInt8(1);
            Unknown2 = Tokenizer.ReadUInt16(2);
            Unknown3 = Tokenizer.ReadUInt16(4);
            Unknown4 = Tokenizer.ReadUInt16(6);
            Unknown5 = Tokenizer.ReadUInt16(8);
            Unknown6 = Tokenizer.ReadUInt16(10);
            Unknown7 = Tokenizer.ReadUInt16(12);
            Unknown8 = Tokenizer.ReadUInt16(14);
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = "[UPDATE ME]";
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("Unknown 1", "Unknown1");
            base.AddUint16("Unknown 2", "Unknown2");
            base.AddUint16("Unknown 3", "Unknown3");
            base.AddUint16("Unknown 4", "Unknown4");
            base.AddUint16("Unknown 5", "Unknown5");
            base.AddUint16("Unknown 6", "Unknown6");
            base.AddUint16("Unknown 7", "Unknown7");
            base.AddUint16("Unknown 8", "Unknown8");
        }
        
        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Unknown1;
            BitConverter.GetBytes(Unknown2).CopyTo(output, 2);
            BitConverter.GetBytes(Unknown3).CopyTo(output, 4);
            BitConverter.GetBytes(Unknown4).CopyTo(output, 6);
            BitConverter.GetBytes(Unknown5).CopyTo(output, 8);
            BitConverter.GetBytes(Unknown6).CopyTo(output, 10);
            BitConverter.GetBytes(Unknown7).CopyTo(output, 12);
            BitConverter.GetBytes(Unknown8).CopyTo(output, 14);

            return output;
        }
    }
}
