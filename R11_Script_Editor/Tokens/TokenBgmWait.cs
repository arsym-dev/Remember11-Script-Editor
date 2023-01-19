using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenBgmWait : Token
    {
        public new const TokenType Type = TokenType.bgm_wait;

        byte fixed1;

        public TokenBgmWait(bool blank=false)
        {
            _command = "BGM Wait";
            _description = "Wait for BGM to start playing";
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
