using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace R11_Script_Editor.Tokens
{
    class TokenMsgDisp2 : Token
    {
        public new const TokenType Type = TokenType.msg_disp2;

        byte fixed1;
        UInt16 msg_ptr;
        public UInt16 MsgPtr { get => msg_ptr; }
        public UInt16 MsgId { get; set; }
        public UInt16 VoiceId { get; set; }
        public MsgSpeaker SpeakerId { get; set; }
        String CompleteMessage;
        public string Speaker { get; set; }
        public string Message { get; set; }
        public string MessageEnding { get; set; }
        public string MessageJp { get; set; }
        private List<string> _identical_jp_labels = new List<string>();
        public List<string> IdenticalJpLabels {
            get =>  _identical_jp_labels;
            set => _identical_jp_labels = value; }

        static Regex terminator_regex = new Regex(@"(%[%ABCDEKNPpSTV0-9]*?)$");

        public TokenMsgDisp2(bool blank=false)
        {
            _command = "Msg Disp2";
            _description = "Displays text spoken by a character.\n" +
                "\nSpeaker ID: The sprite to animate" + 
                "\n\nColor:\n%C<RGBA>\n(eg. %C8CFF would give cyan, %CF66A is a slightly transparent red. Return to white using %CFFFF)" +
                "\n\nFade in:\n%FS<Text>%FE" +
                "\n\nWait for user input:\n%K" +
                "\n\nAlignment:\n%LR (right-align)\n%LC (center)" +
                "\n\nNew line:\n%N"+
                "\n\nFade out text:\n%O (used in the \"Infinity Loop\" Message)"+
                "\n\nClear text:\n%P (usually at end of dialogue)"+
                "\n\nTime delay?:\n%T##\n(eg. %T60 = delay of 60 ticks)" +
                "\n\nTIP:\n%TS###<Text>%TE\n(eg. %TS083That Guy%TE)" +
                "\n\nWait for voice line to end\n%V (at end of dialogue)" +
                "\n\nX Position:\n%X### (before start of text)\n(eg. %X050 shift to 50 pixels from left)";
            _length = 10;

            if (blank)
            {
                fixed1 = 0;
                msg_ptr = 0;
                CompleteMessage = "Message%K%P";

                MsgId = 0;
                VoiceId = 0;
                SpeakerId = MsgSpeaker.Narrator;
            }
            else
            {
                fixed1 = 0;
                msg_ptr = Tokenizer.ReadUInt16(2);
                CompleteMessage = Tokenizer.ReadString(msg_ptr);

                MsgId = Tokenizer.ReadUInt16(4);
                VoiceId = Tokenizer.ReadUInt16(6);
                SpeakerId = (MsgSpeaker)Tokenizer.ReadUInt16(8); if (!Enum.IsDefined(typeof(MsgSpeaker), SpeakerId)) throw new ArgumentOutOfRangeException();
            }
            
            

            int idx1 = CompleteMessage.IndexOf("「");
            int idx2 = CompleteMessage.IndexOf("」");

            if (idx1 > 0)
                Speaker = CompleteMessage.Substring(0, idx1);
            else
                Speaker = "";

            if (idx2 >= 0)
                MessageEnding = CompleteMessage.Substring(idx2 + 1, CompleteMessage.Length - idx2 - 1);
            else if (terminator_regex.IsMatch(CompleteMessage))
            {
                MatchCollection mc = terminator_regex.Matches(CompleteMessage);

                if (mc.Count != 1) throw new Exception("Expected only 1 result");

                MessageEnding = mc[0].Value;
                idx2 = CompleteMessage.Length - MessageEnding.Length;
            }
            else
                MessageEnding = "";

            if (idx2 >= 0)
            {
                if (idx1 > 0)
                    Message = CompleteMessage.Substring(idx1 + 1, idx2 - idx1 - 1);
                else if (idx1 == 0)
                    Message = CompleteMessage.Substring(0, idx2+1);
                else
                    Message = CompleteMessage.Substring(0, idx2);
            }
            else
                Message = CompleteMessage;

            // Remove double spaces
            Message = Tokenizer.StringSingleSpace(Message);
            
            UpdateData();
        }

        public override byte[] GetBytes()
        {
            byte[] output = new byte[_length];
            output[0] = (byte)Type;
            output[1] = (byte)fixed1;
            BitConverter.GetBytes(msg_ptr).CopyTo(output, 2);
            BitConverter.GetBytes(MsgId).CopyTo(output, 4);
            BitConverter.GetBytes(VoiceId).CopyTo(output, 6);
            BitConverter.GetBytes((UInt16)SpeakerId).CopyTo(output, 8);

            return output;
        }

        public override string GetMessages()
        {
            if (MessageJp != null)
                return CompleteMessage + MessageJp;
            return CompleteMessage;
        }

        public override byte[] GetMessagesBytes()
        {
            byte[] msg = Tokenizer.StringEncode(CompleteMessage);
            byte[] output = new byte[msg.Length + 1];
            msg.CopyTo(output, 0);

            return output;
        }

        public override int SetMessagePointer(int offset)
        {
            msg_ptr = (UInt16) offset;
            return offset + Tokenizer.StringEncode(CompleteMessage).Length + 1;
        }

        public override void UpdateData()
        {
            string message_spacing = Tokenizer.StringDoubleSpace(Message);

            if (Speaker.Length > 0)
                //CompleteMessage = Speaker + "「" + Message + "」" + MessageEnding;
                CompleteMessage = Speaker + "「" + message_spacing + "」" + MessageEnding;
            else
                CompleteMessage = message_spacing + "" + MessageEnding;

            Data2 = CompleteMessage;
        }

        public override void UpdateGui()
        {
            base.UpdateGui();
            base.AddTextbox("Speaker", "Speaker");
            base.AddRichTextbox("Text (EN)", "Message");
            base.AddRichTextbox("Text (JP)", "MessageJp");
            base.AddTextbox("Terminator", "MessageEnding");
            base.AddTranslationButton("Translation", "MessageJp");
            base.AddSpacer();
            base.AddUint16("Msg ID", "MsgId");
            base.AddUint16("Voice ID", "VoiceId");
            base.AddCombobox<MsgSpeaker>("Speaker ID", "SpeakerId");
            
            
        }
    }

    public enum MsgSpeaker
    {
        Narrator = 0x00,
        Kokoro = 0x01,
        Satoru = 0x02,
        Mayuzumi = 0x03,
        Yomogi = 0x04,
        Utsumi = 0x05,
        Inubushi = 0x06,
        Yuni = 0x07,
        Enomoto = 0x09,
        Sayaka = 0x0a,
        Yomogi_Mayuzumi = 0xc4,
        Kokoro_Yuni_Mayuzumi = 0x70c1,
        Yuni_Yomogi_Mayuzumi = 0x3107,
    }
}
