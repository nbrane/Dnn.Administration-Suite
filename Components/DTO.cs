using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nBrane.Modules.AdministrationSuite.Components.DTO
{
    public class ApiTemplateResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string HTML { get; set; }
        public string JS { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T CustomObject { get; set; }
    }


    public class GenericListItem
    {
        public string Value { get; set; }
        public string Name { get; set; }
    }
}
