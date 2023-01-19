using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenSeWait : Token
    {
        public new const TokenType Type = TokenType.se_wait;
        
        public byte Channel { get; set; }
        public UInt16 Delay { get; set; }

        public TokenSeWait()
        {
            _command = "SE Wait";
            _description = "Wait for sound effect";
            _length = 4;

            Channel = Tokenizer.ReadUInt8(1);
            Delay = Tokenizer.ReadUInt16(2);
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Channel.ToString() + ", Delay: " + Delay.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("#", "Channel");
            base.AddUint16("Delay", "Delay");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Channel;
            BitConverter.GetBytes(Delay).CopyTo(output, 2);

            return output;
        }
    }
}
