﻿using Jasper.Bus;
using Shouldly;
using Xunit;

namespace Jasper.Testing.Bus.Bootstrapping
{
    public class configuring_serialization : BootstrappingContext
    {
        [Fact]
        public void disallow_non_versioned_serialization()
        {
            new BusSettings().AllowNonVersionedSerialization.ShouldBeTrue();
            
            theRegistry.Serialization.DisallowNonVersionedSerialization();

            theRuntime.Get<BusSettings>().AllowNonVersionedSerialization
                .ShouldBeFalse();
        }
    }
}