using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KeepItUp.Core
{
    public class Server
    {
        public const int NoPid = -1;
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public short Port { get; set; }
        public string BasePath { get;set; }
        public string HomePath { get; set; }
        public bool Enabled { get; set; } = false;
        public int ProcessId { get; set; } = NoPid;
    }
}
