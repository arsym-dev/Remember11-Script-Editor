using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenExternalGoto : Token
    {
        public new const TokenType Type = TokenType.ext_goto;

        byte fixed1;

        public TokenExternalGoto(bool blank=false)
        {
            _command = "Ext Goto";
            _description = "Jump to an instruction outside of the current script";
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
