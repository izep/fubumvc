﻿using FubuTestingSupport;
using NUnit.Framework;

namespace FubuMVC.Tests.StructureMap.Compliance
{
    [TestFixture]
    public class Does_not_require_explicit_registration_in_order_to_retrieve_a_concrete_type
    {
        [Test]
        public void can_retrieve_a_concrete_class()
        {
            ContainerFacilitySource.New(x => { })
                .Get<SomeGuy>().ShouldNotBeNull();
        }
    }

    public class SomeGuy
    {
        
    }
}