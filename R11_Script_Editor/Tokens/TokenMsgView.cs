using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenMsgView : Token
    {
        public new const TokenType Type = TokenType.msg_view;
        
        byte fixed1;
        int fixed2;

        public TokenMsgView(bool blank=false)
        {
            _command = "Msg View";
            _description = "Hide textbox";
            _length = 6;

            fixed1 = Tokenizer.ReadUInt8(1); // [03]
            fixed2 = Tokenizer.ReadUInt32(2);// [19 00 1A 00] 0x19 is actually msg_wait, but this value of delay is always used
            
            Data = "HIDE TEXTBOX";
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
