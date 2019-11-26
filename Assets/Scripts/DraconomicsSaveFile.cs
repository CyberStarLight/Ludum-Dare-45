using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DraconomicsSave : SaveFile<PlayerData>
{
    public override string FileName { get; set; }
    public override string FileExtension { get; set; }
    public override string FileCategory { get; set; }

    public DraconomicsSave()
    {
        FileName = "UserData";
        FileExtension = ".sav";
        FileCategory = "User";
    }
}

