using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenGraphDisp : Token
    {
        public new const TokenType Type = TokenType.graph_disp;

        byte num_entries;
        public List<GraphDispEntry> Entries = new List<GraphDispEntry>();

        public TokenGraphDisp(bool blank = false)
        {
            _command = "Graph Disp";
            _description = "Display an image or sprite" +
                "\n\nFile Number Start Ranges:" +
                "\n12289: Kokoro Start" +
                "\n12377: Satoru Start" +
                "\n12509: Mayuzumi Start" +
                "\n12597: Yomogi Start" +
                "\n12685: Kali Start" +
                "\n12817: Keiko Start" +
                "\n12949: Yuni Start" +
                "\n13086: Hotori Start" +
                "\n13122: Yukidoh Start" +
                "\n13194: Sayaka Start";


            if (blank)
            {
                num_entries = 1;
                Entries.Add(new GraphDispEntry()
                {
                    ImageNumber = 256,
                    Unknown2 = 7935,
                    FileDescription = 4096,
                    Unknown3 = 0,
                    DrawX = 0,
                    DrawY = 0
                });
            }
            else
            {
                num_entries = Tokenizer.ReadUInt8(1);

                for (int i = 0; i < num_entries; i++)
                {
                    Entries.Add(new GraphDispEntry()
                    {
                        ImageNumber = Tokenizer.ReadUInt16(i * 12 + 2),
                        Unknown2 = Tokenizer.ReadUInt16(i * 12 + 4),
                        FileDescription = Tokenizer.ReadUInt16(i * 12 + 6),
                        Unknown3 = Tokenizer.ReadUInt16(i * 12 + 8),
                        DrawX = Tokenizer.ReadUInt16(i * 12 + 10),
                        DrawY = Tokenizer.ReadUInt16(i * 12 + 12)
                    });
                }
            }

            UpdateData();
        }

        public override void UpdateData()
        {
            _length = 2 + 12 * Entries.Count;
            num_entries = (byte)Entries.Count;

            var list_names = new List<string>();
            foreach (var e in Entries)
            {
                if (e.FileDescription == 0xFFFF)
                    list_names.Add("CLEAR SPRITES");
                else
                    list_names.Add(Tokenizer.GetFileName(e.FileDescription));
            }

            Data = "Images: " + num_entries.ToString() + " [" + String.Join(", ", list_names) + "]";
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            PopulateEntryList(Entries, (sender, ev) => {
                if (ev.AddedItems.Count == 0 || !(ev.AddedItems[0] is GraphDispEntry))
                    return;

                base.UpdateGui(false);
                var e = (GraphDispEntry)ev.AddedItems[0];
                AddUint16("ImageNumber", "ImageNumber", e);
                AddUint16("Unknown2", "Unknown2", e);
                AddUint16("File", "FileDescription", e);
                AddUint16("Unknown3", "Unknown3", e);
                AddUint16("Draw X", "DrawX", e);
                AddUint16("Draw Y", "DrawY", e);
            });
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)num_entries;
            for (int i=0; i<num_entries; i++)
            {
                var e = Entries[i];
                BitConverter.GetBytes(e.ImageNumber).CopyTo(output, i * 12 + 2);
                BitConverter.GetBytes(e.Unknown2).CopyTo(output, i * 12 + 4);
                BitConverter.GetBytes(e.FileDescription).CopyTo(output, i * 12 + 6);
                BitConverter.GetBytes(e.Unknown3).CopyTo(output, i * 12 + 8);
                BitConverter.GetBytes(e.DrawX).CopyTo(output, i * 12 + 10);
                BitConverter.GetBytes(e.DrawY).CopyTo(output, i * 12 + 12);
            }
            
            return output;
        }
    }

    class GraphDispEntry
    {
        public UInt16 ImageNumber { get; set; } //Layer number?
        public UInt16 Unknown2 { get; set; } //Fade-in type?
        public UInt16 FileDescription { get; set; }
        public UInt16 Unknown3 { get; set; }
        public UInt16 DrawX { get; set; }
        public UInt16 DrawY { get; set; }
    }
}
