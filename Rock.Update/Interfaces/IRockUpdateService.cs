﻿using System;
using System.Collections.Generic;

namespace Rock.Update.Interfaces
{
    public interface IRockUpdateService
    {
        List<RockRelease> GetReleasesList( Version version );
        string GetRockEarlyAccessRequestUrl();
        RockReleaseProgram GetRockReleaseProgram();
        bool IsEarlyAccessInstance();
    }
}