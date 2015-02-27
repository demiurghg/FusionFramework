// Guids.cs
// MUST match guids.h
using System;

namespace Fusion.FusionTemplate
{
    static class GuidList
    {
        public const string guidFusionTemplatePkgString = "a640bd2e-8a01-4856-b997-48a210808e1d";
        public const string guidFusionTemplateCmdSetString = "f6738bc8-ed36-4c11-b967-99ab4164097d";

        public static readonly Guid guidFusionTemplateCmdSet = new Guid(guidFusionTemplateCmdSetString);
    };
}