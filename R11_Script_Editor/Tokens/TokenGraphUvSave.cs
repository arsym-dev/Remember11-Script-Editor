using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenGraphUvSave : Token
    {
        public new const TokenType Type = TokenType.graph_uv_save;
        
        public byte Value { get; set; }

        public TokenGraphUvSave(bool blank = false)
        {
            _command = "Graph UV Save";
            _description = "[Undocumented]";
            _length = 2;

            Value = Tokenizer.ReadUInt8(1);
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Value.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("Value", "Value");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Value;

            return output;
        }
    }
}
