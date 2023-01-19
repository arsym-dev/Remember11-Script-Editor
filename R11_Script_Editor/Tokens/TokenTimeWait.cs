using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenTimeWait : Token
    {
        public new const TokenType Type = TokenType.time_wait;
        
        public byte Unknown1 { get; set; }
        public UInt16 Value { get; set; }

        public TokenTimeWait(bool blank=false)
        {
            _command = "Time Wait";
            _description = "Wait for some time.\n60 Units = 1 second.";
            _length = 4;

            if (blank)
            {
                Unknown1 = 0;
                Value = 60;
            }
            else
            {
                Unknown1 = Tokenizer.ReadUInt8(1); //[00] or [01]
                Value = Tokenizer.ReadUInt16(2);
            }
            
            UpdateData();
        }


        public override void UpdateData()
        {
            Data = String.Format("{0:0.000} sec", Value / 60.0);
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("Unknown", "Unknown1");
            base.AddUint16("Delay", "Value");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Unknown1;
            BitConverter.GetBytes(Value).CopyTo(output, 2);

            return output;
        }
    }
}
