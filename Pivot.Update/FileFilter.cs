using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pivot.Update
{
    public class FileFilter : IEnumerable<KeyValuePair<string, string>>
    {
        private List<string> m_SourceFiles = new List<string>();
        private Dictionary<string, string> m_FileMappings = new Dictionary<string, string>();

        public FileFilter(IEnumerable<string> filenames)
        {
            foreach (string s in filenames)
                this.m_SourceFiles.Add(s);
        }

        public void ApplyInclude(string regex)
        {
            Regex re = new Regex(regex);
            foreach (string s in this.m_SourceFiles)
                if (re.IsMatch(s))
                    this.m_FileMappings.Add(s, s);
        }

        public void ApplyExclude(string regex)
        {
            Regex re = new Regex(regex);
            List<string> toRemove = new List<string>();
            foreach (KeyValuePair<string, string> kv in this.m_FileMappings)
                if (re.IsMatch(kv.Value))
                    toRemove.Add(kv.Key);
            foreach (string s in toRemove)
                this.m_FileMappings.Remove(s);
        }
        
        public void ApplyRewrite(string find, string replace)
        {
            Regex re = new Regex(find);
            Dictionary<string, string> copy = new Dictionary<string, string>(this.m_FileMappings);
            foreach (KeyValuePair<string, string> kv in copy)
                if (re.IsMatch(kv.Value))
                    this.m_FileMappings[kv.Key] = re.Replace(kv.Value, replace);
        }

        #region IEnumerable<KeyValuePair<string,string>> Members

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return this.m_FileMappings.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)this.m_FileMappings).GetEnumerator();
        }

        #endregion
    }
}
