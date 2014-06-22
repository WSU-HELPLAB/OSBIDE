using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OSBIDE.Data.DomainObjects
{
    public enum TimeScale
    {
        Minutes = 1,
        Hours = 2,
        Days = 3,
    }
    public enum ProcedureType
    {
        ErrorQuotient = 1,
        WatwinScoring = 2,
        DataVisualization = 3,
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

    public class EnumListItem
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }
    public class Enum<T>
    {
        public static List<EnumListItem> Get()
        {
            var enumListItems = new List<EnumListItem>();
            foreach (var e in Enum.GetValues(typeof(T)))
            {
                enumListItems.Add(new EnumListItem { Value = (int)e, Text = NameToDisplayText(Enum.GetName(typeof(T), e)) });
            }
            return enumListItems;
        }
        public static string NameToDisplayText(string name)
        {
            if (string.IsNullOrWhiteSpace(name)) return string.Empty;

            var text = new StringBuilder(name.Length * 2);
            text.Append(name[0]);
            for (var idx = 1; idx < name.Length; idx++)
            {
                if (char.IsUpper(name[idx]))
                {
                    if (name[idx - 1] != ' ' && !char.IsUpper(name[idx - 1]) && idx < name.Length - 1 && !char.IsUpper(name[idx + 1]))
                    {
                        text.Append(' ');
                    }
                }
                text.Append(name[idx]);
            }
            return text.ToString();
        }
    }
}
