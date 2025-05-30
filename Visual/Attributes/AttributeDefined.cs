namespace Visual.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AttributeDefined(string name) : Attribute
    {
        public string Name = name;
    }
}