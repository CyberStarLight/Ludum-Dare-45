using System;
using System.Collections.Generic;
using System.Text;

public class RenamedAttribute : Attribute
{
    public string[] OldNames { get; private set; }

    public RenamedAttribute(params string[] oldNames)
    {
        OldNames = oldNames;
    }
}
