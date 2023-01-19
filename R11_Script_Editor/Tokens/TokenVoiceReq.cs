using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenVoiceReq : Token
    {
        public new const TokenType Type = TokenType.voice_req;
        
        public byte Value { get; set; }

        public TokenVoiceReq()
        {
            _command = "Voice Req";
            _description = "Request voice line?";
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
            base.AddUint8("Channel?", "Value");
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
