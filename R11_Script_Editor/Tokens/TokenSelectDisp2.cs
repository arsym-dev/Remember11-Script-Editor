using System;
using System.Collections.Generic;
using System.Text;

namespace R11_Script_Editor.Tokens
{
    class TokenSelectDisp2 : Token
    {
        public new const TokenType Type = TokenType.sel_disp2;
        
        byte num_entries;
        UInt32 fixed1;
        public List<SelectDisp2Entry> Entries = new List<SelectDisp2Entry>();
        private List<string> _identical_jp_labels = new List<string>();
        public List<string> IdenticalJpLabels
        {
            get => _identical_jp_labels;
            set => _identical_jp_labels = value;
        }

        public TokenSelectDisp2(bool blank=false)
        {
            _command = "Select Disp2";
            _description = "Display choices (version 2)";

            if (blank)
            {
                num_entries = 2;

                fixed1 = 0x00006009;

                for (int i = 0; i < num_entries; i++)
                {
                    var entry = new SelectDisp2Entry();
                    entry.MsgPtr = 0;
                    entry.Unknown1 = 0xffff;
                    entry.Unknown2 = 1;
                    entry.ChoiceId = (ushort) i;

                    entry.Message = "Choice"+i.ToString();
                    // Remove double spaces
                    entry.TempMsg = Tokenizer.StringSingleSpace(entry.Message);
                    Entries.Add(entry);
                }
            }
            else
            {
                num_entries = Tokenizer.ReadUInt8(1);

                fixed1 = 0x00006009;

                for (int i = 0; i < num_entries; i++)
                {
                    var entry = new SelectDisp2Entry();
                    entry.MsgPtr = Tokenizer.ReadUInt16(i * 8 + 6);
                    entry.Unknown1 = Tokenizer.ReadUInt16(i * 8 + 8);
                    entry.Unknown2 = Tokenizer.ReadUInt16(i * 8 + 10);
                    entry.ChoiceId = Tokenizer.ReadUInt16(i * 8 + 12);

                    entry.Message = Tokenizer.ReadString(entry.MsgPtr);
                    // Remove double spaces
                    entry.TempMsg = Tokenizer.StringSingleSpace(entry.Message);
                    Entries.Add(entry);
                }
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
                BitConverter.GetBytes(e.MsgPtr).CopyTo(output, i * 8 + 6);
                BitConverter.GetBytes(e.Unknown1).CopyTo(output, i * 8 + 8);
                BitConverter.GetBytes(e.Unknown2).CopyTo(output, i * 8 + 10);
                BitConverter.GetBytes(e.ChoiceId).CopyTo(output, i * 8 + 12);
            }

            return output;
        }

        public override string GetMessages()
        {
            string msg = "";
            foreach (var e in Entries)
                msg += e.Message + "/" + e.MessageJp + "/";

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
            for (int i=0; i<Entries.Count; i++)
            {
                var e = Entries[i];
                e.MsgPtr = (UInt16)offset;
                offset += Tokenizer.StringEncode(e.Message).Length + 1;
            }
            return offset;
        }

        public override void UpdateData()
        {
            _length = 6 + 8 * Entries.Count;
            num_entries = (byte)Entries.Count;
            Data = "Choices: " + Entries.Count.ToString();

            var m = new List<string>();
            for (int i = 0; i < Entries.Count; i++)
            {
                Entries[i].Message = Tokenizer.StringDoubleSpace(Entries[i].TempMsg);
                m.Add(Entries[i].Message);
            }

            Data2 = String.Join(" / ", m);
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            PopulateEntryList(Entries, (sender, ev) => {
                if (ev.AddedItems.Count == 0 || !(ev.AddedItems[0] is SelectDisp2Entry))
                    return;

                base.UpdateGui(false);
                var e = (SelectDisp2Entry)ev.AddedItems[0];
                AddTextbox("Text (EN)", "TempMsg", e);
                AddTextbox("Text (JP)", "MessageJp", e);
                AddUint16("Unknown1", "Unknown1", e);
                AddUint16("Unknown2", "Unknown2", e);
                AddUint16("Choice ID", "ChoiceId", e);
            });
        }
    }

    class SelectDisp2Entry
    {
        public UInt16 MsgPtr { get; set; }
        public String Message { get; set; }
        public String MessageJp { get; set; }
        public String TempMsg { get; set; }
        public UInt16 Unknown1 { get; set; } //[FF FF] fixed
        public UInt16 Unknown2 { get; set; } //[01 00] fixed
        public UInt16 ChoiceId { get; set; }
    }
}
