using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenMsgMode : Token
    {
        public new const TokenType Type = TokenType.msg_mode;

        public MsgMode Value { get; set; }

        public TokenMsgMode()
        {
            _command = "Msg Mode";
            _description = "Change between ADV and NVL";
            _length = 2;

            Value = (MsgMode) Tokenizer.ReadUInt8(1); if (!Enum.IsDefined(typeof(MsgMode), Value)) throw new ArgumentOutOfRangeException();
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Value.ToString() + " mode";
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddCombobox<MsgMode>("Mode", "Value");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Value;

            return output;
        }
    }

    public enum MsgMode
    {
        ADV,
        NVL
    }
}
