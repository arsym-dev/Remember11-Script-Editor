using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenFadeExWait : Token
    {
        public new const TokenType Type = TokenType.fade_ex_wait;
        
        byte fixed1;

        public TokenFadeExWait(bool blank=false)
        {
            _command = "Fade Ex Wait";
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
