using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LMS.ViewModels
{
    public class AssignedAuthorData
    {
        public int AuthorID { get; set; }
        public string Name { get; set; }
        public bool Assigned { get; set; }
    }

    public class AssignedCategoryData
    {
        public int CategoryID { get; set; }
        public string Name { get; set; }
        public bool Assigned { get; set; }
    }
}