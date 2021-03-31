using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using Rock.Data;

namespace Rock.Web.Cache.Entities
{
    /// <summary>
    /// Information about a named location that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class NamedLocationCache : ModelCache<NamedLocationCache, Rock.Model.Location>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets the campus identifier.
        /// </summary>
        /// <value>
        /// The campus identifier.
        /// </value>
        [DataMember]
        public int? CampusId
        {
            get
            {
                var campuses = CampusCache.All();

                int? campusId = null;

                var loc = this;

                while ( !campusId.HasValue && loc != null )
                {
                    var campus = campuses.Where( c => c.LocationId != null && c.LocationId == loc.Id ).FirstOrDefault();
                    if ( campus != null )
                    {
                        campusId = campus.Id;
                    }
                    else
                    {
                        loc = loc.ParentLocation;
                    }
                }

                return campusId;
            }
        }

        /// <summary>
        /// Gets the parent location identifier.
        /// </summary>
        /// <value>
        /// The parent location identifier.
        /// </value>
        [DataMember]
        public int? ParentLocationId { get; private set; }

        /// <summary>
        /// Gets the parent location.
        /// </summary>
        /// <value>
        /// The parent location.
        /// </value>
        public NamedLocationCache ParentLocation => this.ParentLocationId.HasValue ? NamedLocationCache.Get( ParentLocationId.Value ) : null;

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Gets the CampusID associated with the Location from the location or from the location's parent path
        /// </summary>
        /// <param name="locationId">The location identifier.</param>
        /// <returns></returns>
        public int? GetCampusIdForLocation( int? locationId )
        {
            if ( !locationId.HasValue )
            {
                return null;
            }

            var location = Get( locationId.Value );
            int? campusId = location.CampusId;
            if ( campusId.HasValue )
            {
                return campusId;
            }

            // If location is not a campus, check the location's parent locations to see if any of them are a campus
            var campusLocations = new Dictionary<int, int>();
            CampusCache.All()
                .Where( c => c.LocationId.HasValue )
                .Select( c => new
                {
                    CampusId = c.Id,
                    LocationId = c.LocationId.Value
                } )
                .ToList()
                .ForEach( c => campusLocations.Add( c.CampusId, c.LocationId ) );

            foreach ( var parentLocationId in GetAllAncestorIds() )
            {
                campusId = campusLocations
                    .Where( c => c.Value == parentLocationId )
                    .Select( c => c.Key )
                    .FirstOrDefault();

                if ( campusId != 0 )
                {
                    return campusId;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all ancestor ids.
        /// </summary>
        /// <returns></returns>
        private IEnumerable<int> GetAllAncestorIds()
        {
            var ancestorIds = new List<int>();
            var parentLocation = this.ParentLocation;
            while ( parentLocation != null )
            {
                ancestorIds.Add( parentLocation.Id );
                parentLocation = parentLocation.ParentLocation;
            }

            return ancestorIds;
        }

        /// <summary>
        /// The amount of time that this item will live in the cache before expiring. If null, then the
        /// default lifespan is used.
        /// </summary>
        public override TimeSpan? Lifespan
        {
            get
            {
                if ( Name.IsNullOrWhiteSpace() )
                {
                    return new TimeSpan( 0, 10, 0 );
                }

                return base.Lifespan;
            }
        }

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            Rock.Model.Location location = entity as Rock.Model.Location;
            if ( location == null )
            {
                return;
            }

            this.Name = location.Name;
            this.ParentLocationId = location.ParentLocationId;
        }

        /// <summary>
        /// returns <see cref="Name"/>
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.Name;
        }

        #endregion Public Methods
    }
}
