using System;
using System.Collections.Generic;
using System.Linq;

namespace OSBIDE.Data.DomainObjects
{
    public enum ProcedureType
    {
        ErrorQuotient = 1,
        WatwinScoring = 2,
    }
    public enum ResultViewType
    {
        Tabular = 1,
        Bar = 2,
        Scatter = 3,
        Bubble = 4,
    }
    public enum FileUploadSchema
    {
        CSV = 1,
        Survey = 2,
        Grade = 3,
    }
    public enum CategoryColumn
    {
        InstitutionId,
        Name,
        Gender,
        Age,
        Class,
        Ethnicity,
    }
    public class Enum<T>
    {
        public static List<string> Get()
        {
            return (from e in Enum.GetNames(typeof(T)) select e).ToList();
        }
    }
}
