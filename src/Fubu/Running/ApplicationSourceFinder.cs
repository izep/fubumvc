﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FubuCore;
using FubuMVC.Core;
using FubuMVC.Core.Registration;
using StructureMap;

namespace Fubu.Running
{
    public class ApplicationSourceFinder : IApplicationSourceFinder
    {
        public IEnumerable<Type> Find()
        {
            var list = new List<string> { AppDomain.CurrentDomain.SetupInformation.ApplicationBase };

            string binPath = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath;
            if (binPath.IsNotEmpty())
            {
                if (Path.IsPathRooted(binPath))
                {
                    list.Add(binPath);
                }
                else
                {
                    list.Add(AppDomain.CurrentDomain.SetupInformation.ApplicationBase.AppendPath(binPath));
                }
            }

            var assemblies = list.SelectMany(AssembliesFromPath);

            return
                TypeRepository.FindTypes(assemblies, TypeClassification.Concretes,
                    x => x.IsConcreteTypeOf<FubuRegistry>() && x.IsConcreteWithDefaultCtor()).Result();

        }



        // TODO -- this is so common here and in FubuMVC, just get something into FubuCore
        public static IEnumerable<Assembly> AssembliesFromPath(string path)
        {
            var assemblyPaths = Directory.GetFiles(path)
                                         .Where(file =>
                                                Path.GetExtension(file).Equals(
                                                    ".exe",
                                                    StringComparison.OrdinalIgnoreCase)
                                                ||
                                                Path.GetExtension(file).Equals(
                                                    ".dll",
                                                    StringComparison.OrdinalIgnoreCase));

            foreach (string assemblyPath in assemblyPaths)
            {
                Assembly assembly =
                    AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(
                        x => x.GetName().Name == Path.GetFileNameWithoutExtension(assemblyPath));

                if (assembly == null)
                {
                    try
                    {
                        assembly = Assembly.LoadFrom(assemblyPath);

                        
                    }
                    catch
                    {
                    }
                }




                if (assembly != null)
                {
                    yield return assembly;
                }
            }
        }
    }
}