using System;
using System.Collections.Generic;
using System.Text;

using System.Dynamic;
using System.Threading;
using System.Threading.Tasks;

using System.Reflection;
using System.Reflection.Emit;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections;


namespace SmallDocumentBase
{
    public partial class Engine
    {
        //List<InternalDocument> lst_docs_to_save = new List<InternalDocument>();


        public bool open(string storage_name, params string[] parameters)
        {
            bool bool_ret = false;

            _Globals.storage_name_index = storage_name + ".sdbi";
            _Globals.storage_name_docs = storage_name + ".sdbn"; //folder where all files are
            _Globals._io.parseparams(parameters); //parse params
            bool_ret = _Globals._io.init(); //init storage

            return bool_ret;
        }

        public void close()
        {
            _Globals._io.finalize(); //close storage
        }



        public bool set(Document document)
        {
            bool bool_ret = false;
            //set virtual length
            if (_Globals.storage_virtual_length == 0)
            { _Globals.storage_virtual_length = _Globals._io.get_stream_docs_length(); }
            //start async
            Task<InternalDocument> task_doc = _add_async(document);
            task_doc.Wait();
            //get result
            if (task_doc.Result != null)
            { _Globals.lst_docs_to_save.Add(task_doc.Result); bool_ret = true; }
            //result
            return bool_ret;
        }
        //async creation
        private async Task<InternalDocument> _add_async(Document document)
        {
            bool bool_has_dict = false;
            long l_doc_pos = _Globals.storage_virtual_length, l_tag_pos = 0;
            int i = 0,i_tag_data_len=0;
            ulong u_hash=0;
            //ulong uhash_col = 0;

            //tags
            if (document != null)
            {
                //Type _type = document.GetType();
                //FieldInfo[] fields = _type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                byte[] temp_bytes;
                /*FieldInfo fieldInfo;
                for (i = 0; i < fields.Length; i++) //go thru all fields
                {
                    fieldInfo = fields[i];
                    string dict_name = fieldInfo.Name;
                    if (dict_name == "dict") { bool_has_dict = true; break; } //if not appropriate dictionary inside
                }/**/

                //if it's not a true Document class - exit
                //if (bool_has_dict == false) { return null; }

                InternalDocument _doc = new InternalDocument();
                l_tag_pos = _doc.doc_header_length; //set first tag_pos
                _doc.doc_created = DateTime.Now.Ticks;
                _doc.doc_changed = DateTime.Now.Ticks;
                _doc.doc_id = _Globals.storage_document_id;
                _doc.doc_position = _Globals.storage_virtual_length;
                foreach (KeyValuePair<string, object> _kv in document)// document_dictionary)//document.dict) //go thru all fields
                {
                    if (_kv.Key.Length < _Globals.storage_tag_max_len) //get info if field's name not more than it's possible max value
                    {
                        if (_kv.Key.Length > 0)
                        {
                            if (_kv.Key.Length > _Globals.storage_tag_max_len) { _doc = null; break; }
                            
                            u_hash=_Globals._hash.CreateHash64bit(Encoding.ASCII.GetBytes(_kv.Key));
                            _doc.tag_hash.Add(u_hash, _kv.Key); //get/set tags
                            _doc.lst_tag_hash.Add(u_hash);

                            _doc.tag_data_pos.Add(l_tag_pos); //add tag_pos NOTE pos = tag_hash + tag_data

                            _doc.tag_data_type.Add(_Globals._datatypeserializer.returnTypeAndRawByteArray(_kv.Value, out temp_bytes));

                            i_tag_data_len = temp_bytes.Length;
                            _doc.tag_data_len.Add(i_tag_data_len);
                            _doc.tag_data.Add(temp_bytes); //data byte array
                            _doc._tag_data_length += i_tag_data_len; //sum of all data length

                            l_tag_pos += (8 + i_tag_data_len);
                        }
                    }
                }//foreach
                //increase virtual file length
                _Globals.lst_docs_to_save.Add(_doc);
                //_Globals.lst_docs_to_save_BYTES.Add(_doc.get_data_bytes());
                _Globals.storage_virtual_length += _doc.getlength();
                //increase document id
                _Globals.storage_document_id++;
                //result
                return await Task.FromResult(_doc);
            }
            return null;
        }



        public bool commit()
        {
            if (_Globals.lst_docs_to_save.Count == 0) { return false; } //nothing to save

            Task<bool> task_commit = _commit_async();
            task_commit.Wait();
            //flush
            _Globals.storage_virtual_length = 0;
            _Globals.lst_docs_to_save.Clear();
            // result
            if (task_commit.Result == false)
            { return false; }
            else
            { return true; }
        }

        //async commit
        private async Task<bool> _commit_async()
        {
            bool bool_ret = false;
            int icount = _Globals.lst_docs_to_save.Count;

            if (icount == 0) { return false; }

            //default lists
            List<byte[]> lst_tags_indexes = new List<byte[]>(10);
            List<byte[]> lst_docs = new List<byte[]>(10);


            try
            {
                //go thru all docs
                for (int i = 0; i < _Globals.lst_docs_to_save.Count; i++)
                {
                    lst_tags_indexes.Add(_Globals.lst_docs_to_save[i].get_indexes_bytes());
                    lst_docs.Add(_Globals.lst_docs_to_save[i].get_data_bytes());
                }//for
                byte[] b_tags_indexes = _Globals._service.ListOfByteArraysToByteArray(ref lst_tags_indexes);
                byte[] b_docs = _Globals._service.ListOfByteArraysToByteArray(ref lst_docs);//_Globals.lst_docs_to_save_BYTES);
                lst_docs.Clear();

                //write
                bool_ret = _Globals._io.storageisopen();
                if (bool_ret == false)
                { bool_ret = _Globals._io.init(); } //try to reopen storage

                _Globals._io.write_index(ref b_tags_indexes);
                _Globals._io.write_docs(ref b_docs);
            }
            catch (Exception e) { return false; }

            //flush
            _Globals.lst_docs_to_save.Clear();
            _Globals.lst_docs_to_save_BYTES.Clear();

            //result
            return await Task.FromResult(bool_ret);
        }

        public List<Document> get(string query_string)
        {
            //parse query string
            if (_Globals._query.parse_query(query_string) == false) { return new List<Document>(); }

            //search for documents
            Task<List<Document>> task_get = _Globals._io.search_in_storage_async(query_string);
            task_get.Wait();
            //result
            if (task_get.Result == null)
            { return new List<Document>(); }
            else
            { return task_get.Result; }
        }

    }
}
