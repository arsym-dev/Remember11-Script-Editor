using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenBgmSet : Token
    {
        public new const TokenType Type = TokenType.bgm_set;
        
        byte fixed1;
        UInt16 fixed2;

        public TokenBgmSet(bool blank = false)
        {
            _command = "BGM Set";
            _description = "[Undocumented]";
            _length = 4;

            fixed1 = 0;
            fixed2 = 0x0239;
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)fixed1;
            BitConverter.GetBytes(fixed2).CopyTo(output, 2);

            return output;
        }
    }
}
