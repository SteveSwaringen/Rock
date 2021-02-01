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

using System;

namespace Rock.ViewModel
{
    /// <summary>
    /// ContentChannelItem View Model
    /// </summary>
    [ViewModelOf( typeof( Rock.Model.ContentChannelItem ) )]
    public partial class ContentChannelItemViewModel : ViewModelBase
    {
        /// <summary>
        /// Gets or sets the ApprovedByPersonAliasId.
        /// </summary>
        /// <value>
        /// The ApprovedByPersonAliasId.
        /// </value>
        public int? ApprovedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the ApprovedDateTime.
        /// </summary>
        /// <value>
        /// The ApprovedDateTime.
        /// </value>
        public DateTime? ApprovedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Content.
        /// </summary>
        /// <value>
        /// The Content.
        /// </value>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the ContentChannelId.
        /// </summary>
        /// <value>
        /// The ContentChannelId.
        /// </value>
        public int ContentChannelId { get; set; }

        /// <summary>
        /// Gets or sets the ContentChannelTypeId.
        /// </summary>
        /// <value>
        /// The ContentChannelTypeId.
        /// </value>
        public int ContentChannelTypeId { get; set; }

        /// <summary>
        /// Gets or sets the ExpireDateTime.
        /// </summary>
        /// <value>
        /// The ExpireDateTime.
        /// </value>
        public DateTime? ExpireDateTime { get; set; }

        /// <summary>
        /// Gets or sets the ForeignGuid.
        /// </summary>
        /// <value>
        /// The ForeignGuid.
        /// </value>
        public Guid? ForeignGuid { get; set; }

        /// <summary>
        /// Gets or sets the ForeignKey.
        /// </summary>
        /// <value>
        /// The ForeignKey.
        /// </value>
        public string ForeignKey { get; set; }

        /// <summary>
        /// Gets or sets the ItemGlobalKey.
        /// </summary>
        /// <value>
        /// The ItemGlobalKey.
        /// </value>
        public string ItemGlobalKey { get; set; }

        /// <summary>
        /// Gets or sets the Order.
        /// </summary>
        /// <value>
        /// The Order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the Permalink.
        /// </summary>
        /// <value>
        /// The Permalink.
        /// </value>
        public string Permalink { get; set; }

        /// <summary>
        /// Gets or sets the Priority.
        /// </summary>
        /// <value>
        /// The Priority.
        /// </value>
        public int Priority { get; set; }

        /// <summary>
        /// Gets or sets the StartDateTime.
        /// </summary>
        /// <value>
        /// The StartDateTime.
        /// </value>
        public DateTime StartDateTime { get; set; }

        /// <summary>
        /// Gets or sets the Status.
        /// </summary>
        /// <value>
        /// The Status.
        /// </value>
        public int Status { get; set; }

        /// <summary>
        /// Gets or sets the StructuredContent.
        /// </summary>
        /// <value>
        /// The StructuredContent.
        /// </value>
        public string StructuredContent { get; set; }

        /// <summary>
        /// Gets or sets the Title.
        /// </summary>
        /// <value>
        /// The Title.
        /// </value>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the CreatedDateTime.
        /// </summary>
        /// <value>
        /// The CreatedDateTime.
        /// </value>
        public DateTime? CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the ModifiedDateTime.
        /// </summary>
        /// <value>
        /// The ModifiedDateTime.
        /// </value>
        public DateTime? ModifiedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the CreatedByPersonAliasId.
        /// </summary>
        /// <value>
        /// The CreatedByPersonAliasId.
        /// </value>
        public int? CreatedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the ModifiedByPersonAliasId.
        /// </summary>
        /// <value>
        /// The ModifiedByPersonAliasId.
        /// </value>
        public int? ModifiedByPersonAliasId { get; set; }

        /// <summary>
        /// Gets or sets the ForeignId.
        /// </summary>
        /// <value>
        /// The ForeignId.
        /// </value>
        public int? ForeignId { get; set; }

    }
}