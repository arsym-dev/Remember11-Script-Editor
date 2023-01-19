using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenSeDel : Token
    {
        public new const TokenType Type = TokenType.se_del;
        
        public byte Channel { get; set; }

        public TokenSeDel()
        {
            _command = "SE Del";
            _description = "Stop sound from playing";
            _length = 2;

            Channel = Tokenizer.ReadUInt8(1);
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Channel.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("#", "Channel");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Channel;

            return output;
        }
    }
}
