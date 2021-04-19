using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CaveCache.Nouns.Http
{
    public class ItemResponse<T>:Response
    {
        public T Item { get; set; }
    }
}
