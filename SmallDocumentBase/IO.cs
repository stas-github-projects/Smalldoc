using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SmallDocumentBase
{
    internal class IO
    {
        FileStream fstream_index;
        FileStream fstream_docs;

        internal long get_stream_index_length()//IO_PARAM param)
        {
            if (fstream_index != null) { return fstream_index.Length; } else { return 0; }
        }
        internal long get_stream_docs_length()//IO_PARAM param)
        {
            if (fstream_docs != null) { return fstream_docs.Length; } else { return 0; }
        }

        internal bool init()//string filename)
        {
            bool bool_ret = true;

            //create dir & files
            try
            {
                FileInfo findex = new FileInfo(_Globals.storage_name_index);
                FileInfo fdocs = new FileInfo(_Globals.storage_name_docs);

                if (findex.Exists == false || fdocs.Exists==false)
                {
                    fstream_index = new FileStream(_Globals.storage_name_index, FileMode.Create, FileAccess.ReadWrite, FileShare.None, _Globals.storage_read_write_buffer);
                    fstream_docs = new FileStream(_Globals.storage_name_docs, FileMode.Create, FileAccess.ReadWrite, FileShare.None, _Globals.storage_read_write_buffer);
                    this.write_index(createheader()); //write default empty header
                }
                else
                {
                    fstream_index = new FileStream(_Globals.storage_name_index, FileMode.Open, FileAccess.ReadWrite, FileShare.None, _Globals.storage_read_write_buffer);
                    fstream_docs = new FileStream(_Globals.storage_name_docs, FileMode.Open, FileAccess.ReadWrite, FileShare.None, _Globals.storage_read_write_buffer);
                }

            }
            catch (Exception) //on error return false
            { return false; }

            return bool_ret;
        }

        internal byte[] createheader()
        {
            int ipos = 0;
            byte[] bout = new byte[_Globals.storage_global_header_len];

            _Globals._service.InsertBytes(ref bout, Encoding.ASCII.GetBytes(_Globals.storage_version), ipos); ipos += 4; //version
            //_Globals._service.InsertBytes(ref bout, BitConverter.GetBytes(_Globals.storage_document_id), ipos); ipos += 4; //document id
            //_Globals._service.InsertBytes(ref bout, BitConverter.GetBytes(_Globals.storage_col_max_len), ipos); ipos++; //max collection size
            //_Globals._service.InsertBytes(ref bout, BitConverter.GetBytes(_Globals.storage_tag_max_len), ipos); ipos++; //max tag size
            //_Globals._service.InsertBytes(ref bout, BitConverter.GetBytes(_Globals.storage_indexes_per_page), ipos); ipos += 2; //indexes per page

            //+ 18*3 - pages headers
            return bout;
        }

        internal bool storageisopen()
        {
            if (fstream_index != null && fstream_docs!=null) { return true; }
            return false;
        }

        internal void parseparams(params string[] parameters)
        {

        }

        internal bool write_index(byte[] barray)
        {
            return write_index(ref barray);
        }
        internal bool write_index(ref byte[] barray)
        {
            bool bool_ret = false;

            if (fstream_index != null)
            {
                fstream_index.Position = fstream_index.Length;
                fstream_index.Write(barray, 0, barray.Length);
                fstream_index.Position = fstream_index.Length;
                bool_ret = true;
            }

            return bool_ret;
        }

        internal bool write_docs(byte[] barray)
        {
            return write_docs(ref barray);
        }
        internal bool write_docs(ref byte[] barray)
        {
            bool bool_ret = false;

            if (fstream_docs != null)
            {
                fstream_docs.Position = fstream_docs.Length;
                fstream_docs.Write(barray, 0, barray.Length);
                fstream_docs.Position = fstream_docs.Length;
                bool_ret = true;
            }

            return bool_ret;
        }

        internal void finalize()
        {
            if (fstream_index != null) { fstream_index.Close(); fstream_index = null; }
            if (fstream_docs != null) { fstream_docs.Close(); fstream_docs = null; }
        }

        internal async Task<List<Engine.Document>> search_in_storage_async(string stag)
        {
            //
            //TEST
            //
            //return null;
            //
            //
            //

            int ilen = 0, idoclen = 0, ibuflen = (_Globals.tag_element_size * 40000), icount = 0, ipos = 0;
            long lpos = 0, ifilelen = fstream_index.Length;
            byte[] buf = new byte[ibuflen];
            List<Engine.Document> lst_out = new List<Engine.Document>();

            //Console.Title = "Elements count =  " + (ifilelen / _Globals.tag_element_size)/6;

            //tag hash
            ulong u_given_tag = _Globals._hash.CreateHash64bit(Encoding.ASCII.GetBytes(stag));
            byte[] b_given_hash = BitConverter.GetBytes(u_given_tag);
            List<int> lst = new List<int>();
            //vars
            byte btype = 0;
            byte[] b_hash = new byte[8];
            int doc_id = 0,data_len=0;
            long doc_pos = 0,data_pos=0;
            //ulong u_hash = 0;

            while (lpos < ifilelen)
            {
                fstream_index.Position = lpos;
                ilen = fstream_index.Read(buf, 0, ibuflen);
                if (ilen == 0) { break; }
                lpos += ibuflen;
                ipos=0;

                icount = ilen / _Globals.tag_element_size;
                for (int i = 0; i < icount; i++)
                {
                    if (buf[ipos] == 0) //skip blocked
                    { ipos += _Globals.tag_element_size; }
                    else
                    {
                        ipos++; //data_active
                        //btype = buf[ipos]; ipos++; //data_type
                        ipos += 4; //doc_id
                        idoclen = BitConverter.ToInt32(_Globals._service.GetBytes(buf, ipos, 4), 0); ipos += 4; //doc_hashes_len

                        //u_hash = BitConverter.ToUInt64(_Globals._service.GetBytes(buf, ipos, 8), 0); ipos += 8; //hash //depricated
                        //b_hash = _Globals._service.GetBytes(buf, ipos, 8);

                        //if (u_hash == u_given_tag)
                        if (_Globals._service.CompareHashArrays(b_given_hash, 0, ref buf, ipos) == true)
                        {
                            ipos += 8;
                            //doc_id = BitConverter.ToInt32(_Globals._service.GetBytes(buf, ipos - 4 - 8 - 8, 4), 0);//doc_id
                            //doc_pos = BitConverter.ToInt64(_Globals._service.GetBytes(buf, ipos - 8 - 8, 8), 0); //doc_pos
                            //lst.Add(doc_id);
                            //data_len = BitConverter.ToInt32(_Globals._service.GetBytes(buf, ipos, 4), 0);
                            ipos += 4; //data_len
                            //data_pos = BitConverter.ToInt64(_Globals._service.GetBytes(buf, ipos, 8), 0); 
                            ipos += 8; //data_pos
                        }
                        else
                        { ipos += 8+4 + 8; } //skip the remains
                    }
                }
            }

            return await Task.FromResult(lst_out);
        }

    }
}
