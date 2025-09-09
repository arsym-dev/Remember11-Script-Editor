using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenSeVolume : Token
    {
        public new const TokenType Type = TokenType.se_vol;
        
        public byte Channel { get; set; }
        public UInt16 Volume { get; set; }

        public TokenSeVolume(bool blank = false)
        {
            _command = "SE Volume";
            _description = "Set sound effect volume";
            _length = 4;

            Channel = Tokenizer.ReadUInt8(1);
            Volume = Tokenizer.ReadUInt16(2);
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Channel.ToString() + ", Volume: " + Volume.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("#", "Channel");
            base.AddUint16("Volume", "Volume");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Channel;
            BitConverter.GetBytes(Volume).CopyTo(output, 2);

            return output;
        }
    }
}
