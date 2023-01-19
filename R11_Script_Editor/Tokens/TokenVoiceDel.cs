using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenVoiceDel : Token
    {
        public new const TokenType Type = TokenType.voice_del;
        
        byte fixed1;

        public TokenVoiceDel()
        {
            _command = "Voice Del";
            _description = "Stop voice line from playing?";
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
