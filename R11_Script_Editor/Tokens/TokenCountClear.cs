using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenCountClear : Token
    {
        public new const TokenType Type = TokenType.count_clear;

        byte fixed1;

        public TokenCountClear()
        {
            _command = "Count Clear";
            _description = "Used in that one forced scroll section at the end of Kokoro's route";
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
