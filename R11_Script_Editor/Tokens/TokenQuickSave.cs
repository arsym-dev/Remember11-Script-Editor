using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenQuickSave : Token
    {
        public new const TokenType Type = TokenType.quick_save;
        
        byte fixed1;

        public TokenQuickSave(bool blank=false)
        {
            _command = "Quick Save";
            _description = "Quick save, usually done at the start of a new scene.";
            _length = 2;

            fixed1 = 2;
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
