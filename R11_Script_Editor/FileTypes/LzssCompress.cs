using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R11_Script_Editor.FileTypes
{
    // modified by Luigi Auriemma, ported to C# by Arsym
    /**************************************************************
        LZSS.C -- A Data Compression Program
        (tab = 4 spaces)
    ***************************************************************
        4/6/1989 Haruhiko Okumura
        Use, distribute, and modify this program freely.
        Please send me your improved versions.
            PC-VAN		SCIENCE
            NIFTY-Serve	PAF01022
            CompuServe	74050,1022
    **************************************************************/

    class LzssCompress
    {
        // Compresses using LZSS0 (variant of LZSS that replaces int_chr with a 0x00 rather than space)
        int EI = 12, EJ = 4, P = 2, rless = 2;
        int init_chr = '\0';

        int N, F, THRESHOLD;
        int NIL; // Same as N; /* index for root of binary search trees */
        

        uint
            textsize = 0,   /* text size counter */
            codesize = 0;   /* code size counter */
        static byte[] text_buf;  /* ring buffer of size N,
											with extra F-1 bytes to facilitate string comparison */
        int match_position, match_length;  /* of longest match.  These are
											   set by the InsertNode() procedure. */
        static int[] lson, rson, dad;
        int in_pos = 0, out_pos = 0;

        /* left & right children &
		parents -- These constitute binary search trees. */

		static byte[] infile, outfile;

        public byte[] Compress(byte[] input)
        {
            N = 1 << EI;    /* size of ring buffer */
            F = (1 << EJ) + P;  /* upper limit for match_length */
            THRESHOLD = P;   /* encode string into position and length
							 if match_length is greater than this */


            infile = input;
            outfile = new byte[infile.Length*2];

            NIL = N;
            text_buf = new byte[N + F - 1];
            lson = new int[N + 1];
            rson = new int[N + 257];
            dad = new int[N + 1];

            Encode();
            byte[] output = new byte[out_pos+4];
            BitConverter.GetBytes((UInt32)infile.Length).CopyTo(output,0); // First 4 bytes are the output size
            Buffer.BlockCopy(outfile, 0, output, 4, out_pos); //Then copy the rest of the file
            return (output);
        }

        void Encode()
        {
            int i, r, s, last_match_length, code_buf_ptr;
            uint len;
            byte c;
            byte[] code_buf = new byte[33];
            byte mask;

            InitTree();  /* initialize trees */
            code_buf[0] = 0;  /* code_buf[1..16] saves eight units of code, and
					  code_buf[0] works as eight flags, "1" representing that the unit
					  is an unencoded letter (1 byte), "0" a position-and-length pair
					  (2 bytes).  Thus, eight units require at most 16 bytes of code. */
            code_buf_ptr = mask = 1;
            s = 0;
            r = N - F;
            //for (i = s; i < r; i++) text_buf[i] = init_chr;  /* Clear the buffer with
            //	any character that will appear often. */
            for (int ix = 0; ix < text_buf.Length; ix++)
                text_buf[ix] = 0;

            for (len = 0; len < F; len++)
            {
                try { c = lzss_xgetc(); } catch { break; }
                text_buf[r + len] = c;  /* Read F bytes into the last F bytes of the buffer */
            }
                
            if ((textsize = len) == 0) return;  /* text of size zero */
            for (i = 1; i <= F; i++) InsertNode(r - i);  /* Insert the F strings,
												 each of which begins with one or more 'space' characters.  Note
												 the order in which these strings are inserted.  This way,
												 degenerate trees will be less likely to occur. */
            InsertNode(r);  /* Finally, insert the whole string just read.  The
					global variables match_length and match_position are set. */
            do
            {
                if (match_length > len)
                    match_length = (int) len;  /* match_length
													 may be spuriously long near the end of text. */
                if (match_length <= THRESHOLD)
                {
                    match_length = 1;  /* Not long enough match.  Send one byte. */
                    code_buf[0] |= mask;  /* 'send one byte' flag */
                    code_buf[code_buf_ptr++] = text_buf[r];  /* Send uncoded. */
                }
                else
                {
                    code_buf[code_buf_ptr++] = (byte)match_position;
                    code_buf[code_buf_ptr++] = (byte)
				(((match_position >> (8 - EJ)) & ~((1 << EJ) - 1))
                    | (match_length - (THRESHOLD + 1)));  /* Send position and
														  length pair. Note match_length > THRESHOLD. */
                }
                if ((mask <<= 1) == 0)
                {  /* Shift mask left one bit. */
                    for (i = 0; i < code_buf_ptr; i++)  /* Send at most 8 units of */
                        lzss_xputc(code_buf[i]);     /* code together */
                    codesize += (uint)code_buf_ptr;
                    code_buf[0] = 0; code_buf_ptr = mask = 1;
                }
                last_match_length = match_length;
                for (i = 0; i < last_match_length; i++)
                {
                    try { c = lzss_xgetc(); } catch { break; }

                    DeleteNode(s);      /* Delete old strings and */
                    text_buf[s] = c;    /* read new bytes */
                    if (s < F - 1) text_buf[s + N] = c;  /* If the position is
												 near the end of buffer, extend the buffer to make
												 string comparison easier. */
                    s = (s + 1) & (N - 1); r = (r + 1) & (N - 1);
                    /* Since this is a ring buffer, increment the position
                    modulo N. */
                    InsertNode(r);  /* Register the string in text_buf[r..r+F-1] */
                }
                while (i++ < last_match_length)
                {   /* After the end of text, */
                    DeleteNode(s);                  /* no need to read, but */
                    s = (s + 1) & (N - 1); r = (r + 1) & (N - 1);

                    len--;
                    if (len > 0)
                        InsertNode(r);       /* buffer may not be empty. */
                }
            } while (len > 0);  /* until length of string to be processed is zero */
            if (code_buf_ptr > 1)
            {       /* Send remaining code. */
                for (i = 0; i < code_buf_ptr; i++)
                    lzss_xputc(code_buf[i]);

                codesize += (uint)code_buf_ptr;
            }
        }

        byte lzss_xgetc()
        {
            if (in_pos >= infile.Length)
                throw new Exception("End of file");

            return (infile[in_pos++]);
        }
        byte lzss_xputc(byte chr)
        {
            if (out_pos >= outfile.Length)
                throw new Exception("End of file");
            outfile[out_pos++] = chr;
            return chr;
        }

        void InitTree()  /* initialize trees */
        {
            int i;

            /* For i = 0 to N - 1, rson[i] and lson[i] will be the right and
            left children of node i.  These nodes need not be initialized.
            Also, dad[i] is the parent of node i.  These are initialized to
            NIL (= N), which stands for 'not used.'
            For i = 0 to 255, rson[N + i + 1] is the root of the tree
            for strings that begin with character i.  These are initialized
            to NIL.  Note there are 256 trees. */

            for (i = N + 1; i <= N + 256; i++) rson[i] = NIL;
            for (i = 0; i < N; i++) dad[i] = NIL;
        }

        void InsertNode(int r)
        /* Inserts string of length F, text_buf[r..r+F-1], into one of the
        trees (text_buf[r]'th tree) and returns the longest-match position
        and length via the global variables match_position and match_length.
        If match_length = F, then removes the old node in favor of the new
        one, because the old one will be deleted sooner.
        Note r plays double role, as tree node and position in buffer. */
        {
            int i, p, cmp;

            cmp = 1;
            p = N + 1 + text_buf[r];

            rson[r] = lson[r] = NIL; match_length = 0;
            for (; ; )
            {
                if (cmp >= 0)
                {
                    if (rson[p] != NIL) p = rson[p];
                    else { rson[p] = r; dad[r] = p; return; }
                }
                else
                {
                    if (lson[p] != NIL)p = lson[p];
                    else { lson[p] = r; dad[r] = p; return; }
                }
                for (i = 1; i < F; i++)
                    if ((cmp = text_buf[r+i] - text_buf[p + i]) != 0) break;
                if (i > match_length)
                {
                    match_position = p;
                    if ((match_length = i) >= F) break;
                }
            }
            dad[r] = dad[p]; lson[r] = lson[p]; rson[r] = rson[p];
            dad[lson[p]] = r; dad[rson[p]] = r;
            if (rson[dad[p]] == p) rson[dad[p]] = r;
            else lson[dad[p]] = r;
            dad[p] = NIL;  /* remove p */
        }

        void DeleteNode(int p)  /* deletes node p from tree */
        {
            int q;

            if (dad[p] == NIL) return;  /* not in tree */
            if (rson[p] == NIL) q = lson[p];
            else if (lson[p] == NIL) q = rson[p];
            else
            {
                q = lson[p];
                if (rson[q] != NIL)
                {
                    do { q = rson[q]; } while (rson[q] != NIL);
                    rson[dad[q]] = lson[q]; dad[lson[q]] = dad[q];
                    lson[q] = lson[p]; dad[lson[p]] = q;
                }
                rson[q] = rson[p]; dad[rson[p]] = q;
            }
            dad[q] = dad[p];
            if (rson[dad[p]] == p) rson[dad[p]] = q; else lson[dad[p]] = q;
            dad[p] = NIL;
        }
    }
}
