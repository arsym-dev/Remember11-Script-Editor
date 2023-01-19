using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenMovieStart : Token
    {
        public new const TokenType Type = TokenType.movie_start;
        

        public byte Unknown1 { get; set; }
        public UInt16 Unknown2 { get; set; }

        public TokenMovieStart()
        {
            _command = "Movie Start";
            _description = "[Undocumented] Use with movie_end";
            _length = 4;

            Unknown1 = Tokenizer.ReadUInt8(1); //Always [07]
            Unknown2 = Tokenizer.ReadUInt16(2); //Always [FF FF]
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Unknown1;
            BitConverter.GetBytes(Unknown2).CopyTo(output, 2);

            return output;
        }
    }
}
