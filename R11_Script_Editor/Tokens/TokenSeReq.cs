using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenSeReq : Token
    {
        public new const TokenType Type = TokenType.se_req;
        
        public byte Channel { get; set; }
        public UInt16 Unknown1 { get; set; }

        public TokenSeReq()
        {
            _command = "SE Req";
            _description = "[Undocumented]";
            _length = 4;

            Channel = Tokenizer.ReadUInt8(1);
            Unknown1 = Tokenizer.ReadUInt16(2);
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Channel.ToString() +", " + Unknown1.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("#", "Channel");
            base.AddUint16("Unknown", "Unknown1");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Channel;
            BitConverter.GetBytes(Unknown1).CopyTo(output, 2);

            return output;
        }
    }
}
