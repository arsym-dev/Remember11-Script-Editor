using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenBgmVolume : Token
    {
        public new const TokenType Type = TokenType.bgm_vol;
        
        public byte Volume { get; set; }

        public TokenBgmVolume()
        {
            _command = "BGM Volume";
            _description = "Set BGM volume";
            _length = 2;

            Volume = Tokenizer.ReadUInt8(1);
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Volume.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("Volume", "Volume");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Volume;

            return output;
        }
    }
}
