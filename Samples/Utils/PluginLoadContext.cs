using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using Perigee;
using Perigee.Helpers;

namespace Samples
{
    /// <summary>
    /// Please note: Plugin contexts only work in .NET Core + applications (not .NET standard)
    /// </summary>
    public static partial class PluginContextPerigeeExtensions
    {
        /// <summary>
        /// Add a plugin load context to the perigee system. The <paramref name="Modules"/> callback is called on init to setup initial modules
        /// </summary>
        /// <typeparam name="T">Type of the plugins loaded</typeparam>
        /// <param name="c">Thread registry</param>
        /// <param name="name">Name of the function, must be unique</param>
        /// <param name="typeSearchFunc">Type search for the DLLs exported assembly types</param>
        /// <param name="pluginPath">Plugin paths</param>
        /// <param name="LockEvent">Lock event to synchronize hotload and running apps</param>
        /// <param name="Initial">Initial set of <typeparamref name="T"/> </param>
        /// <param name="Modules">The callback when new modules are assigned</param>
        /// <returns></returns>
        public static ThreadRegistry AddPluginLoadContext<T>(this ThreadRegistry c, string name, Func<Type[], List<T>> typeSearchFunc, ManualResetEvent LockEvent, List<T> Initial, Action<List<T>> Modules, string pluginPath = @"Plugins\", bool started = true)
        {

            if (!Directory.Exists(pluginPath)) Directory.CreateDirectory(pluginPath);

            void plcwatch(CancellationToken ct, Microsoft.Extensions.Logging.ILogger log)
            {
                try
                {
                    var PLC = new PluginLoadContext<T>(typeSearchFunc, pluginPath, log);

                    void Reload()
                    {
                        try
                        {
                            var nMods = new List<T>();
                            log?.LogInformation("Hotloading plugins...");
                            LockEvent.Reset();
                            PLC?.HotReload();
                            nMods.AddRange(Initial);
                            nMods.AddRange(PLC.GetInstances());
                            Modules?.Invoke(nMods);
                        }
                        catch (Exception ex)
                        {
                            Modules?.Invoke(Initial);
                            log?.LogError(ex, "Couldn't hot load plugins...");
                        }
                        finally
                        {

                            LockEvent.Set();
                        }
                    }
                    var debounce = new Debounce(() =>
                    {
                        Reload();
                    });
                    c.AddDirectoryNotifier($"{name}-AutoPluginLoadContext", pluginPath, @".*\.dll$", SearchOption.TopDirectoryOnly, (ct, l, p) =>
                    debounce.Bounce(),
                    NotifierLoop: TimeSpan.FromSeconds(5), NotifyInitial: false);

                    try
                    {
                        var nMods = new List<T>();
                        nMods.AddRange(Initial);
                        nMods.AddRange(PLC.GetInstances());
                        Modules?.Invoke(nMods);
                    }
                    catch (Exception ex) { log?.LogError(ex, "Couldn't call initial module callback"); }

                    while (PerigeeApplication.delayOrCancel(1000, ct)) { }
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    c.RemoveThreadByName($"{name}-AutoPluginLoadContext");
                }

            }

            var mThread = new ManagedThread(name, plcwatch, c.CTS, c.GetLogger<PluginLoadContext<T>>(), null, started);
            c.AddManagedThread(mThread);
            return c;
        }
    }

    /// <summary>
    /// Plugin Load Context enables applications to dynamically load and hot-reload plugins as well as provides a locking mechanism to prevent asynchronous usage issues
    /// </summary>
    /// <typeparam name="T">Type of plugin data to load and return. typically this would be a class that contains the Type param and additional info about where the module was loaded</typeparam>
    public class PluginLoadContext<T> : IDisposable
    {
        internal AssemblyLoadContext alc { get; set; }

        internal AssemblyDependencyResolver _resolver { get; set; }

        internal Dictionary<string, Assembly> asms = new Dictionary<string, Assembly>();
        internal List<Type> AvailableTs { get; set; } = new List<Type>();
        internal List<T> TypeInstances = new List<T>();
        internal ManualResetEvent _MRELoad = new ManualResetEvent(false);
        internal ILogger _Logger;
        internal string PluginPath { get; set; }
        public Func<Type[], List<T>> TypeSearchFunc { get; }

        public PluginLoadContext(Func<Type[], List<T>> typeSearchFunc, string pluginPath = @"Plugins\", ILogger? log = null)
        {
            TypeSearchFunc = typeSearchFunc;
            _Logger = log;
            PluginPath = Path.GetFullPath(pluginPath);
            _resolver = new AssemblyDependencyResolver(PluginPath);
            alc = new AssemblyLoadContext(Guid.NewGuid().ToString(), true);
            ReadPlugins();


        }

        /// <summary>
        /// Read all of the plugins, calling <see cref="TypeSearchFunc"/>
        /// </summary>
        internal void ReadPlugins()
        {
            try
            {
                TypeInstances?.Clear();
                asms?.Clear();
                AvailableTs?.Clear();

                TypeInstances ??= new List<T>();
                asms ??= new Dictionary<string, Assembly>();
                AvailableTs ??= new List<Type>();

                FileUtil.CreateDirectoryIfNotExists(PluginPath);
                var DLLS = Directory.GetFiles(PluginPath, "*.dll", SearchOption.AllDirectories);
                if (DLLS.Length > 0)
                {
                    using (var alc2 = alc.EnterContextualReflection())
                    {
                        foreach (var path in DLLS)
                        {
                            using (MemoryStream msasm = new MemoryStream(File.ReadAllBytes(path)))
                            {
                                var asm = alc.LoadFromStream(msasm);

                                if (asm != null)
                                {
                                    asms.Add(path, asm);

                                    TypeInstances = TypeSearchFunc(asm.GetExportedTypes());
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Failed to initialize plugin instances");
            }
            finally
            {
                _MRELoad.Set();
            }
        }

        /// <summary>
        /// Get the instances, paying attention to the lock
        /// </summary>
        /// <returns></returns>
        public List<T> GetInstances()
        {
            _MRELoad.WaitOne();
            return TypeInstances;
        }

        /// <summary>
        /// Hot reload instances, paying attention to the lock. It will regenerate the AssemblyLoadContext and reread plugins
        /// </summary>
        public void HotReload()
        {

            try
            {
                _MRELoad.WaitOne();
                _MRELoad.Reset();
                alc?.Unload();
                alc = new AssemblyLoadContext(Guid.NewGuid().ToString(), true);
                ReadPlugins();

            }
            catch (Exception ex)
            {
                _Logger.LogError(ex, "Failed to reload instances");
            }
            finally
            {
                _MRELoad.Set();
            }

        }

        /// <summary>
        /// Clean up!
        /// </summary>
        public void Dispose()
        {
            //Unload
            try
            {
                TypeInstances?.Clear();
                TypeInstances = null;

                asms?.Clear();
                asms = null;

                AvailableTs?.Clear();
                AvailableTs = null;

                alc?.Unload();

            }
            catch (Exception ex)
            {

            }
        }
    }
}
