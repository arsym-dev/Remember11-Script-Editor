using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenSkipLock : Token
    {
        public new const TokenType Type = TokenType.skip_lock;
        
        public SkipLock Value { get; set; }

        public TokenSkipLock()
        {
            _command = "Skip Lock";
            _description = "[Undocumented]";
            _length = 2;

            Value = (SkipLock) Tokenizer.ReadUInt8(1); if (!Enum.IsDefined(typeof(SkipLock), Value)) throw new ArgumentOutOfRangeException();
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Value.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddCombobox<SkipLock>("Status", "Value");
        }
        
        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Value;

            return output;
        }
    }

    public enum SkipLock
    {
        Unlocked = 0,
        Locked = 1
    }
}
