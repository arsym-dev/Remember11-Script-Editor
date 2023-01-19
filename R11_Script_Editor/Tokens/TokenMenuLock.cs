using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenMenuLock : Token
    {
        public new const TokenType Type = TokenType.menu_lock;
        
        public MenuLock Value { get; set; }

        public TokenMenuLock()
        {
            _command = "Menu Lock";
            _description = "Prevent user from opening the menus";
            _length = 2;

            Value = (MenuLock) Tokenizer.ReadUInt8(1); if (!Enum.IsDefined(typeof(MenuLock), Value)) throw new ArgumentOutOfRangeException();
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = Value.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddCombobox<MenuLock>("Status", "Value");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Value;

            return output;
        }
    }

    public enum MenuLock
    {
        Unlocked = 0,
        Locked = 2
    }
}
