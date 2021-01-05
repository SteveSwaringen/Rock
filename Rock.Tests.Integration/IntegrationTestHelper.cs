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
//
using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rock.Lava;
using Rock.Tests.Integration.Lava;

namespace Rock.Tests.Integration
{
    [TestClass()]
    public sealed class IntegrationTestHelper
    {
        public static LavaEngineTypeSpecifier EngineType { get; private set; }
        public static LavaTestHelper LavaTestHelper { get; private set; }

        /// <summary>
        /// This will run before any tests in this assembly are run.
        /// </summary>
        /// <param name="context">The context.</param>
        [AssemblyInitialize]
        public static void AssemblyInitialize( TestContext context )
        {
            LavaEngineTypeSpecifier engineType;

            var engineTypeName = context.Properties["LavaEngineType"].ToStringSafe();

            var isValid = Enum.TryParse( engineTypeName, out engineType );

            if ( !isValid )
            {
                engineType = LavaEngineTypeSpecifier.DotLiquid;

                Debug.Print( $"WARNING: The LavaEngineType setting is not specified in the current test configuration. Default value is set to \"{engineType}\"." );
            }

            EngineType = engineType;

            LavaTestHelper = LavaTestHelper.New( engineType );
        }

        /// <summary>
        /// This will run after all tests in this assembly are run.
        /// </summary>
        /// <param name="context">The context.</param>
        [AssemblyCleanup]
        public static void AssemblyCleanup()
        {
        }
    }
}