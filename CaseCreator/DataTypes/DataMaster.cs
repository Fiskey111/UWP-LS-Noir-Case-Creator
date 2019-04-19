using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaseCreator.DataTypes
{
    public class DataMaster
    {
        public List<ItemData> DataEntry { get; set; }

        public DataMaster()
        {
            DataEntry = new List<ItemData>();
        }
    }
}
