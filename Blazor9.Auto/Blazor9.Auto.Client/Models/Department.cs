namespace Blazor9.Auto.Client.Models
{
    public class Department
    {
        public long Departmentid { get; set; }
        public string Name { get; set; } = string.Empty;
        public IEnumerable<Person>? People { get; set; }
    }
}
