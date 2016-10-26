using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmallDocumentBase
{
    internal class Query
    {
        private string query;
        private int i_query_len;
        private Dictionary<string, string> dict_levels = new Dictionary<string, string>(10);
        private Dictionary<string, string> dict_strings = new Dictionary<string, string>(10);
        private List<string> lst_parse_out = new List<string>(10);

        internal bool parse_query(string query)
        {
            bool bool_ret = false;

            //for (int i = 0; i < 100000; i++)
            //{
                dict_levels.Clear(); dict_strings.Clear();
                this.query = query;
                this.i_query_len = query.Length;
                //get all strings out
                _strings_out();
            //}
            return bool_ret;
        }

        private void _strings_out()
        {
            int i = 0, ipos = 0, index = 0, istart = 0, iend = 0;
            string s_level = "", s_mod_str = "";
            StringBuilder sb = new StringBuilder();

            while (ipos < i_query_len)
            {
                istart = query.IndexOf('\'', ipos);
                if (istart == -1)
                {
                    sb.Append(query.Substring(ipos, i_query_len - ipos)); //append rest of the query
                    break;
                }
                iend = query.IndexOf('\'', istart + 1);
                if (iend == -1) { break; }
                //add before string
                sb.Append(query.Substring(ipos, istart - ipos));
                //add string replacement
                dict_strings.Add("str" + index, query.Substring(istart + 1, iend - 1 - istart));
                sb.Append("str" + index);
                //offsets
                ipos = iend + 1; index++;
            }

            index = 0;
            s_mod_str = sb.ToString();
            i_query_len = s_mod_str.Length;

            //split
            //for (i = 0; i < s_mod_str.Length; i++)
            while (ipos < i_query_len)
            {
                istart = s_mod_str.LastIndexOf('(');//, 0);
                if (istart == -1) { break; }
                iend = s_mod_str.IndexOf(')', istart + 1);
                if (iend == -1) { break; }
                s_level = s_mod_str.Substring(istart + 1, iend - 1 - istart);
                s_mod_str = s_mod_str.Remove(istart, iend + 1 - istart).Insert(istart, "id_" + index);
                dict_levels.Add("id_" + index, s_level);
                lst_parse_out.Add(s_level);
                index++;
            }//for

            //dict_levels.Add("id_" + index, s_mod_str);
            //index = index;
        }

    }
}
