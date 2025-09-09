using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenGraphUvAuto : Token
    {
        public new const TokenType Type = TokenType.graph_uv_auto;

        byte fixed1;
        public UInt16 CropX { get; set; }
        public UInt16 CropY { get; set; }
        public UInt16 CropW { get; set; }
        public UInt16 CropH { get; set; }
        public UInt16 CurrentTick { get; set; }
        public UInt16 TotalTicks { get; set; }
        public UInt16 ImageNumber { get; set; }

        public TokenGraphUvAuto(bool blank = false)
        {
            _command = "Graph UV Auto";
            _description = "Automatically change from current crop to target crop. 60 ticks = 1 second";
            _length = 16;

            fixed1 = Tokenizer.ReadUInt8(1);
            CropX = Tokenizer.ReadUInt16(2);
            CropY = Tokenizer.ReadUInt16(4);
            CropW = Tokenizer.ReadUInt16(6);
            CropH = Tokenizer.ReadUInt16(8);
            CurrentTick = Tokenizer.ReadUInt16(10);
            TotalTicks = Tokenizer.ReadUInt16(12);
            ImageNumber = Tokenizer.ReadUInt16(14);
            
            UpdateData();
        }

        public override void UpdateData()
        {
            Data = String.Format("#: {0}, Target[{1}, {2}, {3}, {4}], Unknown4: {5} ticks, Current: {6}",
                Tokenizer.GetMemoryName(ImageNumber),
                Tokenizer.GetMemoryName(CropX),
                Tokenizer.GetMemoryName(CropY),
                Tokenizer.GetMemoryName(CropW),
                Tokenizer.GetMemoryName(CropH),
                Tokenizer.GetMemoryName(TotalTicks),
                Tokenizer.GetMemoryName(CurrentTick)
                );
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint16("X", "CropX");
            base.AddUint16("Y", "CropY");
            base.AddUint16("Width", "CropW");
            base.AddUint16("Height", "CropH");
            base.AddUint16("Current Tick", "CurrentTick");
            base.AddUint16("Total Ticks", "TotalTicks");
            base.AddUint16("Image Number", "ImageNumber");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)fixed1;
            BitConverter.GetBytes(CropX).CopyTo(output, 2);
            BitConverter.GetBytes(CropY).CopyTo(output, 4);
            BitConverter.GetBytes(CropW).CopyTo(output, 6);
            BitConverter.GetBytes(CropH).CopyTo(output, 8);
            BitConverter.GetBytes(CurrentTick).CopyTo(output, 10);
            BitConverter.GetBytes(TotalTicks).CopyTo(output, 12);
            BitConverter.GetBytes(ImageNumber).CopyTo(output, 14);

            return output;
        }
    }
}
