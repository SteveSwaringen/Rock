//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by the Rock.CodeGeneration project
//     Changes to this file will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
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
//

import Entity from '../Entity.js';
import AttributeValue from './AttributeValueViewModel.js';
import { Guid } from '../../Util/Guid.js';

export default interface Campus extends Entity {
    Id: number;
    Attributes: Record<string, AttributeValue> | null;
    CampusStatusValueId: number | null;
    CampusTypeValueId: number | null;
    Description: string | null;
    IsActive: boolean | null;
    IsSystem: boolean;
    LeaderPersonAliasId: number | null;
    LocationId: number | null;
    Name: string;
    Order: number;
    PhoneNumber: string | null;
    ServiceTimes: string | null;
    ShortCode: string | null;
    TeamGroupId: number | null;
    TimeZoneId: string | null;
    Url: string | null;
    CreatedDateTime: string | Date | null;
    ModifiedDateTime: string | Date | null;
    CreatedByPersonAliasId: number | null;
    ModifiedByPersonAliasId: number | null;
    Guid: Guid;
}
