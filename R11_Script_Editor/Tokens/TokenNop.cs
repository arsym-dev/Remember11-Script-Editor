using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenNop : Token
    {
        public new const TokenType Type = TokenType.nop;
        
        byte fixed1;
        public TokenNop()
        {
            _command = "NOP";
            _description = "Mark the end of a script";
            _length = 2;

            fixed1 = 0;

            Data = "END OF SCRIPT";
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
