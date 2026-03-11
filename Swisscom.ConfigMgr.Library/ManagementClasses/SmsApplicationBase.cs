// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SmsApplicationBase.cs" company="LANexpert S.A.">
//   Copyright (c) 2014
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Swisscom.ConfigMgr.Library.ManagementClasses
{
    using System;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    /// <summary>
    /// The SmsApplicationBase is only used for 
    /// serialization. A web service can not return
    /// an interface.
    /// </summary>
    [Serializable]
    [SoapInclude(typeof(SmsPackage)), SoapInclude(typeof(SmsApplication))]
    [XmlInclude(typeof(SmsPackage)), SoapInclude(typeof(SmsApplication))]
    public class SmsApplicationBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SmsApplicationBase"/> class.
        /// </summary>
        public SmsApplicationBase()
        {
        }
    }
}
