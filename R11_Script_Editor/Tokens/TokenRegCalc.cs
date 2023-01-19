using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenRegCalc : Token
    {
        public new const TokenType Type = TokenType.reg_calc;
        
        public RegCalcOperation Operation { get; set; }
        public UInt16 Arg0 { get; set; }
        public UInt16 Arg1 { get; set; }


        public TokenRegCalc(bool blank=false)
        {
            _command = "Reg Calc";
            _description = "Perform a calculation on a register/flag.\n@TODO: Search for variable names";
            _length = 6;

            if (blank)
            {
                Operation = RegCalcOperation.Set;
                Arg0 = 24576;
                Arg1 = 0;
            }
            else
            {
                Operation = (RegCalcOperation)Tokenizer.ReadUInt8(1); if (!Enum.IsDefined(typeof(RegCalcOperation), Operation)) throw new ArgumentOutOfRangeException();
                Arg0 = Tokenizer.ReadUInt16(2);
                Arg1 = Tokenizer.ReadUInt16(4);
            }
            
            
            UpdateData();
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)Operation;
            BitConverter.GetBytes(Arg0).CopyTo(output, 2);
            BitConverter.GetBytes(Arg1).CopyTo(output, 4);

            return output;
        }


        public override void UpdateData()
        {
            string mem0 = Tokenizer.GetMemoryName(Arg0);
            string mem1 = Tokenizer.GetMemoryName(Arg1);

            switch (Operation)
            {
                case RegCalcOperation.Set: Data = mem0 + " = " + mem1; break;
                case RegCalcOperation.Inc: Data = mem0 + " += " + mem1; break;
                case RegCalcOperation.Dec: Data = mem0 + " -= " + mem1; break;
                case RegCalcOperation.Mul: Data = mem0 + " *= " + mem1; break;
                case RegCalcOperation.Div: Data = mem0 + " /= " + mem1; break;
                case RegCalcOperation.Mod: Data = mem0 + " %= " + mem1; break;
                case RegCalcOperation.And: Data = mem0 + " &= " + mem1; break;
                case RegCalcOperation.SetMemToByte: Data = mem0 + " = BYTE[script offset " + mem1 + "]"; break;
                case RegCalcOperation.SetMemToWord: Data = mem0 + " = WORD[script offset " + mem1 + "]"; break;
                case RegCalcOperation.SetByteToMem: Data = "BYTE[script offset " + mem0 + "] = " + mem1; break;
                case RegCalcOperation.SetWordToMem: Data = "WORD[script offset " + mem0 + "] = " + mem1; break;
                case RegCalcOperation.SetMemToLiteral: Data = mem0 + " = LITERAL[" + Arg0.ToString() + "]"; break;
                case RegCalcOperation.SetScriptPointer: Data = "Jump to " + mem0; break; //Set next instruction to [script offset mem0]. Equivalent to "Jump to mem0"
                case RegCalcOperation.SetScriptPointer2: Data = "Jump to script offset 2*" + mem1 + "+" + mem0; break; //Set next instruction to [script offset 2*mem1+mem0]. Equivalent to "Jump"
                case RegCalcOperation.RandomMod: Data = mem0 + " = randInt() % " + mem1; break;
                case RegCalcOperation.ClippedInc: Data = mem0 + " += " + mem1 + "(Clipped at 255)"; break;
                case RegCalcOperation.ClippedDec: Data = mem0 + " -= " + mem1 + "(Clipped at 0)"; break;
                case RegCalcOperation.IncBy1: Data = mem0 + " ++"; break;
                case RegCalcOperation.UnlockAlbum: Data = "Unlock album entry"; Data2 = Tokenizer.GetEvTitle(Arg0); break; //Ensure Arg1 = 1
                case RegCalcOperation.SetSceneTitle: Data = "Set title"; Data2 = Tokenizer.GetSceneTitle(Arg0); break;
                case RegCalcOperation.UnlockMusic: Data = "Unlock music"; Data2 = Tokenizer.GetBgmTitle((Arg0 >> 1) & 0x7FF); break; //Ensure Arg1 = 1
                case RegCalcOperation.UnlockMovie: Data = "Unlock movie"; Data2 = "[UNDOCUMENTED] " + Arg0.ToString(); break; //Ensure Arg1 = 1
                case RegCalcOperation.HasReadAllMessages: Data = mem0 + " = Has read all 34774 messages?"; break;
            }
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddCombobox<RegCalcOperation>("Operation", "Operation");
            base.AddUint16("Arg 1", "Arg0");
            base.AddUint16("Arg 2", "Arg1");
        }
    }

    public enum RegCalcOperation
    {
        Set = 0x00,
        Inc = 0x01,
        Dec = 0x02,
        Mul = 0x03,
        Div = 0x04,
        Mod = 0x05,
        And = 0x06,
        Or = 0x07,
        SetMemToByte = 0x08,
        SetMemToWord = 0x09,
        SetByteToMem = 0x0A,
        SetWordToMem = 0x0B,
        SetMemToLiteral = 0x0C, // 0x0D - Not defined
        SetScriptPointer = 0x0E,
        SetScriptPointer2 = 0x0F,
        RandomMod = 0x10,
        ClippedInc = 0x11,
        ClippedDec = 0x12,
        IncBy1 = 0x13,
        UnlockAlbum = 0x14,
        SetSceneTitle = 0x15,
        UnlockMusic = 0x16,
        UnlockMovie = 0x17,
        HasReadAllMessages = 0x18, //Arg0 = (uint[02b56688] <= Sum of bits from BYTE[00e7e866] to BYTE[00e7e866 + 804])
    }
}
