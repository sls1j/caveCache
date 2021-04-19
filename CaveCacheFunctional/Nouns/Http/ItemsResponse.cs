using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaveCache.Nouns.Http
{
    public class ItemsResponse<T>: Response
    {
        public T[] Items { get; set; }
    }
}
