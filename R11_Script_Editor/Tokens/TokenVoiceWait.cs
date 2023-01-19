using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenVoiceWait : Token
    {
        public new const TokenType Type = TokenType.voice_wait;
        
        byte fixed1;

        public TokenVoiceWait()
        {
            _command = "Voice Wait";
            _description = "Wait for voice line to finish playing?";
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
