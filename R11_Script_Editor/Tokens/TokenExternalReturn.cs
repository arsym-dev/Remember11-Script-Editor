using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenExternalReturn : Token
    {
        public new const TokenType Type = TokenType.ext_return;

        byte fixed1;

        public TokenExternalReturn()
        {
            _command = "Ext Return";
            _description = "[Undocumented]";
            _length = 2;

            fixed1 = 0;
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
