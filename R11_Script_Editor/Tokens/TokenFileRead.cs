using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenFileRead : Token
    {
        public new const TokenType Type = TokenType.file_read;
        
        public byte Unknown1 { get; set; }
        UInt16 Value;
        public FileType FType { get; set; }
        public UInt16 FNum { get; set; }

        public TokenFileRead(bool blank=false)
        {
            _command = "File Read";
            _description = "Read File.\n@TODO: Have a nice dropdown menu or search bar";
            _length = 4;

            if (blank)
            {
                Unknown1 = 0; //[00] or [01]
                Value = 0;
            }
            else
            {
                Unknown1 = Tokenizer.ReadUInt8(1); //[00] or [01]
                Value = Tokenizer.ReadUInt16(2);
            }


            FType = (FileType)(Value >> 12);
            FNum = (UInt16)(Value & 0x0FFF);

            UpdateData();
        }


        public override void UpdateData()
        {
            //Value = (UInt16)(((UInt16)FType) << 12 + FNum);
            Value = (UInt16)(((int)FType << 12) + FNum);
            Data = Tokenizer.GetFileName(Value);
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("Unknown", "Unknown1");
            base.AddCombobox<FileType>("File Type", "FType");
            base.AddUint16("File Number", "FNum");
        }
        
        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Unknown1;
            BitConverter.GetBytes(Value).CopyTo(output, 2);

            return output;
        }

        public enum FileType
        {
            MAC,
            BG,
            EV,
            CHR,
            BGM,
            SE
        }
    }
}
