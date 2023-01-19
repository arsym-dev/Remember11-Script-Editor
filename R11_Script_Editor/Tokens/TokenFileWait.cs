using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenFileWait : Token
    {
        public new const TokenType Type = TokenType.file_wait;
        
        byte fixed1;

        public TokenFileWait(bool blank=false)
        {
            _command = "File Wait";
            _description = "Wait for file to load";
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
