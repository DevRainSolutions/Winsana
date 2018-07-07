using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DevRain.Asana.API.Storage;

namespace Winsana.Phone.Tests.Infrastructure
{
    public class TestDbService:DbService
    {
        protected override string DbName
        {
            get { return "test"; }
        }

    }
}
