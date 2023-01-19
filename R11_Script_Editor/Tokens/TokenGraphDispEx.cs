using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenGraphDispEx : Token
    {
        public new const TokenType Type = TokenType.graph_disp_ex;
        
        byte num_entries;
        public List<GraphDispExEntry> Entries = new List<GraphDispExEntry>();

        public TokenGraphDispEx(bool blank=false)
        {
            _command = "Graph Disp Ex";
            _description = "Display an image onscreen (extended function can crop)";

            if (blank)
            {
                num_entries = 1;
                Entries.Add(new GraphDispExEntry()
                {
                    ImageNumber = 0,
                    Unknown2 = 0,
                    FileDescription = 0,
                    Unknown3 = 0,
                    DrawX = 0,
                    DrawY = 0,
                    CropX = 0,
                    CropY = 0,
                    CropW = 0,
                    CropH = 0
                });
            }
            else
            {
                num_entries = Tokenizer.ReadUInt8(1);

                for (int i = 0; i < num_entries; i++)
                {
                    Entries.Add(new GraphDispExEntry()
                    {
                        ImageNumber = Tokenizer.ReadUInt16(i * 20 + 2),
                        Unknown2 = Tokenizer.ReadUInt16(i * 20 + 4),
                        FileDescription = Tokenizer.ReadUInt16(i * 20 + 6),
                        Unknown3 = Tokenizer.ReadUInt16(i * 20 + 8),
                        DrawX = Tokenizer.ReadUInt16(i * 20 + 10),
                        DrawY = Tokenizer.ReadUInt16(i * 20 + 12),
                        CropX = Tokenizer.ReadUInt16(i * 20 + 14),
                        CropY = Tokenizer.ReadUInt16(i * 20 + 16),
                        CropW = Tokenizer.ReadUInt16(i * 20 + 18),
                        CropH = Tokenizer.ReadUInt16(i * 20 + 20)
                    });
                }
            }
            
                        
            UpdateData();
        }

        public override void UpdateData()
        {
            _length = 2 + 20 * Entries.Count;
            num_entries = (byte)Entries.Count;

            var list_names = new List<string>();
            foreach (var e in Entries)
                list_names.Add( Tokenizer.GetFileName(e.FileDescription) );

            Data = "Images: " + num_entries.ToString() + " [" + String.Join(", ", list_names) + "]";
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)num_entries;
            for (int i = 0; i < num_entries; i++)
            {
                var e = Entries[i];
                BitConverter.GetBytes(e.ImageNumber).CopyTo(output, i * 20 + 2);
                BitConverter.GetBytes(e.Unknown2).CopyTo(output, i * 20 + 4);
                BitConverter.GetBytes(e.FileDescription).CopyTo(output, i * 20 + 6);
                BitConverter.GetBytes(e.Unknown3).CopyTo(output, i * 20 + 8);
                BitConverter.GetBytes(e.DrawX).CopyTo(output, i * 20 + 10);
                BitConverter.GetBytes(e.DrawY).CopyTo(output, i * 20 + 12);
                BitConverter.GetBytes(e.CropX).CopyTo(output, i * 20 + 14);
                BitConverter.GetBytes(e.CropY).CopyTo(output, i * 20 + 16);
                BitConverter.GetBytes(e.CropW).CopyTo(output, i * 20 + 18);
                BitConverter.GetBytes(e.CropH).CopyTo(output, i * 20 + 20);
            }

            return output;
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            PopulateEntryList(Entries, (sender, ev) => {
                if (ev.AddedItems.Count == 0 || !(ev.AddedItems[0] is GraphDispExEntry))
                    return;

                base.UpdateGui(false);
                var e = (GraphDispExEntry)ev.AddedItems[0];
                AddUint16("Image Number", "ImageNumber", e);
                AddUint16("Unknown 2", "Unknown2", e);
                AddUint16("File", "FileDescription", e);
                AddUint16("Unknown3", "Unknown3", e);
                AddUint16("Draw X", "DrawX", e);
                AddUint16("Draw Y", "DrawY", e);
                AddUint16("Crop X", "CropX", e);
                AddUint16("Crop Y", "CropY", e);
                AddUint16("Crop Width", "CropW", e);
                AddUint16("Crop Height", "CropH", e);
            });
        }
    }

    class GraphDispExEntry
    {
        public UInt16 ImageNumber { get; set; } //Image number?
        public UInt16 Unknown2 { get; set; } //Fade-in type?
        public UInt16 FileDescription { get; set; }
        public UInt16 Unknown3 { get; set; } //[00 00] or [00 01]
        public UInt16 DrawX { get; set; }
        public UInt16 DrawY { get; set; }
        public UInt16 CropX { get; set; }
        public UInt16 CropY { get; set; }
        public UInt16 CropW { get; set; }
        public UInt16 CropH { get; set; }
    }
}
