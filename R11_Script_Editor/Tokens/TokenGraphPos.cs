using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenGraphPos : Token
    {
        public new const TokenType Type = TokenType.graph_pos;
        
        public byte ImageNumber { get; set; }
        public UInt16 X { get; set; }
        public UInt16 Y { get; set; }

        public TokenGraphPos()
        {
            _command = "Graph Pos";
            _description = "Sets the position of an image";
            _length = 6;

            ImageNumber = Tokenizer.ReadUInt8(1);
            X = Tokenizer.ReadUInt16(2);
            Y = Tokenizer.ReadUInt16(4);
            
            UpdateData();
        }
        
        public override void UpdateData()
        {
            Data = ImageNumber.ToString() + ", X: " + X.ToString() + ", Y: " + Y.ToString();
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("Image Number", "ImageNumber");
            base.AddUint16("X", "X");
            base.AddUint16("Y", "Y");
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)ImageNumber;
            BitConverter.GetBytes(X).CopyTo(output, 2);
            BitConverter.GetBytes(Y).CopyTo(output, 4);

            return output;
        }
    }
}
