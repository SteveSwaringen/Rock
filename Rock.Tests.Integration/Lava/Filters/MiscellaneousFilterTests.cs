﻿// <copyright>
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

using DotLiquid;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Tests.Shared;
using Rock.Utility.Settings;

namespace Rock.Tests.Integration.Lava
{
    /// <summary>
    /// Tests for Lava Filters categorized as "Miscellaneous".
    /// </summary>
    /// <remarks>
    /// These tests require the standard Rock sample data set to be present in the target database.
    /// </remarks>
    [TestClass]
    public class MiscellaneousFilterTests
    {
        #region Debug

        [TestMethod]
        public void DebugFilter_WithLavaParameter_ReturnsDebugInfo()
        {
            var template = "{{ 'Lava' | Debug }}";

            var output = template.ResolveMergeFields( null );

            // Verify a portion of the expected output.
            Assert.That.Contains( output, "<li><span class='lava-debug-key'>OrganizationName</span> <span class='lava-debug-value'> - Rock Solid Church</span></li>" );
        }

        #endregion

        #region RockInstanceConfigFilter

        [TestMethod]
        public void RockInstanceConfigFilter_MachineName_RendersExpectedValue()
        {
            var template = "{{ 'MachineName' | RockInstanceConfig }}";
            var expectedValue = RockInstanceConfig.MachineName;

            var output = template.ResolveMergeFields( null );

            Assert.That.AreEqual( expectedValue, output );
        }

        [TestMethod]
        public void RockInstanceConfigFilter_ApplicationDirectory_RendersExpectedValue()
        {
            var template = "{{ 'ApplicationDirectory' | RockInstanceConfig }}";
            var expectedValue = RockInstanceConfig.ApplicationDirectory;

            var output = template.ResolveMergeFields( null );

            Assert.That.AreEqual( expectedValue, output );
        }

        [TestMethod]
        public void RockInstanceConfigFilter_PhysicalDirectory_RendersExpectedValue()
        {
            var template = "{{ 'PhysicalDirectory' | RockInstanceConfig }}";
            var expectedValue = RockInstanceConfig.PhysicalDirectory;

            var output = template.ResolveMergeFields( null );

            Assert.That.AreEqual( expectedValue, output );
        }

        [TestMethod]
        public void RockInstanceConfigFilter_IsClustered_RendersExpectedValue()
        {
            var template = "{{ 'IsClustered' | RockInstanceConfig }}";
            var expectedValue = RockInstanceConfig.IsClustered.ToTrueFalse();

            var output = template.ResolveMergeFields( null );

            Assert.That.AreEqual( expectedValue.ToLower(), output.ToLower() );
        }

        [TestMethod]
        public void RockInstanceConfigFilter_SystemDateTime_RendersExpectedValue()
        {
            var template = "{{ 'SystemDateTime' | RockInstanceConfig | Date:'yyyy-MM-dd HH:mm:ss' }}";
            var expectedValue = RockInstanceConfig.SystemDateTime;

            var output = template.ResolveMergeFields( null );

            var actualDateTime = output.AsDateTime();

            if ( actualDateTime == null )
            {
                throw new System.Exception( $"Invalid DateTime - Output = \"{output}\"" );
            }

            Assert.That.AreProximate( expectedValue, actualDateTime, new System.TimeSpan( 0, 0, 30 ) );
        }

        [TestMethod]
        public void RockInstanceConfigFilter_InvalidParameterName_RendersErrorMessage()
        {
            var template = "{{ 'unknown_setting' | RockInstanceConfig }}";

            var output = template.ResolveMergeFields( null );

            Assert.That.AreEqual( "Configuration setting \"unknown_setting\" is not available.", output );
        }

        #endregion
    }
}
