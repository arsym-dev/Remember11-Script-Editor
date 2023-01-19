using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenGraphPosAuto : Token
    {
        public new const TokenType Type = TokenType.graph_pos_auto;
        
        public byte Unknown1 { get; set; }
        public UInt16 Unknown2 { get; set; }
        public UInt16 Unknown3 { get; set; }
        public UInt16 Unknown4 { get; set; }
        public UInt16 Unknown5 { get; set; }
        public UInt16 Unknown6 { get; set; }

        public TokenGraphPosAuto()
        {
            _command = "Graph Pos Auto";
            _description = "[Undocumented]";
            _length = 12;

            Unknown1 = Tokenizer.ReadUInt8(1);
            Unknown2 = Tokenizer.ReadUInt16(2);
            Unknown3 = Tokenizer.ReadUInt16(4);
            Unknown4 = Tokenizer.ReadUInt16(6);
            Unknown5 = Tokenizer.ReadUInt16(8);
            Unknown6 = Tokenizer.ReadUInt16(10);
            
            UpdateData();
        }
        
        public override void UpdateData()
        {
            Data = "[UNDOCUMENTED]";
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("Unknown 1", "Unknown1");
            base.AddUint16("Unknown2", "Unknown2");
            base.AddUint16("Unknown3", "Unknown3");
            base.AddUint16("Unknown4", "Unknown4");
            base.AddUint16("Unknown5", "Unknown5");
            base.AddUint16("Unknown6", "Unknown6");
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

            return output;
        }
    }
}
