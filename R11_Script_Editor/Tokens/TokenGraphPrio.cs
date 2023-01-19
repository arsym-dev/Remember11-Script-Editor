using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenGraphPrio : Token
    {
        public new const TokenType Type = TokenType.graph_prio;
        
        public byte ImageNumber { get; set; }
        public UInt16 Priority { get; set; }

        public TokenGraphPrio()
        {
            _command = "Graph Prio";
            _description = "Set the z-level of an image";
            _length = 4;

            ImageNumber = Tokenizer.ReadUInt8(1);
            Priority = Tokenizer.ReadUInt16(2);
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = "#: " + ImageNumber.ToString() + ", Priority: " + Priority.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("Image Number", "ImageNumber");
            base.AddUint16("Priority", "Priority");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)ImageNumber;
            BitConverter.GetBytes(Priority).CopyTo(output, 2);

            return output;
        }
    }
}
