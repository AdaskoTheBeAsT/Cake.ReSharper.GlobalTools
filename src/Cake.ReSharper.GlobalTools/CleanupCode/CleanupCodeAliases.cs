// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.IO;

namespace Cake.ReSharper.GlobalTools.CleanupCode;

/// <summary>
/// <para>Contains functionality related to ReSharper's <see href="https://www.jetbrains.com/help/resharper/CleanupCode.html">CleanupCode</see> tool.</para>
/// <para>
/// In order to use the commands for this alias, include the following in your build.cake file to download and
/// install from nuget.org, or specify the ToolPath within the <see cref="CleanupCodeSettings" /> class:
/// <code>
/// #tool "nuget:?package=JetBrains.ReSharper.GlobalTools"
/// </code>
/// </para>
/// </summary>
[CakeAliasCategory(nameof(ReSharper))]
public static class CleanupCodeAliases
{
    /// <summary>
    /// Cleanups the specified solution with ReSharper's CleanupCode.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="solution">The solution.</param>
    /// <example>
    /// <code>
    /// CleanupCode("./src/MySolution.sln");
    /// </code>
    /// </example>
    [CakeMethodAlias]
    [CakeAliasCategory(nameof(CleanupCode))]
    public static void CleanupCode(this ICakeContext context, FilePath solution)
    {
        CleanupCode(context, solution, new CleanupCodeSettings());
    }

    /// <summary>
    /// Cleanups the specified solution with ReSharper's CleanupCode,
    /// using the specified settings.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="solution">The solution.</param>
    /// <param name="settings">The settings.</param>
    /// <example>
    /// <code>
    /// var buildOutputDirectory = Directory("./.build");
    /// var resharperReportsDirectory = buildOutputDirectory + Directory("_ReSharperReports");
    ///
    /// var msBuildProperties = new Dictionary&lt;string, string&gt;();
    /// msBuildProperties.Add("configuration", configuration);
    /// msBuildProperties.Add("platform", "AnyCPU");
    ///
    /// CleanupCode("./MySolution.sln", new CleanupCodeSettings {
    ///     Profile = "./MySolution.sln.DotSettings",
    ///     MsBuildProperties = msBuildProperties,
    /// });
    /// </code>
    /// </example>
    [CakeMethodAlias]
    [CakeAliasCategory(nameof(CleanupCode))]
    public static void CleanupCode(this ICakeContext context, FilePath solution, CleanupCodeSettings settings)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var runner = new CleanupCodeRunner(context.FileSystem, context.Environment, context.ProcessRunner, context.Tools, context.Log);
        runner.Run(solution, settings);
    }

    /// <summary>
    /// Runs ReSharper's CleanupCode using the specified config file.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="configFile">The config file.</param>
    /// <example>
    /// <code>
    /// CleanupCodeFromConfig("./src/cleanupcode.config");
    /// </code>
    /// </example>
    [CakeMethodAlias]
    [CakeAliasCategory(nameof(CleanupCode))]
    public static void CleanupCodeFromConfig(this ICakeContext context, FilePath configFile)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var runner = new CleanupCodeRunner(context.FileSystem, context.Environment, context.ProcessRunner, context.Tools, context.Log);
        runner.RunFromConfig(configFile);
    }
}
