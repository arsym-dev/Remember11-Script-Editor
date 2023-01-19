using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenTitleDisplay : Token
    {
        public new const TokenType Type = TokenType.title_disp;
        
        public byte Value { get; set; }

        public TokenTitleDisplay()
        {
            _command = "Title Display";
            _description = "Display title of scene";
            _length = 2;

            Value = Tokenizer.ReadUInt8(1);
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
