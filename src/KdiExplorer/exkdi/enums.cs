using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace exkdi
{
    public enum EncodingOptions
    {
        ASCII, KOI8R
    }


    public enum PrintOutputOptions
    {
        text, csv
    }


    public enum TypeOutputOptions
    {
        hex, text, raw
    }

    public enum DirOutputOptions
    {
        list = 0, 
        wide = 1,
        csv = 2
    }

    public enum ClusterOutputOptions
    {
        hex, hash, raw
    }

}
