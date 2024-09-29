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
        public long Size { get; set; }
        public string DateAdded { get; set; }
    }
}
