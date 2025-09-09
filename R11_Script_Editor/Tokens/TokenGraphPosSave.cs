using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenGraphPosSave : Token
    {
        public new const TokenType Type = TokenType.graph_pos_save;
        
        public byte ImageNumber { get; set; }

        public TokenGraphPosSave(bool blank = false)
        {
            _command = "Graph Pos Save";
            _description = "[Undocumented]";
            _length = 2;

            ImageNumber = Tokenizer.ReadUInt8(1);
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = ImageNumber.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("Image Number", "ImageNumber");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)ImageNumber;

            return output;
        }
    }
}
