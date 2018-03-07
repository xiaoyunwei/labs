using System;
using System.Collections.Generic;

namespace AdventureWorksLTData.Web.Models.AdventureWorks
{
    public partial class BuildVersion
    {
        public byte SystemInformationId { get; set; }
        public string DatabaseVersion { get; set; }
        public DateTime VersionDate { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
