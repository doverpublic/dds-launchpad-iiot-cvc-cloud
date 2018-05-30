using System;
using System.Collections.Generic;
using System.Data.Entity;

using System.Linq;
using System.Threading.Tasks;


using global::Iot.Common;

namespace Launchpad.Iot.Insight.DataService
{
    public class DataContext : DbContext
    {
        public DataContext() : base( "name=InMemoryDB"){}

        public DbSet<User> Users { get; set; }
    }
}
