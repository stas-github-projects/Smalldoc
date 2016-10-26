using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SmallDocumentBase
{
    public class Service
    {
        public bool CompareArrays(byte[] _src, byte[] _compare_with)
        {
            return CompareArrays(ref _src, ref _compare_with);
        }
        public bool CompareArrays(ref byte[] _src, ref byte[] _compare_with)
        {
            int ilen1 = _src.Length, ilen2 = _compare_with.Length;
            if (ilen1 != ilen2) { return false; } //arrays are not equal
            for (int i = 0; i < ilen1; i++)
            {
                if (_src[i] != _compare_with[i])
                { return false; } //not equal
            }
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CompareHashArrays(byte[] _src, int src_start_pos, ref byte[] _compare_with, int compare_start_pos)
        {
            if (_src[src_start_pos] != _compare_with[compare_start_pos]) { return false; }
            if (_src[src_start_pos + 1] != _compare_with[compare_start_pos + 1]) { return false; }
            if (_src[src_start_pos + 2] != _compare_with[compare_start_pos + 2]) { return false; }
            if (_src[src_start_pos + 3] != _compare_with[compare_start_pos + 3]) { return false; }
            if (_src[src_start_pos + 4] != _compare_with[compare_start_pos + 4]) { return false; }
            if (_src[src_start_pos + 5] != _compare_with[compare_start_pos + 5]) { return false; }
            if (_src[src_start_pos + 6] != _compare_with[compare_start_pos + 6]) { return false; }
            if (_src[src_start_pos + 7] != _compare_with[compare_start_pos + 7]) { return false; }
            return true;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool CompareHashArraysEXT(byte[] _src, int src_start_pos, ref byte[] _compare_with, int compare_start_pos)
        {
            if (_src[src_start_pos] != _compare_with[compare_start_pos]) { return false; }
            if (_src[src_start_pos + 1] != _compare_with[compare_start_pos + 1]) { return false; }
            if (_src[src_start_pos + 2] != _compare_with[compare_start_pos + 2]) { return false; }
            if (_src[src_start_pos + 3] != _compare_with[compare_start_pos + 3]) { return false; }
            if (_src[src_start_pos + 4] != _compare_with[compare_start_pos + 4]) { return false; }
            if (_src[src_start_pos + 5] != _compare_with[compare_start_pos + 5]) { return false; }
            if (_src[src_start_pos + 6] != _compare_with[compare_start_pos + 6]) { return false; }
            if (_src[src_start_pos + 7] != _compare_with[compare_start_pos + 7]) { return false; }
            return true;
        }

        public void InsertBytes(ref byte[] _src, byte[] _what, int _pos = 0, int _length = 0)
        {
            InsertBytes(ref _src, ref _what, _pos, _length);
        }
        public void InsertBytes(ref byte[] _src, byte _what, int _pos = 0, int _length = 0)
        {
            int i = _src.Length;
            if (_pos < 0) { _pos = 0; }
            if (_length == 0) { _length = 1; }// _what.Length; }
            if (_pos + _length > i) { return; }//out of dimensions
            //Buffer.BlockCopy(_what, 0, _src, _pos, _length);
            _src[_pos] = _what;
        }
        public void InsertBytes(ref byte[] _src, ref byte[] _what, int _pos = 0, int _length = 0)
        {
            int i = _src.Length;
            if (_pos < 0) { _pos = 0; }
            if (_length == 0) { _length = _what.Length; }
            if (_pos + _length > i) { return; }//out of dimensions
            Buffer.BlockCopy(_what, 0, _src, _pos, _length);
        }
        public byte[] GetBytes(byte[] _src, int _pos = 0, int _length = 0)
        {
            byte[] b_out;
            int ilen = _src.Length;
            if (_length < 1) { _length = ilen; }
            if (_pos < 0) { _pos = 0; }
            if (ilen >= (_pos + _length))
            {
                b_out = new byte[_length];
                Buffer.BlockCopy(_src, _pos, b_out, 0, _length);//copy piece
                return b_out;
            }
            else
            { return _src; }
        }
        public byte GetByte(byte[] _src, int _pos = 0)
        {
            byte b_out;
            int ilen = _src.Length, _length = 1;
            if (_length < 1) { _length = ilen; }
            if (_pos < 0) { _pos = 0; }
            if (ilen >= (_pos + _length))
            {
                b_out = _src[_pos];// Buffer.BlockCopy(_src, _pos, b_out, 0, _length);//copy piece
                return b_out;
            }
            else
            { return 0; }
        }

        public string GetStringWONulls(byte[] _in_bytes)
        {
            return GetStringWONulls(Encoding.ASCII.GetString(_in_bytes));
        }
        public string GetStringWONulls(string _in)
        {
            string _out = "";
            int istart = _in.IndexOf('\0');
            if (istart > -1)
            { _out = _in.Substring(0, istart); return _out; }
            else
            { return _in; }
        }

        public byte[] ListOfByteArraysToByteArray(List<byte[]> lst_in)
        {
            return ListOfByteArraysToByteArray(ref lst_in);
        }
        public byte[] ListOfByteArraysToByteArray(ref List<byte[]> lst_in)
        {
            byte[] b_out = new byte[0];
            int i = 0, ibuflen = 0, icount = lst_in.Count, ipos = 0;

            //count
            for (i = 0; i < icount; i++)
            { ibuflen += lst_in[i].Length; }
            //buffer
            b_out = new byte[ibuflen];
            //proceed
            for (i = 0; i < icount; i++)
            {
                Buffer.BlockCopy(lst_in[i], 0, b_out, ipos, lst_in[i].Length); ipos += lst_in[i].Length;
            }

            return b_out;
        }

        /*
        internal byte[] ListDocsToArray(ref List<InternalDocument> lst_docs)
        {
            var buffers = new List<byte[]>();
            int i=0,icount=lst_docs.Count,itotal=0;

            //get buffer size
            for (i = 0; i < icount; i++)
            { itotal += (8 + (8 + 4 + 1) * lst_docs[i].tag_hash.Count + lst_docs[i]._tag_data_length); }

            //int totalLength = lst_docs.Sum<byte[]>(buffer => buffer.Length);
            byte[] fullBuffer = new byte[itotal];

            int insertPosition = 0;
            for (i = 0; i < icount;i++ )//byte[] buffer in lst_docs)
            {
                byte[] buffer = lst_docs[i].getbytes(0);// lst_docs[i].getbytes();
                buffer.CopyTo(fullBuffer, insertPosition);
                insertPosition += buffer.Length;
            }
            return fullBuffer;
        }
        */
    }

}
