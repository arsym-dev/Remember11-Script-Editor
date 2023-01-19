using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenKeyLock : Token
    {
        public new const TokenType Type = TokenType.key_lock;
        
        public KeyLock Value { get; set; }

        public TokenKeyLock()
        {
            _command = "Key Lock";
            _description = "Prevent user from inputing key commands";
            _length = 2;

            Value = (KeyLock) Tokenizer.ReadUInt8(1); if (!Enum.IsDefined(typeof(KeyLock), Value)) throw new ArgumentOutOfRangeException();
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Value.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddCombobox<KeyLock>("Status", "Value");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Value;

            return output;
        }
    }

    public enum KeyLock
    {
        Unlocked = 0,
        Locked = 1
    }
}
