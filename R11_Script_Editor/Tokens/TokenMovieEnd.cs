using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenMovieEnd : Token
    {
        public new const TokenType Type = TokenType.movie_end;
        
        byte fixed1;

        public TokenMovieEnd()
        {
            _command = "Movie End";
            _description = "Wait till end of movie";
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
