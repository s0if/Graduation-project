﻿namespace Graduation.Model
{
    public class TypeProperty
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<PropertyProject> Properties { get; set; } = new HashSet<PropertyProject>();
    }
}
