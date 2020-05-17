using System;
using System.Collections.Generic;

namespace cw3_apbd.Models_2
{
    public partial class ProcedureAnimal
    {
        public int ProcedureIdProcedure { get; set; }
        public int AnimalIdAnimal { get; set; }
        public DateTime Date { get; set; }

        public virtual Animal AnimalIdAnimalNavigation { get; set; }
        public virtual Procedure ProcedureIdProcedureNavigation { get; set; }
    }
}
