using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R11_Script_Editor
{
    class Init_bin
    {
        /*
        File structure:

        Headers:
        Offset - Table description
        00 - ???
        04 - ???
        08 - section names
        0c - character names
        10 - persistent variables
        14 - semi-persistent variales? (carry overs between routes like last bad end you did?)
        18 - same???
        1c - more variables
        20 - script names - files 0###
        24 - BG images    - files 1###
        28 - CGs          - files 2###
        2c - Sprites      - files 3###
        30 - BGM          - files 4###
        34 - SFX          - files 5###
        38 - Endings
        3c - ??? - length 0x44 = 68
        40 - ??? - length 0x1e7 = 487
        44 - ??? - length 0x1d5 = 469
        48 - ??? - length 0x27c = 636
        4c - ??? - length 0x40 = 64
        50 - ??? - length 0x30 = 48
        54 - null data ? - length 0x02 = 2
        58 - ??? - length = 0x156 = 342
        5c - promo material details
        60 - jukebox data
        64 - lyrics
        68 - tips
        6c - tips ordering
        70 - tips related?
        74 - ??? - length 0x76b = 1899
        78 - BGM name (in-game?)
        */

        /*
        

        */
        class InitTable
        {
            protected byte[] _header;
            protected byte[] _content;

            public virtual byte[] GetBytes() { return null; }

            protected byte[] DwordToByteArray(UInt32[] dw)
            {
                byte[] output = new byte[dw.Length*4];
                int offset = 0;
                foreach (UInt32 x in dw)
                {
                    BitConverter.GetBytes(x).CopyTo(output, offset);
                    offset += 4;
                }
                return output;
            }

            protected void 

            protected byte[] StringToHalfWidthAlpha(string input)
            {
                // Convert alphanumeric text to half-width roman letters
                // These halfwidth characters are in an unused protion of the shift JIS table
                // Used mosely for scene titles to cram as much data as possible

                byte[] output = new byte[input.Length * 2];

                int offset = 0;
                foreach(char c in input)
                {
                    // Half width alphanumeric
                    if (c >=0x21 && c <= 0x5f)
                    {
                        output[offset] = 0x85;
                        output[offset + 1] = (byte)(c + 0x1f);
                    }
                    else if (c > 0x5f && c <= 0x7e)
                    {
                        output[offset] = 0x85;
                        output[offset + 1] = (byte)(c + 0x20);
                    }
                    offset += 2;
                }

                /*
                Halfwidth
                85 6F - P
                85 92 - r
                85 8F - o
                85 8C - l
                85 8F - o
                85 87 - g
                85 95 - u
                85 85 - e
                00 90 90 90 - Stop
                */

                /*
                map_hw (0x8143, ",. :;?!");
                map_hw (0x817c, "-");
                map_hw (0x8140, " ");
                map_hw (0x8169, "()");
                map_hw (0x816D, "[]{}");
                map_hw (0x824f, "0123456789");
                map_hw (0x8260, join ("", map { chr ($_) } ord ("A")..ord("Z")));
                map_hw (0x8281, join ("", map { chr ($_) } ord ("a")..ord("z")));

                local @ARGV = ('cp932.txt');

                my $d = '[0-9A-F]';
                my $d2 = "0x(${d}${d})";
                my $d4 = "0x(${d}${d}${d}${d})";

                while (<>) {
	            $_ =~ s/#.*;
                            next if ($_ = ~ /^\s *$/);

                            if ($_ = ~ /^$d2\s +$d4\s$/ o) {
	                $single_bytes{ (pack('c', hex($1)))} = chr(hex($2));
	                $reverse_lookup{ (chr(hex($2)))} = pack('c', hex($1));
                            }
                            elsif($_ = ~ /^$d4\s +$d4\s$/ o) {
	                $double_bytes{ (pack('n', hex($1)))} = chr(hex($2));
	                $reverse_lookup{ (chr(hex($2)))} = pack('n', hex($1));
                            }
                            elsif($_ = ~ /^$d2\s *$/ o) {
                            } else {
                                print "Bad line '$_'\n";
                            }
                        }
                $single_bytes{('"')} = '\"';
                $double_bytes{("\x87S")} = '\x01';
                $double_bytes{("\x87T")} = '\x02';
                $double_bytes{("\x87U")} = '\x03';
                #$double_bytes{("\x81\x7c")} = '-';
                $double_bytes{("\x81\x7c")} = chr(0x2212);
                #$double_bytes{("\x81\x7c")} = "**";
                $reverse_lookup{("\x01")} = "\x87S";
                $reverse_lookup{("\x02")} = "\x87T";
                $reverse_lookup{("\x03")} = "\x87U";

                #$reverse_lookup{(chr(0x2212))} = "-";
                #$reverse_lookup{(chr(0x2014))} = "-";
                $reverse_lookup{(chr(0x2212))} = "\x81\x7c";
                $reverse_lookup{(chr(0x2014))} = "\x81\x7c";
                $reverse_lookup{(chr(0x301c))} = "~";

                #$reverse_lookup{(chr(0x2160))} = "\x87\x54"; # Roman I
                #$reverse_lookup{(chr(0x2161))} = "\x87\x55"; # Roman II
                #$reverse_lookup{(chr(0x2162))} = "\x87\x56"; # Roman III
                #$reverse_lookup{(chr(0x2163))} = "\x87\x57"; # Roman IV
                #$double_bytes {"\x87\x54"} = chr(0x2160);
                for (my $i = 0; $i< 4; ++ $i) { # Roman ($i + 1)
	            my $str = "\x87".chr(0x54+$i);
	            $reverse_lookup{(chr(0x2160+$i))} = $str;
	            $double_bytes{$str} = chr(0x2160+$i);
                }


                $double_bytes{("\x86\x40")} = "\xf6"; # o umlaut
                #$double_bytes{("\x86\x40")} = chr(0xfffd); # o umlaut
                $reverse_lookup{("\xf6")} = "\x86\x40";
                $reverse_lookup{(chr(0xfffd))} = "\x86\x40";
                $double_bytes{("\x86\x43")} = "\xef"; # i umlaut
                $reverse_lookup{("\xef")} = "\x86\x43";
                $double_bytes{("\x86\x44")} = "\xe9"; # e-accent
                #$reverse_lookup{("\xc3")} = "\x86\x44";
                $reverse_lookup{("\xe9")} = "\x86\x44";
                $double_bytes{("\x86\x41")} = " ";
                $pseudo_sjis{" "} = "\x86\x41";

                #$double_bytes(("
                #foreach (keys %single_bytes) {
                #	$reverse_lookup{($single_bytes{$_})} = $_;
                #}
                #foreach (keys %double_bytes) {
                #	$reverse_lookup{($double_bytes{$_})} = $_;
                #}
                for (my $i = 0x21; $i <= 0x7e; ++ $i) {
	            if ($i< 0x4f) {
	                $double_bytes{(pack("CC",0x86, $i + 0x2f))} = "<I>".chr($i)."</I>";
	            } else {
	                $double_bytes{(pack("CC",0x86, $i + 0x30))} = "<I>".chr($i)."</I>";
	}
    }
*/


                return output;
            }
        }

        class InitTable00 : InitTable
        {
            public override byte[] GetBytes()
            {
                return new byte[] { 0x9f, 0x00, 0xa3, 0x00, 0x9b, 0x00, 0x9c, 0x00, 0x9d, 0x00, 0x9e, 0x00 };
            }
        }

        class InitTable04 : InitTable
        {
            public override byte[] GetBytes() 
            {
                return DwordToByteArray(new UInt32[] {
                    0x018D, 0x0006, 0x0000, 0x00A6, 0x0000, 0x0000, 0x0086, 0x0000,
                    0x0000, 0x008A, 0x0000, 0x0000, 0x005E, 0x0005, 0x0000, 0x00C9,
                    0x0004, 0x0000, 0x00BD, 0x0002, 0x0000, 0x0013, 0x0000, 0x0000,
                    0x0103, 0x0004, 0x0000, 0x0044, 0x0002, 0x0000, 0x00FA, 0x0003,
                    0x0000, 0x0116, 0x0000, 0x0000, 0x010F, 0x0002, 0x0000, 0x00CB,
                    0x0002, 0x0000, 0x028F, 0x0004, 0x0000, 0x0260, 0x0005, 0x0000,
                    0x0167, 0x0006, 0x0000, 0x013B, 0x0003, 0x0000, 0x005E, 0x0002,
                    0x0000, 0x0059, 0x0000, 0x0000, 0x00ED, 0x0004, 0x0000, 0x00A6,
                    0x0002, 0x0000, 0x0007, 0x0000, 0x0000, 0x0106, 0x0004, 0x0000,
                    0x01EE, 0x0002, 0x0000, 0x012B, 0x0002, 0x0000, 0x006B, 0x0002,
                    0x0000, 0x0071, 0x0000, 0x0000, 0x017C, 0x0000, 0x0000, 0x00C1,
                    0x0004, 0x0000, 0x00E3, 0x0002, 0x0000, 0x00AE, 0x0000, 0x0000,
                    0x00F8, 0x0002, 0x0000, 0x0149, 0x0004, 0x0000, 0x0076, 0x0002,
                    0x0000, 0x010D, 0x0004, 0x0000, 0x00F7, 0x0002, 0x0000, 0x0168,
                    0x0002, 0x0000, 0x00AD, 0x0004, 0x0000, 0x00B8, 0x0000, 0x0000,
                    0x01BA, 0x0006, 0x0000, 0x0126, 0x0000, 0x0000, 0x0181, 0x0002,
                    0x0000, 0x00C9, 0x0002, 0x0000, 0x024C, 0x0006, 0x0000, 0x005B,
                    0x0000, 0x0000, 0x0173, 0x0002, 0x0000, 0x0044, 0x0000, 0x0000,
                    0x010B, 0x0006, 0x0000, 0x022A, 0x0002, 0x0000, 0x00E9, 0x0002,
                    0x0000, 0x0080, 0x0000, 0x0000, 0x0120, 0x0000, 0x0000, 0x0233,
                    0x0006, 0x0000, 0x012C, 0x0004, 0x0000, 0x00A6, 0x0000, 0x0000,
                    0x0219, 0x0008, 0x0000, 0x0176, 0x0004, 0x0000, 0x005F, 0x0002,
                    0x0000, 0x007C, 0x0000, 0x0000, 0x0077, 0x0000, 0x0000, 0x00A3,
                    0x0000, 0x0000, 0x00A1, 0x0000, 0x0000, 0x0093, 0x0000, 0x0000,
                    0x010A, 0x0000, 0x0000, 0x00C9, 0x0000, 0x0000, 0x0078, 0x0000,
                    0x0000, 0x008D, 0x0002, 0x0000, 0x0241, 0x0004, 0x0000, 0x0120,
                    0x0002, 0x0000, 0x00EC, 0x0000, 0x0000, 0x014F, 0x0000, 0x0000,
                    0x002D, 0x0000, 0x0000, 0x002D, 0x0000, 0x0000, 0x0021, 0x0000,
                    0x0000, 0x00FA, 0x0002, 0x0000, 0x0127, 0x0004, 0x0000, 0x00F3,
                    0x0000, 0x0000, 0x00C0, 0x0002, 0x0000, 0x024D, 0x0008, 0x0000,
                    0x00B9, 0x0000, 0x0000, 0x00D7, 0x0000, 0x0000, 0x009E, 0x0000,
                    0x0000, 0x00D0, 0x0000, 0x0000, 0x0112, 0x0004, 0x0000, 0x00EB,
                    0x0008, 0x0000, 0x00AB, 0x0002, 0x0000, 0x005E, 0x0000, 0x0000,
                    0x00C0, 0x0004, 0x0000, 0x00BF, 0x0002, 0x0000, 0x0058, 0x0000,
                    0x0000, 0x02B1, 0x000C, 0x0000, 0x0070, 0x0000, 0x0000, 0x0065,
                    0x0002, 0x0000, 0x0229, 0x0008, 0x0000, 0x0133, 0x0000, 0x0000,
                    0x008E, 0x0000, 0x0000, 0x001C, 0x0000, 0x0000, 0x003A, 0x0000,
                    0x0000, 0x00A2, 0x0002, 0x0000, 0x0065, 0x0000, 0x0000, 0x00EB,
                    0x0008, 0x0000, 0x013B, 0x0004, 0x0000, 0x006C, 0x0002, 0x0000,
                    0x0062, 0x0000, 0x0000, 0x0074, 0x0000, 0x0000, 0x0094, 0x0007,
                    0x0000, 0x0043, 0x0000, 0x0000, 0x00A1, 0x0000, 0x0000, 0x0044,
                    0x0000, 0x0000, 0x0052, 0x0000, 0x0000, 0x004E, 0x0000, 0x0000,
                    0x02AD, 0x0004, 0x0000, 0x00D7, 0x0000, 0x0000, 0x0129, 0x0004,
                    0x0000, 0x0049, 0x0000, 0x0000, 0x0119, 0x0002, 0x0000, 0x0028,
                    0x0000, 0x0000, 0x0054, 0x0000, 0x0000, 0x00E5, 0x0000, 0x0000,
                    0x0093, 0x0000, 0x0000, 0x0134, 0x0006, 0x0000, 0x00A7, 0x0000,
                    0x0000, 0x0062, 0x0000, 0x0000, 0x00AD, 0x0000, 0x0000, 0x00A3,
                    0x0000, 0x0000, 0x006E, 0x0000, 0x0000, 0x00CC, 0x0000, 0x0000,
                    0x0008, 0x0000, 0x0000, 0x0169, 0x0000, 0x0000, 0x0046, 0x0000,
                    0x0000, 0x0191, 0x0004, 0x0000, 0x012E, 0x0002, 0x0000, 0x0089,
                    0x0000, 0x0000, 0x00AB, 0x0000, 0x0000, 0x00DC, 0x0000, 0x0000,
                    0x008D, 0x0000, 0x0000, 0x0174, 0x0006, 0x0000, 0x007A, 0x0000,
                    0x0000, 0x0079, 0x0000, 0x0000, 0x01D2, 0x0000, 0x0000, 0x01EC,
                    0x000A, 0x0000, 0x00EB, 0x0002, 0x0000, 0x01F8, 0x0002, 0x0000,
                    0x01D2, 0x0000, 0x0000, 0x002F, 0x0000, 0x0000, 0x003C, 0x0000,
                    0x0000, 0x00C0, 0x0000, 0x0000, 0x00ED, 0x0000, 0x0000, 0x012E,
                    0x0000, 0x0000, 0x0123, 0x0002, 0x0000, 0x001A, 0x0000, 0x0000,
                    0x000F, 0x0000, 0x0000, 0x004C, 0x0000, 0x0000, 0x0024, 0x0000,
                    0x0000, 0x0000, 0x0000, 0x0000, 0x018E, 0x000E, 0x0000, 0x0290,
                    0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
                    0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
                    0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
                    0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
                    0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000, 0x0000,
                    0x0000, 0x8BF4, 0x0123, 0x0000, 0xFFFFFFFF
                });
            }
        }

        class InitTable08 : InitTable
        {
            InitTable08()
            {

            }

            public override byte[] GetBytes()
            {


                return new byte[] { 0x9f, 0x00, 0xa3, 0x00, 0x9b, 0x00, 0x9c, 0x00, 0x9d, 0x00, 0x9e, 0x00 };
            }
        }

    }
}
