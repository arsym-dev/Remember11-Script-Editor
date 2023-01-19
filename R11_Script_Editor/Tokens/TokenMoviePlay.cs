using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenMoviePlay : Token
    {
        public new const TokenType Type = TokenType.movie_play;
        
        public byte Value { get; set; }

        public TokenMoviePlay()
        {
            _command = "Movie Play";
            _description = "Play specified movie";
            _length = 2;

            Value = Tokenizer.ReadUInt8(1);
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Value.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("Movie Number", "Value");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Value;

            return output;
        }
    }
}
