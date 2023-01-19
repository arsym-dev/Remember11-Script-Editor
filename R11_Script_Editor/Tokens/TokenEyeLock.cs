using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenEyeLock : Token
    {
        public new const TokenType Type = TokenType.eye_lock;

        public EyeLock Value { get; set; }

        public TokenEyeLock()
        {
            _command = "Eye Lock";
            _description = "[Undocumented]";
            _length = 2;

            Value = (EyeLock) Tokenizer.ReadUInt8(1); if (!Enum.IsDefined(typeof(EyeLock), Value)) throw new ArgumentOutOfRangeException();

            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Value.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddCombobox<EyeLock>("Status", "Value");
        }
        
        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Value;

            return output;
        }
    }

    public enum EyeLock
    {
        Unlocked = 0,
        Locked = 1
    }
}
