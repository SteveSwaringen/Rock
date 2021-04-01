// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Rock.Update;
using Rock.Update.Exceptions;
using Rock.Update.Interfaces;
using Rock.Update.Models;

namespace Rock.Tests.Integration.RockUpdate
{
    [TestClass]
    public class RockInstallerTests
    {
        [TestMethod]
        [ExpectedException( typeof( PackageNotFoundException ) )]
        public void InstallVersion_ShouldRaisePackageNotFoundExceptionIfNullReleaseList()
        {
            var rockUpdateService = new Mock<IRockUpdateService>();
            var rockInstaller = new RockInstaller( rockUpdateService.Object, new Version( "1.13.1" ), new Version( "1.13.0" ) );
            var package = rockInstaller.InstallVersion();

            rockUpdateService.Verify( x => x.GetReleasesList( It.IsAny<Version>() ), Times.Once );
        }

        [TestMethod]
        [ExpectedException( typeof( PackageNotFoundException ) )]
        public void InstallVersion_ShouldRaisePackageNotFoundExceptionIfEmptyReleaseList()
        {
            var releaseList = new List<RockRelease>();

            var rockUpdateService = new Mock<IRockUpdateService>();

            rockUpdateService.Setup( rus => rus.GetReleasesList( It.IsAny<Version>() ) ).Returns( releaseList );

            var rockInstaller = new RockInstaller( rockUpdateService.Object, new Version( "1.13.1" ), new Version( "1.13.0" ) );
            var package = rockInstaller.InstallVersion();

            rockUpdateService.Verify( x => x.GetReleasesList( It.IsAny<Version>() ), Times.Once );
        }

        [TestMethod]
        [DataRow( "content/test.txt" )]
        [DataRow( "content\\test.txt" )]
        public void InstallVersion_ShouldCorrectlyCopyContentFiles(string targetPath)
        {
            var expectedBackupFileText = "Original Test File";
            var expectedInstalledText = "Installed Test File";

            var testPackagePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\testPackage.rockpkg";
            var targetVersion = "1.13.1";
            var currentVersion = "1.13.0";

            if ( File.Exists( testPackagePath ) )
            {
                File.Delete( testPackagePath );
            }

            var testBackupDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}\\App_Data\\RockBackup\\{currentVersion}";
            if ( Directory.Exists( testBackupDirectory ) )
            {
                Directory.Delete( testBackupDirectory, true );
            }

            File.WriteAllText( $"{AppDomain.CurrentDomain.BaseDirectory}\\test.txt", expectedBackupFileText );

            using ( var packageFileStream = new FileStream( testPackagePath, FileMode.OpenOrCreate ) )
            {
                using ( var testPackage = new ZipArchive( packageFileStream, ZipArchiveMode.Create ) )
                {
                    var testEntry = testPackage.CreateEntry( targetPath );
                    using ( StreamWriter writer = new StreamWriter( testEntry.Open() ) )
                    {
                        writer.Write( expectedInstalledText );
                    }
                }
            }

            var releaseList = new List<RockRelease>
            {
                new RockRelease
                {
                    PackageUri = $"file://{testPackagePath}",
                    SemanticVersion = targetVersion
                }
            };

            var rockUpdateService = new Mock<IRockUpdateService>();

            rockUpdateService.Setup( rus => rus.GetReleasesList( It.IsAny<Version>() ) ).Returns( releaseList );

            var rockInstaller = new RockInstaller( rockUpdateService.Object, new Version( targetVersion ), new Version( currentVersion ) );
            var package = rockInstaller.InstallVersion();

            rockUpdateService.Verify( x => x.GetReleasesList( It.IsAny<Version>() ), Times.Once );

            // Validate backup file was created.
            var expectedBackupFilePath = $"{testBackupDirectory}\\test.txt";
            Assert.IsTrue( File.Exists( expectedBackupFilePath ) );

            var actualBackupFileText = File.ReadAllText( expectedBackupFilePath );
            Assert.AreEqual( expectedBackupFileText, actualBackupFileText );

            // Validate new file was copied in.
            var expectedInstalledFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\test.txt";
            Assert.IsTrue( File.Exists( expectedInstalledFilePath ) );

            var actualInstalledText = File.ReadAllText( expectedInstalledFilePath );
            Assert.AreEqual( expectedInstalledText, actualInstalledText );
        }

        [TestMethod]
        [DataRow( "content/web.config.rock.xdt" )]
        [DataRow( "content\\web.config.rock.xdt" )]
        public void InstallVersion_ShouldCorrectlyHandlesTransformFiles(string targetPath)
        {
            var expectedBackupFileText = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <configuration>
                  <connectionStrings configSource=""web.ConnectionStrings.config"" />
                  <system.web>
                    <httpRuntime targetFramework=""4.7.2""
                                 requestValidationMode=""4.5""
                                 requestValidationType=""Rock.Web.RequestValidator""
                                 relaxedUrlToFileSystemMapping=""true""
                                 maxRequestLength=""102400""
                                 waitChangeNotification=""5""
                                 maxQueryStringLength=""16384"" 
                    />
                  </system.web>
                  <system.webServer>
                    <!-- max request size - rock default 10 MB -->
                    <security>
                      <requestFiltering>
                        <requestLimits maxAllowedContentLength=""104857600"" maxQueryString=""16384"" />
                      </requestFiltering>
                    </security>
                  </system.webServer>
                </configuration>";

            var expectedInstalledText = @"<?xml version=""1.0"" encoding=""utf-8""?>
                <configuration>
                  <connectionStrings configSource=""web.ConnectionStrings.config"" />
                  <system.web>
                    <httpRuntime targetFramework=""4.5.2"" requestValidationMode=""4.5"" requestValidationType=""Rock.Web.RequestValidator""
                      relaxedUrlToFileSystemMapping=""true"" maxRequestLength=""1028"" waitChangeNotification=""5"" maxQueryStringLength=""2048""/>
                  </system.web>
                  <system.webServer>
                    <!-- max request size - rock default 10 MB -->
                    <security>
                      <requestFiltering>
                        <requestLimits maxAllowedContentLength=""208124567"" maxQueryString=""16384""/>
                      </requestFiltering>
                    </security>
                  </system.webServer>
                </configuration>";

            var testPackagePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\testPackage.rockpkg";
            var targetVersion = "1.13.1";
            var currentVersion = "1.13.0";

            if ( File.Exists( testPackagePath ) )
            {
                File.Delete( testPackagePath );
            }

            var testBackupDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}\\App_Data\\RockBackup\\{currentVersion}";
            if ( Directory.Exists( testBackupDirectory ) )
            {
                Directory.Delete( testBackupDirectory, true );
            }

            File.WriteAllText( $"{AppDomain.CurrentDomain.BaseDirectory}\\web.config", expectedBackupFileText );

            using ( var packageFileStream = new FileStream( testPackagePath, FileMode.OpenOrCreate ) )
            {
                using ( var testPackage = new ZipArchive( packageFileStream, ZipArchiveMode.Create ) )
                {
                    var testEntry = testPackage.CreateEntry( targetPath );
                    using ( StreamWriter writer = new StreamWriter( testEntry.Open() ) )
                    {
                        writer.Write( @"<?xml version=""1.0""?>
                            <configuration xmlns:xdt=""http://schemas.microsoft.com/XML-Document-Transform"">
	                            <system.web>
		                            <httpRuntime targetFramework=""4.5.2""
		                              requestValidationMode=""4.5""
		                              requestValidationType=""Rock.Web.RequestValidator""
		                              relaxedUrlToFileSystemMapping=""true""
		                              maxRequestLength=""1028""
		                              waitChangeNotification=""5""
		                              maxQueryStringLength=""2048"" xdt:Transform=""Replace""/>
	                            </system.web>
                                <system.webServer>
		                            <security>
		                              <requestFiltering>
			                            <requestLimits maxAllowedContentLength=""208124567"" maxQueryString=""16384"" xdt:Transform=""Replace"" />
		                              </requestFiltering>
		                            </security>
                                </system.webServer>
                            </configuration>" );
                    }
                }
            }

            var releaseList = new List<RockRelease>
            {
                new RockRelease
                {
                    PackageUri = $"file://{testPackagePath}",
                    SemanticVersion = targetVersion
                }
            };

            var rockUpdateService = new Mock<IRockUpdateService>();

            rockUpdateService.Setup( rus => rus.GetReleasesList( It.IsAny<Version>() ) ).Returns( releaseList );

            var rockInstaller = new RockInstaller( rockUpdateService.Object, new Version( targetVersion ), new Version( currentVersion ) );
            var package = rockInstaller.InstallVersion();

            rockUpdateService.Verify( x => x.GetReleasesList( It.IsAny<Version>() ), Times.Once );

            // Validate backup file was created.
            var expectedBackupFilePath = $"{testBackupDirectory}\\web.config";
            Assert.IsTrue( File.Exists( expectedBackupFilePath ) );

            var actualBackupFileText = File.ReadAllText( expectedBackupFilePath );
            Assert.AreEqual( expectedBackupFileText, actualBackupFileText );

            // Validate new file was copied in.
            var expectedInstalledFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\web.config";
            Assert.IsTrue( File.Exists( expectedInstalledFilePath ) );

            var actualInstalledText = File.ReadAllText( expectedInstalledFilePath );
            Assert.AreEqual( expectedInstalledText, actualInstalledText );
        }

        [TestMethod]
        [DataRow( "install/deletefile.lst" )]
        [DataRow( "install\\deletefile.lst" )]
        public void InstallVersion_ShouldCorrectlyDeleteFiles(string targetPath)
        {
            var expectedBackupFileText = "Original Test File";

            var testPackagePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\testPackage.rockpkg";
            var targetVersion = "1.13.1";
            var currentVersion = "1.13.0";

            if ( File.Exists( testPackagePath ) )
            {
                File.Delete( testPackagePath );
            }

            var testBackupDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}\\App_Data\\RockBackup\\{currentVersion}";
            if ( Directory.Exists( testBackupDirectory ) )
            {
                Directory.Delete( testBackupDirectory, true );
            }

            File.WriteAllText( $"{AppDomain.CurrentDomain.BaseDirectory}\\test.txt", expectedBackupFileText );

            using ( var packageFileStream = new FileStream( testPackagePath, FileMode.OpenOrCreate ) )
            {
                using ( var testPackage = new ZipArchive( packageFileStream, ZipArchiveMode.Create ) )
                {
                    var testEntry = testPackage.CreateEntry( targetPath );
                    using ( StreamWriter writer = new StreamWriter( testEntry.Open() ) )
                    {
                        writer.Write( "test.txt" );
                    }
                }
            }

            var releaseList = new List<RockRelease>
            {
                new RockRelease
                {
                    PackageUri = $"file://{testPackagePath}",
                    SemanticVersion = targetVersion
                }
            };

            var rockUpdateService = new Mock<IRockUpdateService>();

            rockUpdateService.Setup( rus => rus.GetReleasesList( It.IsAny<Version>() ) ).Returns( releaseList );

            var rockInstaller = new RockInstaller( rockUpdateService.Object, new Version( targetVersion ), new Version( currentVersion ) );
            var package = rockInstaller.InstallVersion();

            rockUpdateService.Verify( x => x.GetReleasesList( It.IsAny<Version>() ), Times.Once );

            // Validate backup file was created.
            var expectedBackupFilePath = $"{testBackupDirectory}\\test.txt";
            Assert.IsTrue( File.Exists( expectedBackupFilePath ) );

            var actualBackupFileText = File.ReadAllText( expectedBackupFilePath );
            Assert.AreEqual( expectedBackupFileText, actualBackupFileText );

            // Validate new file was copied in.
            var expectedInstalledFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\test.txt";
            Assert.IsFalse( File.Exists( expectedInstalledFilePath ) );
        }

        [TestMethod]
        [DataRow( "content/test.txt", "install/deletefile.lst" )]
        [DataRow( "content\\test.txt", "install\\deletefile.lst" )]
        [DataRow( "content/test.txt", "install\\deletefile.lst" )]
        [DataRow( "content\\test.txt", "install/deletefile.lst" )]
        public void InstallVersion_ShouldCorrectlyDeleteFilesEvenIfTheyWereModified(string testFileTarget, string deleteFileTarget)
        {
            var expectedBackupFileText = "Original Test File";
            var expectedInstalledText = "Installed Test File";
            var testPackagePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\testPackage.rockpkg";
            var targetVersion = "1.13.1";
            var currentVersion = "1.13.0";

            if ( File.Exists( testPackagePath ) )
            {
                File.Delete( testPackagePath );
            }

            var testBackupDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}\\App_Data\\RockBackup\\{currentVersion}";
            if ( Directory.Exists( testBackupDirectory ) )
            {
                Directory.Delete( testBackupDirectory, true );
            }

            File.WriteAllText( $"{AppDomain.CurrentDomain.BaseDirectory}\\test.txt", expectedBackupFileText );

            using ( var packageFileStream = new FileStream( testPackagePath, FileMode.OpenOrCreate ) )
            {
                using ( var testPackage = new ZipArchive( packageFileStream, ZipArchiveMode.Create ) )
                {
                    var testEntry = testPackage.CreateEntry( testFileTarget );
                    using ( StreamWriter writer = new StreamWriter( testEntry.Open() ) )
                    {
                        writer.Write( expectedInstalledText );
                    }

                    testEntry = testPackage.CreateEntry( deleteFileTarget );
                    using ( StreamWriter writer = new StreamWriter( testEntry.Open() ) )
                    {
                        writer.Write( "test.txt" );
                    }
                }
            }

            var releaseList = new List<RockRelease>
            {
                new RockRelease
                {
                    PackageUri = $"file://{testPackagePath}",
                    SemanticVersion = targetVersion
                }
            };

            var rockUpdateService = new Mock<IRockUpdateService>();

            rockUpdateService.Setup( rus => rus.GetReleasesList( It.IsAny<Version>() ) ).Returns( releaseList );

            var rockInstaller = new RockInstaller( rockUpdateService.Object, new Version( targetVersion ), new Version( currentVersion ) );
            var package = rockInstaller.InstallVersion();

            rockUpdateService.Verify( x => x.GetReleasesList( It.IsAny<Version>() ), Times.Once );

            // Validate backup file was created.
            var expectedBackupFilePath = $"{testBackupDirectory}\\test.txt";
            Assert.IsTrue( File.Exists( expectedBackupFilePath ) );

            var actualBackupFileText = File.ReadAllText( expectedBackupFilePath );
            Assert.AreEqual( expectedBackupFileText, actualBackupFileText );

            // Validate new file was copied in.
            var expectedInstalledFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\test.txt";
            Assert.IsFalse( File.Exists( expectedInstalledFilePath ) );
        }

        [TestMethod]
        [DataRow( "install\\deletefile.lst" )]
        [DataRow( "install/deletefile.lst" )]
        public void InstallVersion_ShouldCorrectlyDeleteDirectories(string targetPath)
        {
            var expectedBackupFileText = "Original Test File";

            var testPackagePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\testPackage.rockpkg";
            var targetVersion = "1.13.1";
            var currentVersion = "1.13.0";

            if ( File.Exists( testPackagePath ) )
            {
                File.Delete( testPackagePath );
            }

            var testBackupDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}\\App_Data\\RockBackup\\{currentVersion}";
            if ( Directory.Exists( testBackupDirectory ) )
            {
                Directory.Delete( testBackupDirectory, true );
            }

            Directory.CreateDirectory( $"{AppDomain.CurrentDomain.BaseDirectory}\\test" );
            File.WriteAllText( $"{AppDomain.CurrentDomain.BaseDirectory}\\test\\test.txt", expectedBackupFileText );

            using ( var packageFileStream = new FileStream( testPackagePath, FileMode.OpenOrCreate ) )
            {
                using ( var testPackage = new ZipArchive( packageFileStream, ZipArchiveMode.Create ) )
                {
                    var testEntry = testPackage.CreateEntry( targetPath );
                    using ( StreamWriter writer = new StreamWriter( testEntry.Open() ) )
                    {
                        writer.Write( "test" );
                    }
                }
            }

            var releaseList = new List<RockRelease>
            {
                new RockRelease
                {
                    PackageUri = $"file://{testPackagePath}",
                    SemanticVersion = targetVersion
                }
            };

            var rockUpdateService = new Mock<IRockUpdateService>();

            rockUpdateService.Setup( rus => rus.GetReleasesList( It.IsAny<Version>() ) ).Returns( releaseList );

            var rockInstaller = new RockInstaller( rockUpdateService.Object, new Version( targetVersion ), new Version( currentVersion ) );
            var package = rockInstaller.InstallVersion();

            rockUpdateService.Verify( x => x.GetReleasesList( It.IsAny<Version>() ), Times.Once );

            // Validate backup file was created.
            var expectedBackupFilePath = $"{testBackupDirectory}\\test\\test.txt";
            Assert.IsTrue( File.Exists( expectedBackupFilePath ) );

            var actualBackupFileText = File.ReadAllText( expectedBackupFilePath );
            Assert.AreEqual( expectedBackupFileText, actualBackupFileText );

            // Validate new file was copied in.
            var expectedInstalledFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\test\\test.txt";
            Assert.IsFalse( File.Exists( expectedInstalledFilePath ) );
        }

        [TestMethod]
        [DataRow( "content/test/test.txt", "install/deletefile.lst" )]
        //[DataRow( "content\\test\\test.txt", "install\\deletefile.lst" )]
        //[DataRow( "content/test/test.txt", "install\\deletefile.lst" )]
        //[DataRow( "content\\test\\test.txt", "install/deletefile.lst" )]
        public void InstallVersion_ShouldCorrectlyDeleteDirectoriesEvenIfTheyWereModified(string fileTarget, string deleteTarget)
        {
            var expectedBackupFileText = "Original Test File";
            var expectedInstalledText = "Installed Test File";
            var testPackagePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\testPackage.rockpkg";
            var targetVersion = "1.13.1";
            var currentVersion = "1.13.0";

            if ( File.Exists( testPackagePath ) )
            {
                File.Delete( testPackagePath );
            }

            var testBackupDirectory = $"{AppDomain.CurrentDomain.BaseDirectory}\\App_Data\\RockBackup\\{currentVersion}";
            if ( Directory.Exists( testBackupDirectory ) )
            {
                Directory.Delete( testBackupDirectory, true );
            }

            Directory.CreateDirectory( $"{AppDomain.CurrentDomain.BaseDirectory}\\test" );
            File.WriteAllText( $"{AppDomain.CurrentDomain.BaseDirectory}\\test\\test.txt", expectedBackupFileText );

            using ( var packageFileStream = new FileStream( testPackagePath, FileMode.OpenOrCreate ) )
            {
                using ( var testPackage = new ZipArchive( packageFileStream, ZipArchiveMode.Create ) )
                {
                    var testEntry = testPackage.CreateEntry( fileTarget );
                    using ( StreamWriter writer = new StreamWriter( testEntry.Open() ) )
                    {
                        writer.Write( expectedInstalledText );
                    }

                    testEntry = testPackage.CreateEntry( deleteTarget );
                    using ( StreamWriter writer = new StreamWriter( testEntry.Open() ) )
                    {
                        writer.Write( "test" );
                    }
                }
            }

            var releaseList = new List<RockRelease>
            {
                new RockRelease
                {
                    PackageUri = $"file://{testPackagePath}",
                    SemanticVersion = targetVersion
                }
            };

            var rockUpdateService = new Mock<IRockUpdateService>();

            rockUpdateService.Setup( rus => rus.GetReleasesList( It.IsAny<Version>() ) ).Returns( releaseList );

            var rockInstaller = new RockInstaller( rockUpdateService.Object, new Version( targetVersion ), new Version( currentVersion ) );
            var package = rockInstaller.InstallVersion();

            rockUpdateService.Verify( x => x.GetReleasesList( It.IsAny<Version>() ), Times.Once );

            // Validate backup file was created.
            var expectedBackupFilePath = $"{testBackupDirectory}\\test\\test.txt";
            Assert.IsTrue( File.Exists( expectedBackupFilePath ) );

            var actualBackupFileText = File.ReadAllText( expectedBackupFilePath );
            Assert.AreEqual( expectedBackupFileText, actualBackupFileText );

            // Validate new file was copied in.
            var expectedInstalledFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}\\test\\test.txt";
            Assert.IsFalse( File.Exists( expectedInstalledFilePath ) );
        }
    }
}
