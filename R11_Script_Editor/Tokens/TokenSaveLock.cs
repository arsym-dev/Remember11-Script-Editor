using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenSaveLock : Token
    {
        public new const TokenType Type = TokenType.save_lock;
        
        public SaveLock Value { get; set; }

        public TokenSaveLock()
        {
            _command = "Save Lock";
            _description = "Determines where the player returns in they save or backjump. If they backjump to a locked section, they move back until the first unlocked line of text. This mainly prevents strange behavior in NVL mode.";
            _length = 2;

            Value = (SaveLock) Tokenizer.ReadUInt8(1); if (!Enum.IsDefined(typeof(SaveLock), Value)) throw new ArgumentOutOfRangeException();
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Value.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddCombobox<SaveLock>("Status", "Value");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Value;

            return output;
        }
    }

    public enum SaveLock
    {
        Unlocked = 0,
        Locked = 1
    }
}
