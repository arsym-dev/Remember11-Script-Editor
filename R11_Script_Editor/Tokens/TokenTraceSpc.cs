using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenTraceSpc : Token
    {
        public new const TokenType Type = TokenType.trace_spc;
        
        byte fixed1;

        public TokenTraceSpc()
        {
            _command = "Trace Spc";
            _description = "[Undocumented]";
            _length = 2;

            fixed1 = 1;
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
