using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenSeSpeed : Token
    {
        public new const TokenType Type = TokenType.se_speed;
        
        public byte Channel { get; set; }
        public UInt16 Speed { get; set; }

        public TokenSeSpeed()
        {
            _command = "SE Speed";
            _description = "Set sound effect speed";
            _length = 4;

            Channel = Tokenizer.ReadUInt8(1);
            Speed = Tokenizer.ReadUInt16(2);
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Channel.ToString() + ", Speed: " + Speed.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("#", "Channel");
            base.AddUint16("Speed", "Speed");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Channel;
            BitConverter.GetBytes(Speed).CopyTo(output, 2);

            return output;
        }
    }
}
