    using System;

    namespace BarberShop.Domain.Entities
    {
        public class Log
        {
            public int? LogId { get; set; }
            public DateTime? LogDateTime { get; set; }
            public string? LogLevel { get; set; }
            public string? Source { get; set; }
            public string? Message { get; set; }
            public string? Data { get; set; }
            public string? ResourceID { get; set; } // Nova propriedade para armazenar o ID do recurso
        }
    }
