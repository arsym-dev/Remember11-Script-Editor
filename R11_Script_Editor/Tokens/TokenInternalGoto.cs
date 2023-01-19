using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenInternalGoto : Token
    {
        public new const TokenType Type = TokenType.int_goto;
        
        byte fixed1;
        UInt16 address;
        public string LabelJump { get; set; }

        public TokenInternalGoto(bool blank = false)
        {
            _command = "Int Goto";
            _description = "Jump to instruction in current script";
            _length = 4;

            fixed1 = 0;
            if (blank)
            {
                address = 0;
                LabelJump = "";
            }
            else
            {
                address = Tokenizer.ReadUInt16(2);
                LabelJump = address.ToString();
            }
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = "Jump to " + LabelJump;
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddTextbox("Jump Label", "LabelJump");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)fixed1;
            BitConverter.GetBytes(address).CopyTo(output, 2);

            return output;
        }

        public void UpdateAddress(UInt16 addr)
        {
            address = addr;
        }
    }
}
