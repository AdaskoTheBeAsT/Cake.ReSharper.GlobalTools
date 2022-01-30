// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Cake.Core;
using Cake.Core.IO;
using Cake.ReSharper.GlobalTools.InspectCode;
using Cake.ReSharper.GlobalTools.Tests.Fixtures.InspectCode;
using Cake.Testing;
using Xunit;

namespace Cake.ReSharper.GlobalTools.Tests.Unit.InspectCode;

public sealed class InspectCodeRunnerTests
{
    public sealed class TheRunMethod
    {
        [Fact]
        public void Should_Throw_If_Solution_Is_Null()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Solution = null,
            };

            // When
            var result = Record.Exception(() => fixture.Run());

            // Then
            AssertEx.IsArgumentNullException(result, "solution");
        }

        [Fact]
        public void Should_Find_Inspect_Code_Runner()
        {
            // Given
            var fixture = new InspectCodeRunFixture();

            // When
            var result = fixture.Run();

            // Then
            Assert.Equal("/Working/tools/inspectcode.exe", result.Path.FullPath);
        }

        [Fact]
        public void Should_Find_Inspect_Code_Runner_X86()
        {
            // Given
            var fixture = new InspectCodeRunFixture(isWindows: true, useX86: true);
            fixture.Settings.UseX86Tool = true;

            // When
            var result = fixture.Run();

            // Then
            Assert.Equal("/Working/tools/inspectcode.x86.exe", result.Path.FullPath);
        }

        [Fact]
        public void Should_Use_Provided_Solution_In_Process_Arguments()
        {
            // Given
            var fixture = new InspectCodeRunFixture();

            // When
            var result = fixture.Run();

            // Then
            Assert.Equal("--build \"/Working/Test.sln\"", result.Args);
        }

        [Fact]
        public void Should_Throw_If_Process_Was_Not_Started()
        {
            // Given
            var fixture = new InspectCodeRunFixture();
            fixture.GivenProcessCannotStart();

            // When
            var result = Record.Exception(() => fixture.Run());

            // Then
            Assert.IsType<CakeException>(result);
            Assert.Equal("InspectCode: Process was not started.", result.Message);
        }

        [Fact]
        public void Should_Throw_If_Process_Has_A_Non_Zero_Exit_Code()
        {
            // Given
            var fixture = new InspectCodeRunFixture();
            fixture.GivenProcessExitsWithCode(1);

            // When
            var result = Record.Exception(() => fixture.Run());

            // Then
            Assert.IsType<CakeException>(result);
            Assert.Equal("InspectCode: Process returned an error (exit code 1).", result.Message);
        }

        [Fact]
        public void Should_Set_Output()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Settings =
                {
                    OutputFile = "build/inspect_code.xml",
                },
            };

            // When
            var result = fixture.Run();

            // Then
            Assert.Equal(
                "--build --output=\"/Working/build/inspect_code.xml\" \"/Working/Test.sln\"", result.Args);
        }

        [Fact]
        public void Should_Throw_If_OutputFile_Contains_Violations_And_Set_To_Throw()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Settings =
                {
                    OutputFile = new FilePath("build/violations.xml"),
                    ThrowExceptionOnFindingViolations = true,
                },
            };

            // When
            var result = Record.Exception(() => fixture.Run());

            // Then
            AssertEx.IsCakeException(result, "Code Inspection Violations found in code base.");
        }

        [Fact]
        public void Should_Set_Solution_Wide_Analysis_Switch()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Settings =
                {
                    SolutionWideAnalysis = true,
                },
            };

            // When
            var result = fixture.Run();

            // Then
            Assert.Equal("--build --swea \"/Working/Test.sln\"", result.Args);
        }

        [Fact]
        public void Should_Set_No_Solution_Wide_Analysis_Switch()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Settings =
                {
                    SolutionWideAnalysis = false,
                },
            };

            // When
            var result = fixture.Run();

            // Then
            Assert.Equal("--build --no-swea \"/Working/Test.sln\"", result.Args);
        }

        [Fact]
        public void Should_Set_Project_Filter()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Settings =
                {
                    ProjectFilter = "Test.*",
                },
            };

            // When
            var result = fixture.Run();

            // Then
            Assert.Equal("--build --project=\"Test.*\" \"/Working/Test.sln\"", result.Args);
        }

        [Fact]
        public void Should_Set_MsBuild_Properties()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Settings =
                {
                    MsBuildProperties = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["TreatWarningsAsErrors"] = "true",
                        ["Optimize"] = "false",
                    },
                },
            };

            // When
            var result = fixture.Run();

            // Then
            Assert.Equal(
                "--properties:TreatWarningsAsErrors=\"true\" --properties:Optimize=\"false\" --build \"/Working/Test.sln\"",
                result.Args);
        }

        [Fact]
        public void Should_Set_Caches_Home()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Settings =
                {
                    CachesHome = "caches/",
                },
            };

            // When
            var result = fixture.Run();

            // Then
            Assert.Equal(
                "--caches-home=\"/Working/caches\" --build \"/Working/Test.sln\"",
                result.Args);
        }

        [Fact]
        public void Should_Set_ReSharper_Plugins()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Settings =
                {
                    Extensions = new[]
                    {
                        "ReSharper.AgentSmith",
                        "X.Y",
                    },
                },
            };

            // When
            var result = fixture.Run();

            // Then
            Assert.Equal(
                "-x=\"ReSharper.AgentSmith;X.Y\" --build \"/Working/Test.sln\"",
                result.Args);
        }

        [Fact]
        public void Should_Set_Debug_Switch()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Settings =
                {
                    Debug = true,
                },
            };

            // When
            var result = fixture.Run();

            // Then
            Assert.Equal("--debug --build \"/Working/Test.sln\"", result.Args);
        }

        [Fact]
        public void Should_Set_No_Buildin_Settings_Switch()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Settings =
                {
                    NoBuildInSettings = true,
                },
            };

            // When
            var result = fixture.Run();

            // Then
            Assert.Equal("--no-buildin-settings --build \"/Working/Test.sln\"", result.Args);
        }

        [Fact]
        public void Should_Set_Disabled_Settings_Layers()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Settings =
                {
                    DisabledSettingsLayers = new[]
                    {
                        ReSharperSettingsLayer.GlobalAll,
                        ReSharperSettingsLayer.GlobalPerProduct,
                        ReSharperSettingsLayer.SolutionShared,
                        ReSharperSettingsLayer.SolutionPersonal,
                        ReSharperSettingsLayer.ProjectShared,
                        ReSharperSettingsLayer.ProjectPersonal,
                    },
                },
            };

            // When
            var result = fixture.Run();

            // Then
            Assert.Equal(
                "--disable-settings-layers=GlobalAll;GlobalPerProduct;SolutionShared;SolutionPersonal;ProjectShared;ProjectPersonal --build \"/Working/Test.sln\"",
                result.Args);
        }

        [Fact]
        public void Should_Set_Profile()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Settings =
                {
                    Profile = "profile.DotSettings",
                },
            };

            // When
            var result = fixture.Run();

            // Then
            Assert.Equal(
                "--profile=\"/Working/profile.DotSettings\" --build \"/Working/Test.sln\"",
                result.Args);
        }

        [Fact]
        public void Should_Set_Verbosity()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Settings =
                {
                    Verbosity = ReSharperVerbosity.Error,
                },
            };

            // When
            var result = fixture.Run();

            // Then
            Assert.Equal("--verbosity=ERROR --build \"/Working/Test.sln\"", result.Args);
        }

        [Fact]
        public void Should_Set_Severity()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Settings =
                {
                    Severity = InspectCodeSeverity.Hint,
                },
            };

            // When
            var result = fixture.Run();

            // Then
            Assert.Equal("--build --severity=HINT \"/Working/Test.sln\"", result.Args);
        }

        [Fact]
        public void Should_Analyze_Output()
        {
            var log = new FakeLog();

            // Given
            var fixture = new InspectCodeRunFixture
            {
                Log = log,
                Settings =
                {
                    OutputFile = new FilePath("build/violations.xml"),
                },
            };

            // When
            fixture.Run();

            // Then
            var logContainsInspectionResults =
                log.Entries.Any(p => p.Message.StartsWith("Code Inspection Error(s) Located.", StringComparison.Ordinal));

            Assert.True(logContainsInspectionResults);
        }

        [Fact]
        public void Should_Not_Analyze_Output()
        {
            var log = new FakeLog();

            // Given
            var fixture = new InspectCodeRunFixture
            {
                Log = log,
                Settings =
                {
                    OutputFile = new FilePath("build/violations.xml"),
                    SkipOutputAnalysis = true,
                },
            };

            // When
            fixture.Run();

            // Then
            var logContainsInspectionResults =
                log.Entries.Any(p => p.Message.StartsWith("Code Inspection Error(s) Located.", StringComparison.Ordinal));

            Assert.False(logContainsInspectionResults);
        }
    }

    public sealed class TheRunFromConfigMethod
    {
        [Fact]
        public void Should_Throw_If_Config_File_Is_Null()
        {
            // Given
            var fixture = new InspectCodeRunFromConfigFixture
            {
                Config = null,
            };

            // When
            var result = Record.Exception(() => fixture.Run());

            // Then
            AssertEx.IsArgumentNullException(result, "configFile");
        }

        [Fact]
        public void Should_Use_Provided_Config_File()
        {
            // Given
            var fixture = new InspectCodeRunFromConfigFixture
            {
                Config = "config.xml",
            };

            // Then
            var result = fixture.Run();

            // Then
            Assert.Equal("--config=\"/Working/config.xml\"", result.Args);
        }

        [Fact]
        public void Should_Contain_Build_If_Build_Is_Not_Set()
        {
            // Given
            var fixture = new InspectCodeRunFixture();

            // When
            var result = fixture.Run();

            // Then
            Assert.Contains("--build", result.Args, StringComparison.Ordinal);
            Assert.DoesNotContain("--no-build", result.Args, StringComparison.Ordinal);
        }

        [Fact]
        public void Should_Contain_Build_If_Build_Is_Set_To_True()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Settings =
                {
                    Build = true,
                },
            };

            // When
            var result = fixture.Run();

            // Then
            Assert.Contains("--build", result.Args, StringComparison.Ordinal);
        }

        [Fact]
        public void Should_Contain_NoBuild_If_Build_Is_Set_To_False()
        {
            // Given
            var fixture = new InspectCodeRunFixture
            {
                Settings =
                {
                    Build = false,
                },
            };

            // When
            var result = fixture.Run();

            // Then
            Assert.Contains("--no-build", result.Args, StringComparison.Ordinal);
        }
    }
}
