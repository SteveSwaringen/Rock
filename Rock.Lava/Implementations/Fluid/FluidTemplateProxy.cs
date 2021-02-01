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

namespace Rock.Lava.Fluid
{
    /// <summary>
    /// A wrapper for a Fluid Template for use with the Rock Lava library.
    /// </summary>
    /// <remarks>
    /// This class should exist in the Rock.Lava.Fluid library.
    /// </remarks>
    [Obsolete]
    public class FluidTemplateProxy : LavaTemplateBase
    {
        #region Constructors

        private LavaFluidTemplate _template;

        //public override ILavaEngine LavaEngine => throw new NotImplementedException();

        public FluidTemplateProxy( LavaFluidTemplate template )
        {
            _template = template;
        }

        #endregion

        public LavaFluidTemplate FluidTemplate
        {
            get
            {
                return _template;
            }
        }
    }
}
