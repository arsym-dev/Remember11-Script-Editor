using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenBreathLock : Token
    {
        public new const TokenType Type = TokenType.breath_lock;

        public BreathLock Value { get; set; }

        public TokenBreathLock()
        {
            _command = "Breath Lock";
            _description = "[Undocumented]";
            _length = 2;

            Value = (BreathLock) Tokenizer.ReadUInt8(1); if (!Enum.IsDefined(typeof(BreathLock), Value)) throw new ArgumentOutOfRangeException();

            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Value.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddCombobox<BreathLock>("Status", "Value");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Value;

            return output;
        }
    }

    public enum BreathLock
    {
        Unlocked = 0,
        Locked = 1
    }
}
