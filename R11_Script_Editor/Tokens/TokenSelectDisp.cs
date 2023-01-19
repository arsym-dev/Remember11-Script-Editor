using System;
using System.Collections.Generic;
using System.Text;

namespace R11_Script_Editor.Tokens
{
    class TokenSelectDisp : Token
    {
        public new const TokenType Type = TokenType.sel_disp;
        
        byte num_entries;
        UInt16 fixed1;
        public List<SelectDispEntry> Entries = new List<SelectDispEntry>();

        public TokenSelectDisp()
        {
            _command = "Select Disp";
            _description = "Display choices [@TODO: Fix jump/label handling]";
            num_entries = Tokenizer.ReadUInt8(1);

            fixed1 = 0x6009;

            for (int i=0; i<num_entries; i++)
            {
                var entry = new SelectDispEntry();
                entry.MsgPtr = Tokenizer.ReadUInt16(i * 8 + 4);
                entry.JumpAddress = Tokenizer.ReadUInt16(i * 8 + 6);
                entry.Unknown = Tokenizer.ReadUInt16(i * 8 + 8);
                entry.ChoiceId = Tokenizer.ReadUInt16(i * 8 + 10);

                entry.Message = Tokenizer.ReadString(entry.MsgPtr);
                Entries.Add(entry);
            }

            UpdateData();
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)num_entries;
            BitConverter.GetBytes(fixed1).CopyTo(output, 2);
            for (int i = 0; i < num_entries; i++)
            {
                var e = Entries[i];
                BitConverter.GetBytes(e.MsgPtr).CopyTo(output, i * 8 + 4);
                BitConverter.GetBytes(e.JumpAddress).CopyTo(output, i * 8 + 6);
                BitConverter.GetBytes(e.Unknown).CopyTo(output, i * 8 + 8);
                BitConverter.GetBytes(e.ChoiceId).CopyTo(output, i * 8 + 10);
            }

            return output;
        }

        public override string GetMessages()
        {
            string msg = "";
            foreach (var e in Entries)
                msg += e.Message + " ";

            return msg;
        }

        public override byte[] GetMessagesBytes()
        {
            byte[] msg;
            List<byte[]> messages = new List<byte[]>();
            int len = 0;
            int offset = 0;

            foreach (var e in Entries)
            {
                msg = Tokenizer.StringEncode(e.Message);
                messages.Add(msg);
                len += msg.Length + 1;
            }

            byte[] output = new byte[len];
            foreach (var m in messages)
            {
                m.CopyTo(output, offset);
                offset += m.Length + 1;
            }

            return output;
        }

        public override int SetMessagePointer(int offset)
        {
            for (int i = 0; i < Entries.Count; i++)
            {
                var e = Entries[i];
                e.MsgPtr = (UInt16)offset;
                offset += Tokenizer.StringEncode(e.Message).Length + 1;
            }
            return offset;
        }

        public override void UpdateData()
        {
            _length = 4 + 8 * Entries.Count;
            Data = "Choices: " + Entries.Count.ToString();

            for (int i = 0; i < Entries.Count; i++)
            {
                if (i == 0)
                    Data2 += Entries[i].Message;
                else
                    Data2 += " / " + Entries[i].Message;
            }
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            PopulateEntryList(Entries, (sender, ev) => {
                if (ev.AddedItems.Count == 0 || !(ev.AddedItems[0] is SelectDispEntry))
                    return;

                base.UpdateGui(false);
                var e = (SelectDispEntry)ev.AddedItems[0];
                AddTextbox("Message", "Message", e);
                AddUint16("JumpAddress", "Unknown1", e);
                AddUint16("Unknown", "Unknown", e);
                AddUint16("Choice ID", "ChoiceId", e);
            });
        }
    }

    class SelectDispEntry
    {
        public UInt16 MsgPtr { get; set; }
        public String Message { get; set; }
        public UInt16 JumpAddress { get; set; } //Address to go to if selected. [FF FF] seems to mean don't jump.
        public UInt16 Unknown { get; set; } //[01 00] fixed
        public UInt16 ChoiceId { get; set; }
    }
}
