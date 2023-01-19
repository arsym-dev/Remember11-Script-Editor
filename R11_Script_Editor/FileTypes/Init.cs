using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R11_Script_Editor.FileTypes
{
    class Init
    {
        /*
        * AFS file chunks are aligned on 2048byte (0x800) boundaries.
        * 
        * ======File header=====
        * 
        * Offset  Len  Description
        *      0    4  AFS/0 magic word
        *      4    4  Num of files (int, 0 <= x <= 0x0FFFFF
        *  8*i+8    4  Offset to file 'i'
        *  8*i+12   4  Length of file 'i'
        *  ...
        *  chunk_end-8    4  Offset to metadata
        *  chunk_end-4    4  Length of metadata block
        *  ------------------------------------
        *  y*0x800  START OF FILES (header has length of 0x800)
        *  
        *  
        *  ===== Filename Block =====
        *  Each entry has the following:
        *  
        *  Offset  Len  Description
        *       0   32  Filename (string, null terminated, 32 char max)
        *     0x20   2  Year
        *     0x22   2  Month
        *     0x24   2  Day
        *     0x26   2  Hour
        *     0x28   2  Minute
        *     0x2A   2  Second
        *     0x2C   4  File size
        */














        static FileStream fstream;
        static public byte[] Pack(string list_file, string folder)
        {
            /*
             * AFS file chunks are aligned on 2048byte (0x800) boundaries.
             * 
             * ======File header=====
             * 
             * Offset  Len  Description
             *      0    4  AFS/0 magic word
             *      4    4  Num of files (int, 0 <= x <= 0x0FFFFF
             *  8*i+8    4  Offset to file 'i'
             *  8*i+12   4  Length of file 'i'
             *  ...
             *  chunk_end-8    4  Offset to metadata
             *  chunk_end-4    4  Length of metadata block
             *  ------------------------------------
             *  y*0x800  START OF FILES (header has length of 0x800)
             *  
             *  
             *  ===== Filename Block =====
             *  Each entry has the following:
             *  
             *  Offset  Len  Description
             *       0   32  Filename (string, null terminated, 32 char max)
             *     0x20   2  Year
             *     0x22   2  Month
             *     0x24   2  Day
             *     0x26   2  Hour
             *     0x28   2  Minute
             *     0x2A   2  Second
             *     0x2C   4  File size
             */

            string[] fnames = File.ReadAllLines(Path.Combine(folder, list_file));
            foreach (var fname in fnames)
                if (fname.Length > 0x20)
                    Console.WriteLine("Filename " + fname + " is too long. Shorten to 32 characters.");

            var chunks = new List<byte[]>();
            var chunk_sizes = new List<int>();

            foreach (var fname in fnames)
                chunks.Add(File.ReadAllBytes(Path.Combine(folder, fname)));

            // Determine size to allocate for header
            int header_length = 8 + chunks.Count * 8 + 4;
            chunk_sizes.Add(AlignChunk(header_length));

            // Determine size to allocate for chunks
            foreach (var c in chunks)
                chunk_sizes.Add(AlignChunk(c));

            // Determine size to allocate for metadata
            int meta_length = chunks.Count * 0x30;
            chunk_sizes.Add(AlignChunk(meta_length));

            // Create our buffer
            int total_size = 0;
            foreach (var sz in chunk_sizes)
                total_size += sz;
            byte[] outfile = new byte[total_size];
            int pos = 0;


            // Create the header copy in the chunks
            Encoding.GetEncoding("UTF-8").GetBytes("AFS\0").CopyTo(outfile, 0);
            BitConverter.GetBytes(chunks.Count).CopyTo(outfile, 4);

            for (int i = 0; i < chunks.Count; i++)
            {
                pos += chunk_sizes[i];
                var c = chunks[i];

                BitConverter.GetBytes(pos).CopyTo(outfile, i * 8 + 8);
                BitConverter.GetBytes(c.Length).CopyTo(outfile, i * 8 + 12);
                Buffer.BlockCopy(c, 0, outfile, pos, c.Length);
            }
            pos += chunk_sizes[chunk_sizes.Count - 2];

            // Add the metadata table
            // (Append to header)
            BitConverter.GetBytes(pos).CopyTo(outfile, chunk_sizes[0] - 8);
            BitConverter.GetBytes(meta_length).CopyTo(outfile, chunk_sizes[0] - 4);

            // (Create each entry in the actual table)
            for (int i = 0; i < chunks.Count; i++)
            {
                var fname = fnames[i];
                var d = File.GetLastWriteTime(Path.Combine(folder, fname));

                Encoding.GetEncoding("UTF-8").GetBytes(fname).CopyTo(outfile, pos + i * 0x30);
                BitConverter.GetBytes((UInt16)d.Year).CopyTo(outfile, pos + i * 0x30 + 0x20);
                BitConverter.GetBytes((UInt16)d.Month).CopyTo(outfile, pos + i * 0x30 + 0x22);
                BitConverter.GetBytes((UInt16)d.Day).CopyTo(outfile, pos + i * 0x30 + 0x24);
                BitConverter.GetBytes((UInt16)d.Hour).CopyTo(outfile, pos + i * 0x30 + 0x26);
                BitConverter.GetBytes((UInt16)d.Minute).CopyTo(outfile, pos + i * 0x30 + 0x28);
                BitConverter.GetBytes((UInt16)d.Second).CopyTo(outfile, pos + i * 0x30 + 0x2A);
                BitConverter.GetBytes(chunks[i].Length).CopyTo(outfile, pos + i * 0x30 + 0x2C);
            }

            return outfile;
        }

        static int AlignChunk(byte[] chunk)
        {
            return AlignChunk(chunk.Length);
        }

        static int AlignChunk(int len)
        {
            // Align all of our chunks to 0x800 sized sections
            return (int)Math.Ceiling(len / 2048.0) * 2048;
        }
    }
}
