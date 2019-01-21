﻿using System.Collections.Generic;
using System.Linq;
using Marr.Data.QGen;
using NzbDrone.Common.EnsureThat;
using NzbDrone.Core.Datastore;
using NzbDrone.Core.Messaging.Events;

namespace NzbDrone.Core.Music
{
    public interface IReleaseRepository : IBasicRepository<AlbumRelease>
    {
        List<AlbumRelease> FindByAlbum(int id);
        List<AlbumRelease> FindByRecordingId(List<string> recordingIds);
        List<AlbumRelease> SetMonitored(AlbumRelease release);
        List<AlbumRelease> FindByForeignReleaseId(List<string> foreignReleaseIds);
    }

    public class ReleaseRepository : BasicRepository<AlbumRelease>, IReleaseRepository
    {
        public ReleaseRepository(IMainDatabase database, IEventAggregator eventAggregator)
            : base(database, eventAggregator)
        {
        }

        public List<AlbumRelease> FindByAlbum(int id)
        {
            // populate the albums and artist metadata also
            // this hopefully speeds up the track matching a lot
            return Query
                .Join<AlbumRelease, Album>(JoinType.Left, r => r.Album.Value, (r, a) => r.AlbumId == a.Id)
                .Join<Album, ArtistMetadata>(JoinType.Left, a => a.ArtistMetadata.Value, (a, m) => a.ArtistMetadataId == m.Id)
                .Where<AlbumRelease>(r => r.AlbumId == id)
                .ToList();
        }

        public List<AlbumRelease> FindByForeignReleaseId(List<string> foreignReleaseIds)
        {
            var query = "SELECT AlbumReleases.*" +
                "FROM AlbumReleases " +
                $"WHERE AlbumReleases.ForeignReleaseId IN ('{string.Join("', '", foreignReleaseIds)}')";

            return Query.QueryText(query).ToList();
        }

        public List<AlbumRelease> FindByRecordingId(List<string> recordingIds)
        {
            var query = "SELECT DISTINCT AlbumReleases.*" +
                "FROM AlbumReleases " +
                "JOIN Tracks ON Tracks.AlbumReleaseId = AlbumReleases.Id " +
                $"WHERE Tracks.ForeignRecordingId IN ('{string.Join("', '", recordingIds)}')";

            return Query.QueryText(query).ToList();
        }

        public List<AlbumRelease> SetMonitored(AlbumRelease release)
        {
            var allReleases = FindByAlbum(release.AlbumId);
            allReleases.ForEach(r => r.Monitored = r.Id == release.Id);
            Ensure.That(allReleases.Count(x => x.Monitored) == 1).IsTrue();
            UpdateMany(allReleases);
            return allReleases;
        }
    }
}
