public class BiorbdNode
{
    public string Name { get; protected set; }
    public string ParentName { get; protected set; }

    public BiorbdNode(string _name, string _parentName)
    {
        Name = _name;
        ParentName = _parentName;
    }
}
