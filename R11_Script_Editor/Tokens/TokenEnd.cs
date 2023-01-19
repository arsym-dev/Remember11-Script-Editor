using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenEnd : Token
    {
        public new const TokenType Type = TokenType.end;

        byte fixed1;

        public TokenEnd(bool blank=false)
        {
            _command = "End";
            _description = "Ending sequence in case we hit an end";
            _length = 2;

            fixed1 = Tokenizer.ReadUInt8(1);
        }
        
        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)fixed1;

            return output;
        }
    }
}