using System.Diagnostics;
using Bottles;
using Bottles.Assemblies;
using FubuCore;
using FubuMVC.Core;
using NUnit.Framework;
using FubuMVC.StructureMap;
using StructureMap;
using FubuTestingSupport;
using System.Linq;
using System.Collections.Generic;

namespace FubuMVC.Tests
{
    [TestFixture]
    public class can_find_and_load_bottles_with_the_FubuModule_attribute
    {
        [Test]
        public void find_assembly_bottles()
        {
            // Trash gets left over from other tests.  Joy.
            new FileSystem().DeleteFile("something.asset.config");
            new FileSystem().DeleteFile("something.script.config");
            new FileSystem().DeleteFile("else.script.config");
            new FileSystem().DeleteFile("else.asset.config");

            FubuApplication.For(new FubuRegistry()).StructureMap(new Container())
                .Bootstrap();

            var assembly = typeof(AssemblyPackage.AssemblyPackageMarker).Assembly;

            PackageRegistry.PackageAssemblies.ShouldContain(assembly);

            PackageRegistry.Packages.Each(x => Debug.WriteLine(x.Name));


            PackageRegistry.Packages.OfType<AssemblyPackageInfo>().Any(x => x.Name == AssemblyPackageInfo.CreateFor(assembly).Name)
                .ShouldBeTrue();
        }
    }
}