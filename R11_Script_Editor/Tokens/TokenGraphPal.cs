using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenGraphPal : Token
    {
        public new const TokenType Type = TokenType.graph_pal;
        
        public byte Unknown1 { get; set; }
        public UInt16 Unknown2 { get; set; }

        public TokenGraphPal()
        {
            _command = "Graph Pal";
            _description = "[Undocumented]";
            _length = 4;

            Unknown1 = Tokenizer.ReadUInt8(1);
            Unknown2 = Tokenizer.ReadUInt16(2);

            UpdateData();
        }
        
        public override void UpdateData()
        {
            Data = Unknown1.ToString() + "," + Unknown2.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("Unknown 1", "Unknown1");
            base.AddUint16("Unknown 2", "Unknown2");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Unknown1;
            BitConverter.GetBytes(Unknown2).CopyTo(output, 2);

            return output;
        }
    }
}
