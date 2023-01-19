using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace R11_Script_Editor.Tokens
{
    class TokenMsgType : Token
    {
        public new const TokenType Type = TokenType.msg_type;
        
        public MsgboxType Value { get; set; }

        public TokenMsgType(bool blank=false)
        {
            _command = "Msg Type";
            _description = "Change textbox trim color";
            _length = 2;

            if (blank)
            {
                Value = MsgboxType.Kokoro;
            }
            else
            {
                Value = (MsgboxType)Tokenizer.ReadUInt8(1); if (!Enum.IsDefined(typeof(MsgboxType), Value)) throw new ArgumentOutOfRangeException();
            }
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Value.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddCombobox<MsgboxType>("Msg Type", "Value");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Value;

            return output;
        }

        public enum MsgboxType
        {
            Self,
            Kokoro,
            Satoru
        }
    }
}
