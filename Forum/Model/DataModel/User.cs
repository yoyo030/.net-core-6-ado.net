using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

namespace Forum.Models.DataModel
{
    [Serializable]
    public class User
    {
        public long ID { get; set; }
        public string Account { get; set; } = "";

        public string Name { get; set; } = "";

        public DateTime CreateTime { get; set; }
    }
}