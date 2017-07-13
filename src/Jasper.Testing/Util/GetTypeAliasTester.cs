﻿using Jasper.Testing.Bus.Runtime;
using Jasper.Util;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Util
{
    public class GetTypeAliasTester
    {
        [Fact]
        public void respect_the_type_alias_attribute()
        {
            typeof(AliasedMessage).ToTypeAlias()
                .ShouldBe("MyThing");
        }

        [Fact]
        public void use_the_types_full_name_otherwise()
        {
            typeof(Message1).ToTypeAlias()
                .ShouldBe(typeof(Message1).FullName);
        }
    }

    [TypeAlias("MyThing")]
    public class AliasedMessage
    {

    }
}