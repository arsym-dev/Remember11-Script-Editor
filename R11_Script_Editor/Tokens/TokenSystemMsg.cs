using System;
using System.Collections.Generic;
using System.Text;

namespace R11_Script_Editor.Tokens
{
    class TokenSystemMsg : Token
    {
        public new const TokenType Type = TokenType.sys_msg;

        byte fixed1;
        public UInt16 MsgPtr;
        public String Message { get; set; }
        public String MessageJp { get; set; }
        public String TempMsg { get; set;}

        public TokenSystemMsg(bool blank=false)
        {
            _command = "Sys Msg";
            _description = "Display a full-screen system message, usually after an ending unlocks something.";
            _length = 4;

            fixed1 = 0;

            if (blank)
            {
                MsgPtr = 0;
                Message = "Message";

                // Remove double spaces
                TempMsg = Tokenizer.StringSingleSpace(Message);
            }
            else
            {
                MsgPtr = Tokenizer.ReadUInt16(2);
                Message = Tokenizer.ReadString(MsgPtr);

                // Remove double spaces
                TempMsg = Tokenizer.StringSingleSpace(Message);
            }

            UpdateData();
        }

        public override void UpdateData()
        {
            Message = Tokenizer.StringDoubleSpace(TempMsg);
            Data2 = Message;
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddTextbox("Text (EN)", "TempMsg");
            base.AddTextbox("Text (JP)", "MessageJp");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)fixed1;
            BitConverter.GetBytes(MsgPtr).CopyTo(output, 2);

            return output;
        }

        public override string GetMessages()
        {
            return Message + MessageJp;
        }

        public override byte[] GetMessagesBytes()
        {
            byte[] msg = Tokenizer.StringEncode(Message);
            byte[] output = new byte[msg.Length + 1];
            msg.CopyTo(output, 0);

            return output;
        }

        public override int SetMessagePointer(int offset)
        {
            MsgPtr = (UInt16)offset;
            return offset + Tokenizer.StringEncode(Message).Length + 1;
        }

    }
}
