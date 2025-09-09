using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenEffectEx : Token
    {
        public new const TokenType Type = TokenType.effect_ex;

        public byte Var1 { get; set; }
        public byte EffectType { get; set; }
        public byte Var3 { get; set; }
        public UInt16 Var4 { get; set; }
        public UInt16 Var5 { get; set; }
        public UInt16 Var6 { get; set; }
        public UInt16 Var7 { get; set; }
        public UInt16 Var8 { get; set; }
        public UInt16 Var9 { get; set; }
        public UInt16 Var10 { get; set; }
        public UInt16 Var11 { get; set; }
        public UInt16 Var12 { get; set; }
        public UInt16 Var13 { get; set; }
        public UInt16 Var14 { get; set; }
        public UInt16 Var15 { get; set; }
        public UInt16 Var16 { get; set; }
        public UInt16 Var17 { get; set; }
        public UInt16 Var18 { get; set; }
        public UInt16 Var19 { get; set; }
        public UInt16 Var20 { get; set; }

        public TokenEffectEx(bool blank = false)
        {
            _command = "Effect Ex";

            _description = "[Undocumented]\nEffectType takes special values:" +
                "\n16 - Screen flashing (DMT effect)" +
                "\n18 - Ever17 perspective change effect" +
                "\n19 - Dark overlay" +
                "\n20 - Flashback desaturated overlay" +
                "\n21 - Color overlay" +
                "\n    Var3: 0" +
                "\n    Var4: Duration (10 is quick)" +
                "\n    Var5: Red" +
                "\n    Var6: Green" +
                "\n    Var7: Blue" +
                "\n    Var8: Opacity (15 is maximum)" +
                "\n22 - Flashback overlay (white vignette)" +
                "\n23 - Snow / Blizzard" +
                "\n    Var3: 1" +
                "\n    Var4: Duration (60 = 1 sec, 0 keeps the effect permanently)" +
                "\n    Var5: Direction (0 is right, 1 is left)" +
                "\n    Var6: Fog opacity (15 is maximum)" +
                "\n    Var7: Snow speed (100 regular snow, 256 blizzard)" +
                "\n24 - Grayscale" +
                "\n-----" +
                "\n252" +
                "\n253 (apply effect?)" +
                "\n254" +
                "\n255";

            _length = 38;

            if (blank)
            {

            }
            else
            {
                Var1 = Tokenizer.ReadUInt8(1);
                EffectType = Tokenizer.ReadUInt8(2);
                Var3 = Tokenizer.ReadUInt8(3);
                Var4 = Tokenizer.ReadUInt16(4);
                Var5 = Tokenizer.ReadUInt16(6);
                Var6 = Tokenizer.ReadUInt16(8);
                Var7 = Tokenizer.ReadUInt16(10);
                Var8 = Tokenizer.ReadUInt16(12);
                Var9 = Tokenizer.ReadUInt16(14);
                Var10 = Tokenizer.ReadUInt16(16);
                Var11 = Tokenizer.ReadUInt16(18);
                Var12 = Tokenizer.ReadUInt16(20);
                Var13 = Tokenizer.ReadUInt16(22);
                Var14 = Tokenizer.ReadUInt16(24);
                Var15 = Tokenizer.ReadUInt16(26);
                Var16 = Tokenizer.ReadUInt16(28);
                Var17 = Tokenizer.ReadUInt16(30);
                Var18 = Tokenizer.ReadUInt16(32);
                Var19 = Tokenizer.ReadUInt16(34);
                Var20 = Tokenizer.ReadUInt16(36);
            }


            UpdateData();
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Var1;
            output[2] = (byte)EffectType;
            output[3] = (byte)Var3;
            BitConverter.GetBytes(Var4).CopyTo(output, 4);
            BitConverter.GetBytes(Var5).CopyTo(output, 6);
            BitConverter.GetBytes(Var6).CopyTo(output, 8);
            BitConverter.GetBytes(Var7).CopyTo(output, 10);
            BitConverter.GetBytes(Var8).CopyTo(output, 12);
            BitConverter.GetBytes(Var9).CopyTo(output, 14);
            BitConverter.GetBytes(Var10).CopyTo(output, 16);
            BitConverter.GetBytes(Var11).CopyTo(output, 18);
            BitConverter.GetBytes(Var12).CopyTo(output, 20);
            BitConverter.GetBytes(Var13).CopyTo(output, 22);
            BitConverter.GetBytes(Var14).CopyTo(output, 24);
            BitConverter.GetBytes(Var15).CopyTo(output, 26);
            BitConverter.GetBytes(Var16).CopyTo(output, 28);
            BitConverter.GetBytes(Var17).CopyTo(output, 30);
            BitConverter.GetBytes(Var18).CopyTo(output, 32);
            BitConverter.GetBytes(Var19).CopyTo(output, 34);
            BitConverter.GetBytes(Var20).CopyTo(output, 36);

            return output;
        }


        public override void UpdateData()
        {
            Data = "[UNDOCUMENTED]";
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddUint8("Var1", "Var1");
            base.AddUint8("Effect Type", "EffectType");
            base.AddUint8("Var3", "Var3");
            base.AddUint16("Var4", "Var4");
            base.AddUint16("Var5", "Var5");
            base.AddUint16("Var6", "Var6");
            base.AddUint16("Var7", "Var7");
            base.AddUint16("Var8", "Var8");
            base.AddUint16("Var9", "Var9");
            base.AddUint16("Var10", "Var10");
            base.AddUint16("Var11", "Var11");
            base.AddUint16("Var12", "Var12");
            base.AddUint16("Var13", "Var13");
            base.AddUint16("Var14", "Var14");
            base.AddUint16("Var15", "Var15");
            base.AddUint16("Var16", "Var16");
            base.AddUint16("Var17", "Var17");
            base.AddUint16("Var18", "Var18");
            base.AddUint16("Var19", "Var19");
            base.AddUint16("Var20", "Var20");
        }
    }
}
