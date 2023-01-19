using System;
using System.Collections.Generic;

namespace R11_Script_Editor.Tokens
{
    class TokenIf : Token
    {
        public new const TokenType Type = TokenType.ifx;
        
        public IfAction Action { get; set; }
        public UInt16 ptr_jump { get; set; }
        UInt16 final_jump_addr;
        public UInt16 PtrJump { get => final_jump_addr; }
        public String LabelJump { get; set; }
        public UInt16 Arg0 { get; set; }
        public UInt16 Arg1 { get; set; }
        public IfCondition Condition { get; set; }
        byte fixed1;

        public TokenIf(bool blank=false)
        {
            _command = "If";
            _description = "Performs a jump if the specified criterion is met.";
            _length = 10;

            if (blank)
            {
                Action = IfAction.If;
                ptr_jump = 0;
                Arg0 = 24618;
                Arg1 = 2;
                Condition = IfCondition.Equal;
                fixed1 = 0;
            }
            else
            {
                Action = (IfAction)Tokenizer.ReadUInt8(1); if (!Enum.IsDefined(typeof(IfAction), Action)) throw new ArgumentOutOfRangeException();
                ptr_jump = Tokenizer.ReadUInt16(2);
                Arg0 = Tokenizer.ReadUInt16(4);
                Arg1 = Tokenizer.ReadUInt16(6);
                Condition = (IfCondition)Tokenizer.ReadUInt8(8); if (!Enum.IsDefined(typeof(IfCondition), Condition)) throw new ArgumentOutOfRangeException();
                fixed1 = 0;
            }
            
            UpdateData();
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];

            if (Action == IfAction.SwitchCase)
            {
                // @TODO: Uncomment these once I've fixed the switch
                /*
                output[0] = (byte)Type;
                output[1] = (byte)IfAction.If;
                BitConverter.GetBytes(final_jump_addr).CopyTo(output, 2);
                BitConverter.GetBytes(Arg0).CopyTo(output, 4);
                BitConverter.GetBytes(Arg1).CopyTo(output, 6);
                output[8] = (byte)IfCondition.NotEqual;
                output[9] = (byte)fixed1;
                */
                output[0] = (byte)Type;
                output[1] = (byte)Action;
                BitConverter.GetBytes(ptr_jump).CopyTo(output, 2);
                BitConverter.GetBytes(Arg0).CopyTo(output, 4);
                BitConverter.GetBytes(Arg1).CopyTo(output, 6);
                output[8] = (byte)Condition;
                output[9] = (byte)fixed1;
            }
            else if (Action == IfAction.SwitchBreak)
            {
                /*
                // Replace with IntGoto
                var t = new TokenInternalGoto(false);
                t.UpdateAddress(final_jump_addr);
                return t.GetBytes();
                */

                /*
                // Force a jump by checking 0==0
                output[0] = (byte)Type;
                output[1] = (byte)IfAction.If;
                BitConverter.GetBytes(final_jump_addr).CopyTo(output, 2);
                BitConverter.GetBytes(0).CopyTo(output, 4);
                BitConverter.GetBytes(0).CopyTo(output, 6);
                output[8] = (byte)IfCondition.Equal;
                output[9] = (byte)fixed1;
                */
                output[0] = (byte)Type;
                output[1] = (byte)Action;
                BitConverter.GetBytes(ptr_jump).CopyTo(output, 2);
                BitConverter.GetBytes(Arg0).CopyTo(output, 4);
                BitConverter.GetBytes(Arg1).CopyTo(output, 6);
                output[8] = (byte)Condition;
                output[9] = (byte)fixed1;
            }
            else
            {
                output = new byte[_length];
                output[0] = (byte)Type;
                output[1] = (byte)Action;
                BitConverter.GetBytes(final_jump_addr).CopyTo(output, 2);
                BitConverter.GetBytes(Arg0).CopyTo(output, 4);
                BitConverter.GetBytes(Arg1).CopyTo(output, 6);
                output[8] = (byte)Condition;
                output[9] = (byte)fixed1;
            }

            return output;
        }

        public override void UpdateData()
        {
            string mem0 = Tokenizer.GetMemoryName(Arg0);
            string mem1 = Tokenizer.GetMemoryName(Arg1);
            string cond_txt = "";
            switch (Condition)
            {
                case IfCondition.Equal: cond_txt = mem0 + " == " + mem1; break;
                case IfCondition.NotEqual: cond_txt = mem0 + " != " + mem1; break;
                case IfCondition.Less: cond_txt = mem0 + " < " + mem1; break;
                case IfCondition.Greater: cond_txt = mem0 + " > " + mem1; break;
                case IfCondition.LessEqual: cond_txt = mem0 + " <= " + mem1; break;
                case IfCondition.GreaterEqual: cond_txt = mem0 + " >= " + mem1; break;
                case IfCondition.And: cond_txt = mem0 + " && " + mem1; break;
                case IfCondition.Or: cond_txt = mem0 + " || " + mem1; break;
            }

            UInt16 temp_addr = ptr_jump;

            switch (Action)
            {
                case IfAction.If:
                    final_jump_addr = ptr_jump;
                    if (LabelJump == null)
                        LabelJump = final_jump_addr.ToString();

                    Data = "if (" + cond_txt + ")"; Data2 = "Jump to " + LabelJump;
                    break;
                case IfAction.IfNot:
                    final_jump_addr = ptr_jump;
                    if (LabelJump == null)
                        LabelJump = ptr_jump.ToString();

                    Data = "if NOT (" + cond_txt + ")"; Data2 = "Jump to " + LabelJump;
                    break;
                case IfAction.SwitchBreak:
                    while (Tokenizer.ReadUInt16(temp_addr, false) != 3)
                        temp_addr += 4;

                    final_jump_addr = Tokenizer.ReadUInt16(temp_addr + 2, false);
                    if (LabelJump == null)
                        LabelJump = final_jump_addr.ToString();

                    Data = "[Break]";
                    Data2 = String.Format("Jump to {0}", LabelJump);
                    break;
                    
                case IfAction.SwitchCase:
                    byte temp = Tokenizer.ReadUInt8(temp_addr+1, false);

                    while (true)
                    {
                        temp_addr += 4;
                        if (Tokenizer.ReadUInt8(temp_addr+1, false) == temp)
                            break;
                    }

                    final_jump_addr = Tokenizer.ReadUInt16(temp_addr + 2, false);
                    if (LabelJump == null)
                        LabelJump = final_jump_addr.ToString();

                    Data = String.Format("[Case] {0} != {1}", mem0, mem1);
                    Data2 = String.Format("Jump to {0}", LabelJump);
                    break;
            }
        }

        public void UpdateAddress(UInt16 addr)
        {
            final_jump_addr = addr;
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddCombobox<IfAction>("Action", "Action");
            base.AddUint16("Arg 1", "Arg0");
            base.AddCombobox<IfCondition>("Condition", "Condition");
            base.AddUint16("Arg 2", "Arg1");
            base.AddTextbox("Jump Label", "LabelJump");
        }
    }

    public enum IfAction
    {
        If = 0,
        IfNot = 1,
        SwitchBreak = 0xFE, // These are created by subroutines and have a separate table at the end of the script for holding jump addresses
        SwitchCase = 0xFF // These are created by subroutines and have a separate table at the end of the script for holding jump addresses
    }

    public enum IfCondition
    {
        Equal,
        NotEqual,
        Less,
        Greater,
        LessEqual,
        GreaterEqual,
        And,
        Or
    }
}
