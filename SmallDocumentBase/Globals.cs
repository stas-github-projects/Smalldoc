using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SmallDocumentBase
{
    internal static class _Globals
    {
        internal static DataTypeSerializer _datatypeserializer = new DataTypeSerializer();
        internal static HashFNV _hash = new HashFNV();
        internal static Service _service = new Service();
        internal static Query _query = new Query();
        internal static IO _io = new IO();

        internal static string storage_name_index = "";
        internal static string storage_name_docs = "";

        internal static char[] storage_version = new char[] { 'S', 'D', 'S', '1' };
        internal static int storage_document_id = 0;
        internal static byte storage_tag_max_len = 36;

        //constants
        internal static long storage_service_document_pos = 0; //where all preferences are stores
        internal static int storage_global_header_len = 4; //14 + (8 + 8 + 2) * 2;
        internal static int tag_element_size = 1;//34;
        internal static int storage_read_write_buffer = 1024 * 1024;

        //for save documents
        internal static long storage_virtual_length = 0;
        internal static List<InternalDocument> lst_docs_to_save = new List<InternalDocument>();
        internal static List<byte[]> lst_docs_to_save_BYTES = new List<byte[]>();
    }
}
