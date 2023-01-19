using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenCountWait : Token
    {
        public new const TokenType Type = TokenType.count_wait;

        byte fixed1;
        public UInt16 Value { get; set; }

        public TokenCountWait()
        {
            _command = "Count Wait";
            _description = "Used in that one force scroll section at the end of Kokoro's route.\n60 units = 1 second";
            _length = 4;

            fixed1 = 0;
            Value = Tokenizer.ReadUInt16(2); //Every 60 values = 1 second

            UpdateData();
        }


        public override void UpdateData()
        {
            Data = String.Format("{0:0.000} sec", Value / 60.0);
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint16("Delay", "Value");
        }
        
        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)fixed1;
            BitConverter.GetBytes(Value).CopyTo(output, 2);

            return output;
        }
    }
}
