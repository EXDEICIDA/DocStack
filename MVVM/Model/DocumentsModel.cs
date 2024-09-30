using DocStack.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocStack.MVVM.Model
{
    public class DocumentsModel
    {
        //A model for holding data of documents

        public int Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        private long _size;
        public long Size
        {
            get => _size;
            set
            {
                _size = value;
                SizeString = FileSizeConverter.ConvertBytesToString(value);
            }
        }
        public string SizeString { get; private set; }

        public string DateAdded { get; set; }
    }
}
