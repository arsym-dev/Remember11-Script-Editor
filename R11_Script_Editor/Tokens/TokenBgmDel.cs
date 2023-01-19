using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace R11_Script_Editor.Tokens
{
    class TokenBgmDel : Token
    {
        public new const TokenType Type = TokenType.bgm_del;

        byte fixed1;

        public TokenBgmDel(bool blank=false)
        {
            _command = "BGM Delete";
            _description = "Stop current track from playing";
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
